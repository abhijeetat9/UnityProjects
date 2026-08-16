public class SwapWithPlayerCommand : ICommand
{
    public int OwnSlotIndex;
    public int TargetPlayerId;
    public int TargetSlotIndex;
    
    public bool CanExecute(GameState state, int playerId)
    {
        bool isPlayerTurn = state.Players[state.CurrentPlayerIndex].PlayerId == playerId;
        bool targetIsNotSelf = TargetPlayerId != playerId;
        
        PlayerState targetPlayer = state.Players.Find(p  => p.PlayerId == TargetPlayerId);
        if (targetPlayer == null)
        {
            return false;
        }

        bool isOwnSlot = OwnSlotIndex >= 0 && OwnSlotIndex < state.Players[state.CurrentPlayerIndex].Slots.Length;
        bool isTargetSlotIndex = TargetSlotIndex >= 0 && TargetSlotIndex < targetPlayer.Slots.Length;
        
        return isPlayerTurn && targetIsNotSelf && isOwnSlot && isTargetSlotIndex;
    }

    public void Execute(GameState state, int playerId)
    {
        PlayerState ownPlayer = state.Players.Find(p  => p.PlayerId == playerId);
        PlayerState targetPlayer = state.Players.Find(p  => p.PlayerId == TargetPlayerId);

        (ownPlayer.Slots[OwnSlotIndex], targetPlayer.Slots[TargetSlotIndex]) =
            (targetPlayer.Slots[TargetSlotIndex], ownPlayer.Slots[OwnSlotIndex]);
    }
}