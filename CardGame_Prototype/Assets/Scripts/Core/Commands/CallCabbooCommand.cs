public class CallCabbooCommand : ICommand
{
    public bool CanExecute(GameState state, int playerId)
    {
        bool isPlayerTurn = state.Players[state.CurrentPlayerIndex].PlayerId == playerId;
        bool isGamePhase = state.GamePhases == GamePhase.InProgress;
        bool hasNoPendingCard = state.PendingDrawnCard == null;
        
        return isPlayerTurn && isGamePhase && hasNoPendingCard;
    }

    public void Execute(GameState state, int playerId)
    {
        state.CallCabboo();
    }
}