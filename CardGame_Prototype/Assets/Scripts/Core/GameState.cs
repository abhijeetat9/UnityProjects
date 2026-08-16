using System;
using System.Collections.Generic;

public class GameState
{
    public Deck Deck;
    public List<PlayerState> Players;
    public GamePhase GamePhases;
    public int CurrentPlayerIndex;
    public int? CabbooCallerIndex;
    public Card? PendingDrawnCard;
    public PendingLookAndSwap PendingLookAndSwap;

    public void CallCabboo()
    {
        CabbooCallerIndex = CurrentPlayerIndex;
        GamePhases = GamePhase.RoundEnded;
        //AdvanceTurn();
    }

    public void DrawingCard()
    {
        if (PendingDrawnCard.HasValue)
        {
            throw new InvalidOperationException("Make decision");
        }
        PendingDrawnCard = Deck.DrawCard();
    }

    public void ClearPendingDrawn()
    {
        PendingDrawnCard = null;
    }

    public void AdvanceTurn()
    {
        int next = (CurrentPlayerIndex + 1) % Players.Count;

        if (next == CabbooCallerIndex && GamePhases == GamePhase.FinalRound)
        {
            GamePhases = GamePhase.RoundEnded;
        }
        else
        {
            CurrentPlayerIndex = next;
        }
    }
}