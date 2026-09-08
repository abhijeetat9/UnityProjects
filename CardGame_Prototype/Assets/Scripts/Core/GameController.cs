using System;

public class GameController
{
    public GameState State  { get;  private set; }
    public event Action StateChanged;
    public event Action NewRoundStarted;

    public GameController(GameState state)
    {
        State = state;
    }

    public bool TryExecute(ICommand command, int playerId)
    {
        if (!command.CanExecute(State, playerId))
        {
            return false;
        }
        command.Execute(State,  playerId);
        StateChanged?.Invoke();
        return true;
    }

    public bool TryDrawCard(int playerId)
    {
        bool isPlayerTurn = State.Players[State.CurrentPlayerIndex].PlayerId == playerId;
        if (!isPlayerTurn || State.PendingDrawnCard.HasValue)
        {
            return false;
        }
        State.DrawingCard();
        StateChanged?.Invoke();
        return true;
    }
    
    public void StartNewRound(int numPlayers)
    {
        State = RoundSetup.CreateGameState(numPlayers);
        StateChanged?.Invoke();
        NewRoundStarted?.Invoke();
    }
}   