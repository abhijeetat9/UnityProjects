public class SwapDrawnCommand : ICommand
{
    public int SlotIndex;

    public bool CanExecute(GameState state, int playerId)
    {
        bool isPlayerTurn = state.Players[state.CurrentPlayerIndex].PlayerId == playerId;
        bool hasPendingCard = state.PendingDrawnCard.HasValue;

        bool isValidSlotIndex = SlotIndex >= 0 && SlotIndex < state.Players[state.CurrentPlayerIndex].Slots.Length;
        return isPlayerTurn && hasPendingCard  && isValidSlotIndex;
    }

    public void Execute(GameState state, int playerId)
    {
        PlayerState drawingPlayer = state.Players[state.CurrentPlayerIndex];
        
        Card oldCard = drawingPlayer.Slots[SlotIndex];
        drawingPlayer.Slots[SlotIndex] = state.PendingDrawnCard.Value;
        
        state.Deck.DiscardCard(oldCard);
        state.ClearPendingDrawn();
        
        state.AdvanceTurn();
    }
}