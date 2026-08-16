using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Random = System.Random;

public enum TurnUIState
{
    WaitingForAction,        // show Draw / Call Cabboo
    WaitingForSwapSlot, // player must click one of their own 4 slots
    WaitingForPlayerChoice,
    WaitingForPowerTarget,   // player must click an opponent's card
    WaitingForOwnSlotForPower, // e.g. BlindSwap's second click
    WaitingForRedKingDecision  // swap y/n after LookAndSwap peek
}

public class TurnUIController : MonoBehaviour
{
    private GameController _controller;
    [SerializeField] private TurnUIState _state;
    
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
    
    private int _pendingTargetPlayerId;
    private int _pendingTargetSlotIndex;
    private int _pendingOwnSlotIndex;
    private ScoreBoard _scoreBoard = new ScoreBoard();

    public void Initialize(GameController controller, List<PlayerGridPanel> panels)
    {
        _controller = controller;
        playerGridPanels = panels;
        actionPanel.SetActive(false);
        drawnCardPanel.SetActive(false);
        redKingDecisionPanel.SetActive(false);
        roundEndPanel.SetActive(false);
        roundEndText.text = "";
        Debug.Log("Intialize: actionPanel set inactive");
    }

    public void Draw()
    {
        var currentPlayer = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        if (!_controller.TryDrawCard(currentPlayer))
        {
            Debug.Log("Cannot draw card right now");
            return;
        }

        deckSlot.Clicked -= OnDeckClicked;
        discardSlot.Clicked -= OnDiscardClicked;
        _state = TurnUIState.WaitingForPlayerChoice;
        drawnCardSlot.ShowCard(_controller.State.PendingDrawnCard.Value);
        actionPanel.SetActive(false);
        drawnCardSlot.Clicked += OnDrawnCardClicked;
        // drawnCardPanel.SetActive(true);
        Debug.Log($"Drew: {_controller.State.PendingDrawnCard.Value}");
    }

    private void OnDrawnCardClicked(CardSlot slot)
    {
        drawnCardSlot.Clicked -= OnDrawnCardClicked;
        drawnCardPanel.SetActive(true);

        _currentAbility = SpecialAbilityResolver.GetAbility(_controller.State.PendingDrawnCard.Value);
        usePowerButton.SetActive(_currentAbility != SpecialAbility.None);
    }

    public void StartSwap()
    {
        drawnCardPanel.SetActive(false);
        _state = TurnUIState.WaitingForSwapSlot;
        
        var currentPlayerId = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        foreach (var slot in GetOwnSlots(currentPlayerId))
        {
            slot.Clicked += OnSwapSlotClicked;
        }
    }
    
    private void StartPeekOwn()
    {
        drawnCardPanel.SetActive(false);
        _state = TurnUIState.WaitingForOwnSlotForPower;
        var currentPlayerId = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        foreach (var slot in GetOwnSlots(currentPlayerId))
        {
            slot.Clicked += OnPeekOwnSlotClicked;
        }
    }

    private void StartPeekTarget()
    {
        drawnCardPanel.SetActive(false);
        _state = TurnUIState.WaitingForPowerTarget;
        var currentPlayerId = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        foreach (var slot in GetTargetSlots(currentPlayerId))
        {
            slot.Clicked += OnPeekTargetSlotClicked;
        }
    }

    private void StartBlindSwap()
    {
        drawnCardPanel.SetActive(false);
        _state = TurnUIState.WaitingForPowerTarget;
        var currentPlayerId = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        foreach (var slot in GetTargetSlots(currentPlayerId))
        {
            slot.Clicked += OnBlindSwapTargetClicked;
        }
    }

    private void StartLookAndSwap()
    {
        drawnCardPanel.SetActive(false);
        _state = TurnUIState.WaitingForPowerTarget;
        var currentPlayerId = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        foreach (var slot in GetOwnSlots(currentPlayerId))
        {
            slot.Clicked += OnLookAndSwapOwnSlotClicked;
        }
    }

