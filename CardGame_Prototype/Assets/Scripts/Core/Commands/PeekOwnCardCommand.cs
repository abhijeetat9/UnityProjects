using UnityEngine;

public class PeekOwnCardCommand : ICommand
{
    public int SlotIndex;
    public Card RevealedCard; // it is a single card reveal

    public bool CanExecute(GameState state, int playerId)
    {
        if (state.GamePhases == GamePhase.InitialPeek)
        {
            return SlotIndex == 2 || SlotIndex == 3;
        }
      
        bool isPlayerTurn = state.Players[state.CurrentPlayerIndex].PlayerId == playerId;
        bool isValidSlotIndex = SlotIndex >= 0 && SlotIndex < state.Players[state.CurrentPlayerIndex].Slots.Length;
        
        return isPlayerTurn && isValidSlotIndex;
    }

    public void Execute(GameState state, int playerId)
    {
        PlayerState peekingPlayer = state.Players.Find(p => p.PlayerId == playerId);
        RevealedCard = peekingPlayer.Slots[SlotIndex];
        state.RevealedTo[playerId][SlotIndex] = playerId;
        Debug.Log($"PeekOwnCardCommand.Execute: player {playerId} slot {SlotIndex} -> RevealedTo now {state.RevealedTo[playerId][SlotIndex]}");
    }
}