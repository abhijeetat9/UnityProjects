public interface ICommand
{
    bool CanExecute(GameState state, int playerId);
    void Execute(GameState state, int playerId);
}