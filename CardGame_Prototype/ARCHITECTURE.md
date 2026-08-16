# Cabboo — Architecture

## Stack
- Unity 6000.1.13f1, URP 2D
- Multiplayer: Mirror Networking (free, OSS), self-hosted dedicated server (can be a headless Linux build). WebGL clients connect via Mirror's SimpleWebTransport (WebSocket). Server and client share the same C# codebase.
- Animation: DOTween
- Input: new Input System

## Why server-authoritative
Cabo/Cabboo is a hidden-information game (you can't see your own top two cards, opponents can't see any of yours, several special powers are "peek without revealing"). A client can never be trusted with the full deck/hand state — it must live only on the server, which sends each client just the slice of state it's allowed to see.

## Layers
1. **Core** (`Assets/Scripts/Core`, asmdef `Cabboo.Core`) — plain C#, no MonoBehaviours: `Card`, `Deck`, `PlayerState`, `GameState`, turn state machine, command objects (`DrawCard`, `SwapFromDeck`, `DiscardDrawn`, `SwapFromDiscard`, `CallCabboo`, `UseSpecial`), `ScoreCalculator`. Fully unit-testable (Edit Mode tests) and reusable unmodified on the dedicated server later.
2. **Network** (`Assets/Scripts/Network`) — Mirror `NetworkBehaviour`s/messages wrapping Core: server receives commands, validates via Core, mutates the authoritative `GameState`, and pushes each client a filtered view. *Not built yet — Phase 3.*
3. **Presentation** (`Assets/Scripts/UI`) — MonoBehaviours (`GridManager`, `CardSlot`, `PlayerGridPanel`, a new `ScoreboardView`) that render a state snapshot and forward player input as Core commands. No game rules live here.
4. **Data** (`Assets/Scripts/Cards`, `Resources/Cards`) — `CardDefinition` ScriptableObjects (identity + art only).

## Fixes needed in the existing scaffold
- `CardDefinition.rank` currently doubles as score, but scoring isn't a pure function of rank: Red King = 13, Black King = 0, Joker = -1. Score must be computed from `(suit, rank)` in Core's `ScoreCalculator`, not stored on the asset.
- `Packages/manifest.json` and `packages-lock.json` are currently git-ignored — recommend un-ignoring them so package versions (Mirror, DOTween, etc.) are reproducible for anyone else who clones the repo.
- `GridManager` currently just spawns empty slots; it'll be repointed to render from Core `GameState` in Phase 2.

## Performance & Asset Loading (WebGL)
Browsers (mobile Safari especially) hard-cap tab memory, so this is scoped deliberately, not left to chance:
- **Sprite Atlases**: all 54 card faces + back + special icons packed into 1–2 atlases — fewer HTTP requests, fewer draw calls, automatic batching.
- **Small textures**: card art capped at ~128×192px import size, mipmaps off (2D, no depth) — compressed texture format, not raw RGBA32.
- **Lazy-load faces**: most cards are face-down for most of the game, so only the shared "card back" sprite ships in the initial build; unique face sprites stream in via Addressables only when a card is actually revealed.
- **Canvas splitting**: static UI chrome vs. frequently-updated elements (scoreboard, turn indicator) on separate Canvases, so a scoreboard tick doesn't re-batch the whole UI.
- **Pooling**: card slot/view GameObjects are pooled and reused, not instantiate/destroy'd each turn, to avoid GC hitches (WebGL runs on the browser's single main thread — no background GC thread to hide pauses).
- **Build settings**: Brotli/gzip compression enabled for the WebGL build; WASM heap (initial/max memory) tuned against Unity's Memory Profiler once real art exists, not guessed upfront.

This becomes concrete in Phase 2, once real card/board/UI art is in the project to actually profile.

## Phase plan
1. **Core rules engine + local hotseat** *(next)* — build the domain model above, unit-test it, and drive it from a temporary single-screen pass-and-play harness. No networking yet.
2. **Presentation wiring** — bind existing prefabs + a new scoreboard to Core via events, still hotseat.
3. **Mirror networking** — dedicated server authority, SimpleWebTransport for WebGL, swap the hotseat harness for real per-player sessions (2–5, upscalable).
4. **Scorecard persistence, lobby/matchmaking, polish.**
