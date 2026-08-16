public class DiscardDrawnCommand : ICommand
{
    public bool CanExecute(GameState state, int playerId)
    {
        bool isPlayerTurn = state.Players[state.CurrentPlayerIndex].PlayerId == playerId;
        bool hasPendingCard = state.PendingDrawnCard.HasValue;
        return isPlayerTurn && hasPendingCard;
    }

    public void Execute(GameState state, int playerId)
    {
        state.Deck.DiscardCard(state.PendingDrawnCard.Value);
        state.ClearPendingDrawn();
        state.AdvanceTurn();
    }
}