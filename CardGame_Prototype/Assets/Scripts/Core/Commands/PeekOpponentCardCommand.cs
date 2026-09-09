public class PeekOpponentCardCommand : ICommand
{
    public int TargetPlayerId;
    public int SlotIndex;

    public bool CanExecute(GameState state, int playerId)
    {
        bool isPlayerTurn = state.Players[state.CurrentPlayerIndex].PlayerId == playerId;
        bool targetIsNotSelf = TargetPlayerId != playerId;
        
        PlayerState targetPlayer = state.Players.Find(p => p.PlayerId == TargetPlayerId);
        if (targetPlayer == null)
        {
            return false;
        }
        
        bool isValidSlotIndex = SlotIndex >= 0 && SlotIndex < targetPlayer.Slots.Length;
        return isPlayerTurn && targetIsNotSelf && isValidSlotIndex;
    }

    public void Execute(GameState state, int playerId)
    {
        PlayerState targetPlayer = state.Players.Find(p => p.PlayerId == TargetPlayerId);
        state.RevealedTo[targetPlayer.PlayerId][SlotIndex] = playerId;
    }
}