    private void OnLookAndSwapOwnSlotClicked(CardSlot slot)
    {
        var currentPlayerId = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        foreach (var slots in GetOwnSlots(currentPlayerId))
        {
            slots.Clicked -= OnLookAndSwapOwnSlotClicked;
        }
        _pendingOwnSlotIndex = slot.SlotIndex;
        slot.FlashReveal(_controller.State.Players.Find(
            p => p.PlayerId == currentPlayerId).Slots[slot.SlotIndex], 5f);

        foreach (var slots in GetTargetSlots(currentPlayerId))
        {
            slots.Clicked += OnLookAndSwapTargetClicked;
        }
    }

    private void OnLookAndSwapTargetClicked(CardSlot slot)
    {
        var currentPlayerId = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        foreach (var slots in GetTargetSlots(currentPlayerId))
        {
            slots.Clicked -= OnLookAndSwapTargetClicked;
        }

        PeekOpponentCardCommand lookInner;
        RedKingLookCommand lookPower;
        lookInner = new PeekOpponentCardCommand { TargetPlayerId = slot.PlayerId, SlotIndex =  slot.SlotIndex };
        lookPower = new RedKingLookCommand {LookCommand =  lookInner};
        if (_controller.TryExecute(lookPower, currentPlayerId))
        {
            slot.FlashReveal(lookInner.RevealedCard, 5f);
            redKingDecisionPanel.SetActive(true);
            _state = TurnUIState.WaitingForRedKingDecision;
        }

    }

    public void RedKingSwapNo()
    {
        var currentPlayerId = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        redKingDecisionPanel.SetActive(false);
        RedKingDecisionCommand redKingDecisionCommand = new RedKingDecisionCommand
        {
            ShouldSwap = false
        };
        if (_controller.TryExecute(redKingDecisionCommand, currentPlayerId))
        {
            Debug.Log("LookAndSwap: declined swap");
            EnterWaitingForAction();
        }
    }

    public void RedKingSwapYes()
    {
        var currentPlayerId = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        redKingDecisionPanel.SetActive(false);
        RedKingDecisionCommand redKingDecisionCommand = new RedKingDecisionCommand
        {
            ShouldSwap = true,
            OwnSlotIndex = _pendingOwnSlotIndex
        };
        if (_controller.TryExecute(redKingDecisionCommand, currentPlayerId))
        {
            EnterWaitingForAction();
            Debug.Log($"LookAndSwap: swapped your slot {_pendingOwnSlotIndex}");
        }
    }

    // private void OnRedKingOwnSlotClicked(CardSlot slot)
    // {
    //     var currentPlayerId = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
    //     foreach (var slots in GetOwnSlots(currentPlayerId))
    //     {
    //         slots.Clicked -= OnRedKingOwnSlotClicked;
    //     }
    //     RedKingDecisionCommand redKingDecisionCommand = new RedKingDecisionCommand
    //     {
    //         ShouldSwap = true,
    //         OwnSlotIndex = slot.SlotIndex
    //     };
    //     if (_controller.TryExecute(redKingDecisionCommand, currentPlayerId))
    //     {
    //         EnterWaitingForAction();
    //     }
    // }

    private void OnBlindSwapTargetClicked(CardSlot slot)
    {
        var currentPlayerId = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        foreach (var slots in GetOwnSlots(currentPlayerId))
        {
            slots.Clicked += OnBlindSwapOwnSlotClicked;
        }

        foreach (var slots in GetTargetSlots(currentPlayerId))
        {
            slots.Clicked -= OnBlindSwapTargetClicked;
        }
        _state = TurnUIState.WaitingForOwnSlotForPower;
        _pendingTargetPlayerId = slot.PlayerId;
        _pendingTargetSlotIndex = slot.SlotIndex;
    }

