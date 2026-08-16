public class SkipNextCommand : ICommand
{
    public bool CanExecute(GameState state,  int playerId)
    {
        bool isPlayerTurn = state.Players[state.CurrentPlayerIndex].PlayerId == playerId;
        bool drewCorrectRank = state.PendingDrawnCard.HasValue && state.PendingDrawnCard.Value.Rank == 11;
        return isPlayerTurn  && drewCorrectRank;
    }

    public void Execute(GameState state,  int playerId)
    {
        state.AdvanceTurn();
    }
}