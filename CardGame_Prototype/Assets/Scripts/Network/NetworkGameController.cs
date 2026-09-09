using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class NetworkGameController : NetworkBehaviour
{
    private GameController _gameController; 
    public GameController GameController => _gameController;
    private RoundResult[] _cachedRoundResults;
    private Room _room;
    public Room room => _room;
    public struct SlotView
    {
        public Card? Card;   // null = hidden from this recipient
    }

    public struct PlayerView
    {
        public string PlayerName;
        public int PlayerId;
        public SlotView[] Slots;   // length 4
    }

    public struct BoardSnapshot
    {
        public int CurrentPlayerIndex;
        public int DeckCount;
        public Card DiscardTop;      // always public — but what if the pile's empty? worth a nullable here too
        public Card? DrawnCard;      // null unless this snapshot's recipient is the one who drew it
        public PlayerView[] Players;
        public GamePhase GamePhases;
        public RoundResult[] RoundResults;
        public bool PendingRedKingDecision;
    }

    public void Initialize(GameController gameController, Room room)
    {
        _gameController = gameController;
        TableView.Instance.SetGameController(gameController);
        _room = room;
        _gameController.StateChanged += BroadcastSnapshots;
        StartCoroutine(RunInitialPeekPhase());
    }

    public BoardSnapshot BuildSnapshotFor(int recipientPlayerId)
    {
        
        var gameState = _gameController.State;
        var snapshot = new BoardSnapshot();
        
        snapshot.GamePhases = gameState.GamePhases;
        snapshot.CurrentPlayerIndex = gameState.CurrentPlayerIndex;
        snapshot.DeckCount = gameState.Deck.DrawPileCount;
        snapshot.DiscardTop = gameState.Deck.GetDiscardTop();
        
        if (recipientPlayerId == gameState.Players[gameState.CurrentPlayerIndex].PlayerId)
        {
            snapshot.DrawnCard = gameState.PendingDrawnCard;
        }
        
        snapshot.Players = new PlayerView[_gameController.State.Players.Count];
        
        for (int i = 0; i < _gameController.State.Players.Count; i++)
        {
            var serverPlayer = _gameController.State.Players[i];
            
            var playerView = new PlayerView
            {
                PlayerId = serverPlayer.PlayerId,
                PlayerName = serverPlayer.PlayerName,
                Slots = new SlotView[4] // Initialize the fixed length-4 array
            };

            for (int slotIndex = 0; slotIndex < 4; slotIndex++)
            {
                var serverSlot = serverPlayer.Slots[slotIndex];
                
                bool isRevealedToRecipient =
                    gameState.RevealedTo[serverPlayer.PlayerId][slotIndex] == recipientPlayerId;
                var slotView = new SlotView();
                if (isRevealedToRecipient)
                {
                    slotView.Card = serverSlot;
                }
                else
                {
                    slotView.Card = null;
                }
                
                playerView.Slots[slotIndex] = slotView;
            }
            snapshot.Players[i] = playerView;
        }
        
        snapshot.RoundResults = _cachedRoundResults;

        snapshot.PendingRedKingDecision = gameState.PendingLookAndSwap != null
            && recipientPlayerId == gameState.Players[gameState.CurrentPlayerIndex].PlayerId;

        return snapshot;
    }

    public void BroadcastSnapshots()
    {
        if (_cachedRoundResults == null && _gameController.State.GamePhases == GamePhase.RoundEnded)
        {
            _cachedRoundResults = RoundScorer.ScoreRound(_gameController.State).ToArray();
        }
        
        foreach (var conn in _room.connections)
        {
            NetworkIdentity networkIdentity = conn.identity;
            if (networkIdentity != null)
            {
                if (networkIdentity.TryGetComponent<NetworkPlayer>(out var player))
                {
                    int seatIndex = player.SeatIndex;
                    TargetReceiveSnapshot(conn, BuildSnapshotFor(seatIndex));
                }
            }
        }
    }
    
    
    [TargetRpc]
    private void TargetReceiveSnapshot(NetworkConnectionToClient target, BoardSnapshot snapshot)
    {
        TableView.Instance.RenderAll(snapshot);
    }

    public IEnumerator RunInitialPeekPhase()
    {
        Debug.Log("Running InitialPeekPhase");
        yield return new WaitUntil(() => _room.connections.All(c => c.identity != null));
        
        for (int i = 0; i < _gameController.State.Players.Count; i++)
        {
            var peek2 = new PeekOwnCardCommand { SlotIndex = 2 };
            _gameController.TryExecute(peek2,  _gameController.State.Players[i].PlayerId);
            var peek3 = new PeekOwnCardCommand { SlotIndex = 3 };
            _gameController.TryExecute(peek3,  _gameController.State.Players[i].PlayerId);
        }
        BroadcastSnapshots();
        yield return new WaitForSeconds(5f);
        for (int i = 0; i < _gameController.State.Players.Count; i++)
        {
            var serverPlayer = _gameController.State.Players[i];
            _gameController.State.RevealedTo[serverPlayer.PlayerId][2] = null;
            _gameController.State.RevealedTo[serverPlayer.PlayerId][3] = null;
        }
        _gameController.State.GamePhases = GamePhase.InProgress;
        BroadcastSnapshots();
        Debug.Log("Ran InitialPeekPhase");
    }

    public void RevealTemporarily(int playerId, int slotIndex, float duration)
    {
        Debug.Log($"RevealTemporarily: START playerId={playerId} slot={slotIndex} duration={duration}");
        StartCoroutine(RevealTemporarilyCoroutine(playerId, slotIndex, duration));
    }
    private IEnumerator RevealTemporarilyCoroutine(int playerId, int slotIndex, float duration)
    {
        yield return new WaitForSeconds(duration);
        _gameController.State.RevealedTo[playerId][slotIndex] = null;
        BroadcastSnapshots();
        Debug.Log($"RevealTemporarily: CLEARED playerId={playerId} slot={slotIndex}");
    }

    public void BeginNewRound(int numPlayers)
    {
        _cachedRoundResults = null;
        _gameController.StartNewRound(numPlayers);
        StartCoroutine(RunInitialPeekPhase());
    }
}