    private void OnBlindSwapOwnSlotClicked(CardSlot slot)
    {
        var currentPlayerId = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        foreach (var slots in GetOwnSlots(currentPlayerId))
        {
            slots.Clicked -= OnBlindSwapOwnSlotClicked;
        }

        SwapWithPlayerCommand swapWithPlayerCommand;
        UseSpecialPowerCommand swapwithPlayerPower;
        
        swapWithPlayerCommand = new SwapWithPlayerCommand {OwnSlotIndex= slot.SlotIndex, TargetPlayerId = _pendingTargetPlayerId, TargetSlotIndex = _pendingTargetSlotIndex};
        swapwithPlayerPower = new UseSpecialPowerCommand { PowerCommand = swapWithPlayerCommand };
        if (_controller.TryExecute(swapwithPlayerPower, currentPlayerId))
        {
            Debug.Log($"BlindSwap: your slot {slot.SlotIndex} <-> Player {_pendingTargetPlayerId}'s slot {_pendingTargetSlotIndex}");
            EnterWaitingForAction();
        }
    }

    private void OnPeekOwnSlotClicked(CardSlot slot)
    {
        var currentPlayer = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        foreach (var slots in GetOwnSlots(currentPlayer))
        {
            slots.Clicked -= OnPeekOwnSlotClicked;
        }
        PeekOwnCardCommand peekOwnCommand;
        UseSpecialPowerCommand peekOwnPower;
        peekOwnCommand = new PeekOwnCardCommand { SlotIndex = slot.SlotIndex };
        peekOwnPower = new UseSpecialPowerCommand { PowerCommand = peekOwnCommand };
        if(_controller.TryExecute(peekOwnPower ,currentPlayer))
        {
            slot.FlashReveal(peekOwnCommand.RevealedCard, 5f);
            Debug.Log($"PeekOwn: {slot.SlotIndex}");
            EnterWaitingForAction();
        }
    }

    private void OnPeekTargetSlotClicked(CardSlot slot)
    {
        var currentPlayer = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        foreach (var slots in GetTargetSlots(currentPlayer))
        { 
            slots.Clicked -= OnPeekTargetSlotClicked;
        }
        PeekOpponentCardCommand peekOpponentCommand;
        UseSpecialPowerCommand peekOpponentPower;
        peekOpponentCommand = new PeekOpponentCardCommand { TargetPlayerId = slot.PlayerId, SlotIndex = slot.SlotIndex };
        peekOpponentPower = new UseSpecialPowerCommand { PowerCommand = peekOpponentCommand };
        if (_controller.TryExecute(peekOpponentPower, currentPlayer))
        {
            slot.FlashReveal(peekOpponentCommand.RevealedCard, 5f);
            Debug.Log($"PeekTarget: {slot.SlotIndex}");
            EnterWaitingForAction();
        }
    }

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

    private void OnSwapSlotClicked(CardSlot slot)
    {
        var currentPlayer = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        foreach (var slots in GetOwnSlots(currentPlayer))
        {
            slots.Clicked -= OnSwapSlotClicked;
        }
        
        if (_controller.TryExecute(new SwapDrawnCommand { SlotIndex = slot.SlotIndex }, currentPlayer))
        {
            EnterWaitingForAction();
            Debug.Log($"Swap: {slot.SlotIndex}");
        }
    }

    public void Discard()
    {
        if (!_controller.State.PendingDrawnCard.HasValue)
        {
            Debug.Log("Cannot discard right now");
            return;
        }
        
        var currentPlayer = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        Card discardedCard = _controller.State.PendingDrawnCard.Value;
        if (_controller.TryExecute(new DiscardDrawnCommand(), currentPlayer))
        {
            drawnCardPanel.SetActive(false);
            EnterWaitingForAction();
            Debug.Log($"Discard: {discardedCard}");
        }
        else
        {
            Debug.Log("Cannot discard right now");
        }
    }

