using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class NetworkGameController : NetworkBehaviour
{
    private GameController _gameController;
    
    public struct SlotView
    {
        public Card? Card;   // null = hidden from this recipient
    }

    public struct PlayerView
    {
        public int PlayerId;
        public string PlayerName;
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
    }

    public void Initialize(GameController gameController)
    {
        _gameController = gameController;
        TableView.Instance.SetGameController(gameController);
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
                if (slotIndex == 2 || slotIndex == 3)
                {
                    Debug.Log($"BuildSnapshotFor recipient={recipientPlayerId} owner={serverPlayer.PlayerId} slot={slotIndex} revealed={isRevealedToRecipient}");
                }
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
        return snapshot;
    }

    public void BroadcastSnapshots()
    {
        foreach (var conn in NetworkServer.connections.Values)
        {
            Debug.Log($"BroadcastSnapshots iterating connection {conn.connectionId}");
            NetworkIdentity networkIdentity = conn.identity;
            if (networkIdentity != null)
            {
                if (networkIdentity.TryGetComponent<NetworkPlayer>(out var player))
                {
                    int seatIndex = player.SeatIndex;
                    TargetReceiveSnapshot(conn, BuildSnapshotFor(seatIndex));
                }
            }
            else
            {
                Debug.Log($"Skipped broadcast — null identity for connection {conn.connectionId}");
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
        yield return new WaitUntil(() => NetworkServer.connections.Values.All(c => c.identity != null));
        BroadcastSnapshots();
        for (int i = 0; i < _gameController.State.Players.Count; i++)
        {
            var peek2 = new PeekOwnCardCommand { SlotIndex = 2 };
            _gameController.TryExecute(peek2,  _gameController.State.Players[i].PlayerId);
            var peek3 = new PeekOwnCardCommand { SlotIndex = 3 };
            _gameController.TryExecute(peek3,  _gameController.State.Players[i].PlayerId);
        }
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
}