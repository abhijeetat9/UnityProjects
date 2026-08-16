public class RedKingLookCommand : ICommand
{
    public PeekOpponentCardCommand LookCommand;
    public bool CanExecute(GameState state, int playerId)
    {
        bool isPlayerTurn = state.Players[state.CurrentPlayerIndex].PlayerId == playerId; 
        bool isRedKing = state.PendingDrawnCard.HasValue && state.PendingDrawnCard.Value.Rank == 13 && (state.PendingDrawnCard.Value.Suit == Suit.Hearts || state.PendingDrawnCard.Value.Suit == Suit.Diamonds);

        bool hasPlayerLooked = LookCommand.CanExecute(state, playerId);
        return isPlayerTurn && isRedKing  && hasPlayerLooked;
    }

    public void Execute(GameState state, int playerId)
    {
        LookCommand.Execute(state, playerId);
        state.PendingLookAndSwap = new PendingLookAndSwap
        {
            TargetPlayerId = LookCommand.TargetPlayerId,
            TargetSlotIndex = LookCommand.SlotIndex
        };
    }
}