using System;
using System.Collections.Generic;
using System.Linq;

public static class RoundSetup
{
    public static GameState CreateGameState(int numPlayers)
    {
        Deck deck = new Deck();
        deck.BuildStandardDeck();
        deck.Shuffle(new Random());
        
        List<PlayerState> players = Enumerable.Range(0, numPlayers)
            .Select(i => new PlayerState(i , $"Player {i}"))
            .ToList();
        
        foreach (var player in players)
        {
            for (int slot = 0; slot < 4; slot++)
            {
                player.Slots[slot] = deck.DrawCard();
            }
        }

        deck.DiscardCard(deck.DrawCard());
        return new GameState()
        {
            Deck = deck,
            Players = players,
            GamePhases = GamePhase.InitialPeek,
            CurrentPlayerIndex = 0
        };
    }
}