    public void UsePower()
    {
        var currentPlayer = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;

        switch (_currentAbility)
        {
            case SpecialAbility.SkipNext:
                var skipNext = new UseSpecialPowerCommand
                {
                    PowerCommand = new SkipNextCommand()
                };
                if (_controller.TryExecute(skipNext, currentPlayer))
                {
                    drawnCardPanel.SetActive(false);
                    EnterWaitingForAction();
                }
                else
                {
                    Debug.Log("Cannot skip");
                }
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

    public void ShowActions()
    {
        EnterWaitingForAction();
        _controller.State.GamePhases = GamePhase.InProgress;
        Debug.Log("ShowActions called");
    }
    
    private void EnterWaitingForAction()
    {
        actionPanel.SetActive(true);
        _state = TurnUIState.WaitingForAction;
        deckSlot.Clicked += OnDeckClicked;
        discardSlot.Clicked += OnDiscardClicked;
        drawnCardSlot.ShowEmpty();
        
        var current = _controller.State.Players[_controller.State.CurrentPlayerIndex];
        Debug.Log($"Now {current.PlayerName}'s turn (index {_controller.State.CurrentPlayerIndex})");
    }

    private void OnDeckClicked(CardSlot slot)
    {
        Draw();
    }

    private void OnDiscardClicked(CardSlot slot)
    {
        
        if (!_controller.State.Deck.HasDiscardTop)
        {
            Debug.Log("Discard top is empty");
            return;
        }
        var currentPlayer = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        deckSlot.Clicked -= OnDeckClicked;
        discardSlot.Clicked -= OnDiscardClicked;
        _state = TurnUIState.WaitingForSwapSlot;
        Debug.Log("Discard picked up - choose a slot to swap it into");
        
        foreach (var slots in GetOwnSlots(currentPlayer))
        {
            slots.Clicked += OnSwapFromDiscardSlotClicked;
        }
    }

    private void OnSwapFromDiscardSlotClicked(CardSlot slot)
    {
        var currentPlayer = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        foreach (var slots in GetOwnSlots(currentPlayer))
        {
            slots.Clicked -= OnSwapFromDiscardSlotClicked;
        }
        
        SwapFromDiscardCommand swapFromDiscardCommand = new SwapFromDiscardCommand
        {
            SlotIndex = slot.SlotIndex,
        };
        if (_controller.TryExecute(swapFromDiscardCommand, currentPlayer))
        {
            EnterWaitingForAction();
            Debug.Log($"Swap: {slot.SlotIndex}");
        }
    }

    public void CallCabboo()
    {
        var currentPlayer = _controller.State.Players[_controller.State.CurrentPlayerIndex].PlayerId;
        if (_controller.TryExecute(new CallCabbooCommand(), currentPlayer))
        {
            Debug.Log($"Cabboo Called! GamePhases = {_controller.State.GamePhases}");
            ShowRoundEnd();
        }
        else
        {
            Debug.Log("Cabboo not called");
        }
    }

    private void ShowRoundEnd()
    {
        actionPanel.SetActive(false);
        deckSlot.Clicked -= OnDeckClicked;
        discardSlot.Clicked -= OnDiscardClicked;
        roundEndPanel.SetActive(true);
        var roundScorer = RoundScorer.ScoreRound(_controller.State);
        _scoreBoard.ApplyResultScore(roundScorer);
        
        StringBuilder roundEndBuilder = new StringBuilder();
        
        foreach (var result in roundScorer)
        {
            var players = _controller.State.Players.Find(p => p.PlayerId == result.PlayerId);
            
           roundEndBuilder.AppendLine($"{players.PlayerName} {players.PlayerId}: Raw: {result.RawScore} Pts: {result.PointsAwarded} Total: {_scoreBoard.GetTotalScore(result.PlayerId)} {(result.IsCabbooCaller ? "(Called Cabboo)" : "")}"); 
        }
        roundEndText.text = roundEndBuilder.ToString();
    }

    public void NewRound()
    {
        roundEndPanel.SetActive(false);
        _controller.StartNewRound(playerGridPanels.Count);
        Debug.Log("New round requested");
    }
}