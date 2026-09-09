public class RedKingDecisionCommand : ICommand
{
    public bool ShouldSwap;
    public int OwnSlotIndex;

    public bool CanExecute(GameState state, int playerId)
    {
        bool isPlayerTurn = state.Players[state.CurrentPlayerIndex].PlayerId == playerId; 
        if (state.PendingLookAndSwap == null) return false;

        if (ShouldSwap)
        {
            PlayerState ownPlayer = state.Players.Find(p  => p.PlayerId == playerId);
            
            if (OwnSlotIndex < 0 || OwnSlotIndex >= ownPlayer.Slots.Length) 
            {
                return false;
            }
        }
        return isPlayerTurn;
    }

    public void Execute(GameState state, int playerId)
    {
        if (ShouldSwap)
        {
            var swap = new SwapWithPlayerCommand()
            {
                OwnSlotIndex = OwnSlotIndex,
                TargetPlayerId = state.PendingLookAndSwap.TargetPlayerId,
                TargetSlotIndex = state.PendingLookAndSwap.TargetSlotIndex
            };
            swap.Execute(state, playerId);
        }
        state.RevealedTo[state.PendingLookAndSwap.TargetPlayerId][state.PendingLookAndSwap.TargetSlotIndex] = null;
        state.Deck.DiscardCard(state.PendingDrawnCard.Value);
        state.PendingLookAndSwap = null;
        state.ClearPendingDrawn();
        state.AdvanceTurn();
    }
}