public class SwapFromDiscardCommand : ICommand
{
    public int SlotIndex;

    public bool CanExecute(GameState state, int playerId)
    {
        bool isPlayerTurn = state.Players[state.CurrentPlayerIndex].PlayerId == playerId;
        bool isValidSlotIndex = SlotIndex >= 0 && SlotIndex < state.Players[state.CurrentPlayerIndex].Slots.Length;
        
        bool isDiscardPileEmpty = state.Deck.DiscardPileCount > 0;
        return isPlayerTurn && isValidSlotIndex  && isDiscardPileEmpty;
    }
    
    public void Execute(GameState state, int playerId)
    {
        PlayerState drawingPlayer = state.Players[state.CurrentPlayerIndex];
    
        Card oldCard = drawingPlayer.Slots[SlotIndex];
        drawingPlayer.Slots[SlotIndex] = state.Deck.TakeDiscardTop();
    
        state.Deck.DiscardCard(oldCard);
        
        state.AdvanceTurn();
    }
    
}