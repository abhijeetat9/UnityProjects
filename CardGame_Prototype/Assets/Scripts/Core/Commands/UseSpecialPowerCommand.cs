public class UseSpecialPowerCommand : ICommand
{
    public ICommand PowerCommand;

    public bool CanExecute(GameState state, int playerId)
    {
        bool isPlayerTurn = state.Players[state.CurrentPlayerIndex].PlayerId == playerId;
        bool hasPendingCard = state.PendingDrawnCard.HasValue;

        bool isPowerValid = PowerCommand.CanExecute(state, playerId);
        return isPlayerTurn && hasPendingCard  && isPowerValid;
    }

    public void Execute(GameState state, int playerId)
    {
        PowerCommand.Execute(state, playerId);
        state.Deck.DiscardCard(state.PendingDrawnCard.Value);
        state.ClearPendingDrawn();
        state.AdvanceTurn();
    }
}