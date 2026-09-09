using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class TurnUIController : MonoBehaviour
{
    private GameController _controller;
    private NetworkPlayer _localPlayer;
    
    [SerializeField] private GameObject actionPanel;
    [SerializeField] private GameObject drawnCardPanel;
    [SerializeField] private GameObject redKingDecisionPanel;
    
    [SerializeField] private GameObject roundEndPanel;
    [SerializeField] private TextMeshProUGUI roundEndText;
    
    [SerializeField] private CardSlot drawnCardSlot;
    [SerializeField] private CardSlot deckSlot;
    [SerializeField] private CardSlot discardSlot;
    
    private List<PlayerGridPanel> playerGridPanels;
    [SerializeField] private GameObject usePowerButton;
    private SpecialAbility _currentAbility;

    private int _pendingTargetPlayerId = -1;
    private int _pendingTargetSlotIndex = -1;
    private int _pendingOwnSlotIndex;
    private ScoreBoard _scoreBoard = new ScoreBoard();

    private bool hasDrawnCardUIShown;
    private bool _redKingDecisionShown;
    private bool _roundEndShown;
    private bool _actionPanelActive;
    
    private Card _cachedDrawnCard;
    
    #region Lifecycle & Setup
    public void Awake()
    {
        actionPanel.SetActive(false);
        drawnCardPanel.SetActive(false);
        redKingDecisionPanel.SetActive(false);
        roundEndPanel.SetActive(false);
        roundEndText.text = "";
    }
    public void Initialize(GameController controller, List<PlayerGridPanel> panels)
    {
        _controller = controller;
        playerGridPanels = panels;
        Debug.Log("Intialize: actionPanel set inactive");
    }
    #endregion
    
    #region Network Snapshot Reaction
    public void SetLocalPlayer(NetworkPlayer player)
    {
        _localPlayer = player;
    }
    public void OnSnapshotReceived(NetworkGameController.BoardSnapshot snapshot)
    {
        DrawnCardSnapshot(snapshot);
        ActionPanelSnapshot(snapshot);
        RedKingSnapShot(snapshot);
        RoundEndSnapshot(snapshot);
        ResetPendingPowerStateOnNewRound(snapshot);
    }

    private void DrawnCardSnapshot(NetworkGameController.BoardSnapshot snapshot)
    {
        if (snapshot.DrawnCard.HasValue && !hasDrawnCardUIShown)
        {
            deckSlot.Clicked -= OnDeckClicked;
            discardSlot.Clicked -= OnDiscardClicked;
            drawnCardSlot.ShowCard(snapshot.DrawnCard.Value);
            _cachedDrawnCard = snapshot.DrawnCard.Value;
            actionPanel.SetActive(false);
            drawnCardSlot.Clicked += OnDrawnCardClicked;
            Debug.Log($"Drew: {snapshot.DrawnCard.Value}");
            hasDrawnCardUIShown = true;
        }
        else if (!snapshot.DrawnCard.HasValue && hasDrawnCardUIShown)
        {
            drawnCardSlot.Clicked -= OnDrawnCardClicked;
            drawnCardPanel.SetActive(false);
            drawnCardSlot.ShowEmpty();
            hasDrawnCardUIShown = false;
        }
    }

    private void ActionPanelSnapshot(NetworkGameController.BoardSnapshot snapshot)
    {
        if (_localPlayer == null)
        {
            // First snapshot(s) can arrive before WaitForTableViewAndSetLocalPlayer finishes.
            return;
        }

        var isMyTurnIdle = !snapshot.DrawnCard.HasValue
            && snapshot.Players[snapshot.CurrentPlayerIndex].PlayerId == _localPlayer.SeatIndex
            && snapshot.GamePhases != GamePhase.InitialPeek
            && snapshot.GamePhases != GamePhase.RoundEnded;

        if (isMyTurnIdle && !_actionPanelActive)
        {
            EnterWaitingForAction();
            _actionPanelActive = true;
        }
        else if (!isMyTurnIdle && _actionPanelActive)
        {
            actionPanel.SetActive(false);
            deckSlot.Clicked -= OnDeckClicked;
            discardSlot.Clicked -= OnDiscardClicked;
            _actionPanelActive = false;
        }
    }

    private void RoundEndSnapshot(NetworkGameController.BoardSnapshot snapshot)
    {
        if (snapshot.GamePhases == GamePhase.RoundEnded && !_roundEndShown)
        {
            _roundEndShown = true;
            ShowRoundEnd(snapshot);
        }
        else if (snapshot.GamePhases == GamePhase.InitialPeek && _roundEndShown)
        {
            _roundEndShown = false;
            roundEndPanel.SetActive(false);
        }
    }

    private void ResetPendingPowerStateOnNewRound(NetworkGameController.BoardSnapshot snapshot)
    {
        if (snapshot.GamePhases == GamePhase.InitialPeek)
        {
            _pendingTargetPlayerId = -1;
            _pendingTargetSlotIndex = -1;
            _redKingDecisionShown = false;
            redKingDecisionPanel.SetActive(false);
        }
    }
    private void RedKingSnapShot(NetworkGameController.BoardSnapshot snapshot)
    {
        if (snapshot.PendingRedKingDecision && !_redKingDecisionShown)
        {
            Debug.Log("RedKingSnapShot: showing decision panel (server-confirmed pending decision)");
            redKingDecisionPanel.SetActive(true);
            _redKingDecisionShown = true;
        }
        else if (!snapshot.PendingRedKingDecision && _redKingDecisionShown)
        {
            Debug.Log("RedKingSnapShot: hiding decision panel");
            redKingDecisionPanel.SetActive(false);
            _redKingDecisionShown = false;
        }
    }

    #endregion
    
    #region Idle State
    private void EnterWaitingForAction()
    {
        actionPanel.SetActive(true);
        deckSlot.Clicked += OnDeckClicked;
        discardSlot.Clicked += OnDiscardClicked;
        drawnCardSlot.ShowEmpty();

        if (_controller != null)
        {
            var current = _controller.State.Players[_controller.State.CurrentPlayerIndex];
            Debug.Log($"Now {current.PlayerName}'s turn (index {_controller.State.CurrentPlayerIndex})");
        }
    }
    public void ShowActions()
    {
        EnterWaitingForAction();
        Debug.Log("ShowActions called");
    }
    #endregion
    
    #region Draw
    public void Draw()
    {
        _localPlayer.CmdDraw();
    }
    private void OnDeckClicked(CardSlot slot)
    {
        Draw();
    }
    #endregion
    
    #region Discard / Resolve Drawn Card
    public void Discard()
    {
        _localPlayer.CmdDiscard();
    }
    private void OnDrawnCardClicked(CardSlot slot)
    {
        drawnCardSlot.Clicked -= OnDrawnCardClicked;
        drawnCardPanel.SetActive(true);

        _currentAbility = SpecialAbilityResolver.GetAbility(_cachedDrawnCard);
        usePowerButton.SetActive(_currentAbility != SpecialAbility.None);
    }
    #endregion
    
    #region Swap From Draw
    public void StartSwap()
    {
        drawnCardPanel.SetActive(false);

        var currentPlayerId = _localPlayer.SeatIndex;
        foreach (var slot in GetOwnSlots(currentPlayerId))
        {
            slot.Clicked += OnSwapSlotClicked;
        }
    }
    private void OnSwapSlotClicked(CardSlot slot)
    {
        var currentPlayer = _localPlayer.SeatIndex;
        foreach (var slots in GetOwnSlots(currentPlayer))
        {
            slots.Clicked -= OnSwapSlotClicked;
        }
        
        _localPlayer.CmdSwap(slot.SlotIndex);
    }
    #endregion
    
    #region Swap From Discard
    private void OnDiscardClicked(CardSlot slot)
    {
        var currentPlayer = _localPlayer.SeatIndex;
        deckSlot.Clicked -= OnDeckClicked;
        discardSlot.Clicked -= OnDiscardClicked;
        actionPanel.SetActive(false);
        _actionPanelActive = false;
        
        Debug.Log("Discard picked up - choose a slot to swap it into");
        
        foreach (var slots in GetOwnSlots(currentPlayer))
        {
            slots.Clicked += OnSwapFromDiscardSlotClicked;
        }
    }
    private void OnSwapFromDiscardSlotClicked(CardSlot slot)
    {
        var currentPlayer = _localPlayer.SeatIndex;
        foreach (var slots in GetOwnSlots(currentPlayer))
        {
            slots.Clicked -= OnSwapFromDiscardSlotClicked;
        }
        _localPlayer.CmdSwapFromDiscard(slot.SlotIndex);
    }
    #endregion
    
    #region Powers - Dispatch
    public void UsePower()
    { 
        switch (_currentAbility)
        {
            case SpecialAbility.SkipNext:
                _localPlayer.CmdSkipNext();
                break;
            
            case SpecialAbility.PeekOwn:
                StartPeekOwn();
                break;
            
            case SpecialAbility.PeekTarget:
                StartPeekTarget();
                break;
            
            case SpecialAbility.BlindSwap:
                StartBlindSwap();
                break;
            
            case SpecialAbility.LookAndSwap:
                StartLookAndSwap();
                break;
            
            default:
                Debug.Log($"{_currentAbility} not implemented");
                break;
        }
    }
    #endregion
    
    #region Powers - Peek Own
    private void StartPeekOwn()
    {
        drawnCardPanel.SetActive(false);
        var currentPlayerId = _localPlayer.SeatIndex;
        foreach (var slot in GetOwnSlots(currentPlayerId))
        {
            slot.Clicked += OnPeekOwnSlotClicked;
        }
    }
    private void OnPeekOwnSlotClicked(CardSlot slot)
    {
        var currentPlayer = _localPlayer.SeatIndex;
        foreach (var slots in GetOwnSlots(currentPlayer))
        {
            slots.Clicked -= OnPeekOwnSlotClicked;
        } 
        _localPlayer.CmdPeekOwn(slot.SlotIndex);
    }
    #endregion
    
    #region Powers - Peek Target
    private void StartPeekTarget()
    {
        drawnCardPanel.SetActive(false);
        var currentPlayerId = _localPlayer.SeatIndex;
        foreach (var slot in GetTargetSlots(currentPlayerId))
        {
            slot.Clicked += OnPeekTargetSlotClicked;
        }
    }
    private void OnPeekTargetSlotClicked(CardSlot slot)
    {
        var currentPlayer = _localPlayer.SeatIndex;
        foreach (var slots in GetTargetSlots(currentPlayer))
        { 
            slots.Clicked -= OnPeekTargetSlotClicked;
        }
        _localPlayer.CmdPeekTarget(slot.SlotIndex, slot.PlayerId);
    }
    #endregion

    #region Powers - Blind Swap
    private void StartBlindSwap()
    {
        drawnCardPanel.SetActive(false);
        var currentPlayerId = _localPlayer.SeatIndex;
        foreach (var slot in GetTargetSlots(currentPlayerId))
        {
            slot.Clicked += OnBlindSwapTargetClicked;
        }
    }
    private void OnBlindSwapTargetClicked(CardSlot slot)
    {
        var currentPlayerId = _localPlayer.SeatIndex;
        foreach (var slots in GetOwnSlots(currentPlayerId))
        {
            slots.Clicked += OnBlindSwapOwnSlotClicked;
        }

        foreach (var slots in GetTargetSlots(currentPlayerId))
        {
            slots.Clicked -= OnBlindSwapTargetClicked;
        }
        _pendingTargetPlayerId = slot.PlayerId;
        _pendingTargetSlotIndex = slot.SlotIndex;
    }
    private void OnBlindSwapOwnSlotClicked(CardSlot slot)
    {
        var currentPlayerId = _localPlayer.SeatIndex;
        foreach (var slots in GetOwnSlots(currentPlayerId))
        {
            slots.Clicked -= OnBlindSwapOwnSlotClicked;
        }

        _localPlayer.CmdBlindSwap(slot.SlotIndex, _pendingTargetPlayerId, _pendingTargetSlotIndex);
        _pendingTargetPlayerId = -1;
        _pendingTargetSlotIndex = -1;
    }
    #endregion
    
    #region Powers - Look And Swap
    private void StartLookAndSwap()
    {
        drawnCardPanel.SetActive(false);
        var currentPlayerId = _localPlayer.SeatIndex;
        foreach (var slot in GetOwnSlots(currentPlayerId))
        {
            slot.Clicked += OnLookAndSwapOwnSlotClicked;
        }
    }
    private void OnLookAndSwapTargetClicked(CardSlot slot)
    {
        var currentPlayerId = _localPlayer.SeatIndex;
        foreach (var slots in GetTargetSlots(currentPlayerId))
        {
            slots.Clicked -= OnLookAndSwapTargetClicked;
        }
        _pendingTargetPlayerId = slot.PlayerId;
        _pendingTargetSlotIndex = slot.SlotIndex;
        _localPlayer.CmdLookAndSwapPeekTarget(_pendingTargetSlotIndex, _pendingTargetPlayerId);
    }
    private void OnLookAndSwapOwnSlotClicked(CardSlot slot)
    {
        var currentPlayerId = _localPlayer.SeatIndex;
        foreach (var slots in GetOwnSlots(currentPlayerId))
        {
            slots.Clicked -= OnLookAndSwapOwnSlotClicked;
        }
        _pendingOwnSlotIndex = slot.SlotIndex;
        _localPlayer.CmdLookAndSwapPeekOwn(_pendingOwnSlotIndex);
        foreach (var slots in GetTargetSlots(currentPlayerId))
        {
            slots.Clicked += OnLookAndSwapTargetClicked;
        }
    }
    public void RedKingSwapNo()
    {
        redKingDecisionPanel.SetActive(false);
        _localPlayer.CmdRedKingDecision(false, _pendingOwnSlotIndex);
        _pendingTargetPlayerId = -1;
        _pendingTargetSlotIndex = -1;
    }
    public void RedKingSwapYes()
    {
        redKingDecisionPanel.SetActive(false);
       _localPlayer.CmdRedKingDecision(true, _pendingOwnSlotIndex);
       _pendingTargetPlayerId = -1;
       _pendingTargetSlotIndex = -1;
    }
    #endregion
    
    #region Round End
    public void CallCabboo()
    {
        _localPlayer.CmdCallCabboo();
    }
    private void ShowRoundEnd(NetworkGameController.BoardSnapshot snapshot)
    {
        actionPanel.SetActive(false);
        deckSlot.Clicked -= OnDeckClicked;
        discardSlot.Clicked -= OnDiscardClicked;
        roundEndPanel.SetActive(true);
        var roundScorer = snapshot.RoundResults;
        
        _scoreBoard.ApplyResultScore(roundScorer.ToList());
        
        StringBuilder roundEndBuilder = new StringBuilder();
        
        foreach (var result in roundScorer)
        {
            var players = Array.Find(snapshot.Players, p => p.PlayerId == result.PlayerId);
            roundEndBuilder.AppendLine($"{players.PlayerName} {players.PlayerId}: Raw: {result.RawScore} Pts: {result.PointsAwarded} Total: {_scoreBoard.GetTotalScore(result.PlayerId)} {(result.IsCabbooCaller ? "(Called Cabboo)" : "")}"); 
        }
        roundEndText.text = roundEndBuilder.ToString();
    }
    public void NewRound()
    {
        roundEndPanel.SetActive(false);
        _localPlayer.CmdNewRound();
    }
    #endregion
    
    #region Helpers
    private IEnumerable<CardSlot> GetOwnSlots(int playerId)
    {
        foreach (var panel in playerGridPanels)
        {
            foreach (var slot in panel.CardSlots)
            {
                if (slot.PlayerId == playerId)
                {
                    yield return slot;
                }
            }
        }
    }
    private IEnumerable<CardSlot> GetTargetSlots(int playerId)
    {
        foreach (var panel in playerGridPanels)
        {
            foreach (var slot in panel.CardSlots)
            {
                if (slot.PlayerId != playerId)
                { 
                    yield return slot;
                }
            }
        }
    }

    public void SetPlayerGridPanels(List<PlayerGridPanel> panels)
    {
        playerGridPanels = panels;
    }

    #endregion
}