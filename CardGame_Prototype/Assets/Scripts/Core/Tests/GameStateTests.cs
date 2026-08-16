using System.Collections.Generic;
using NUnit.Framework;

public class GameStateTests
{
    [Test]
    public void AdvanceTurn_MovesToNextPlayer_InNormalPhase()
    {
        var state = new GameState
        {
            Players = new List<PlayerState>
            {
                new PlayerState(0, "A"),
                new PlayerState(1, "B"),
                new PlayerState(2, "C"),
            },
            CurrentPlayerIndex = 0,
            GamePhases = GamePhase.InProgress
        };

        state.AdvanceTurn();
        
        Assert.AreEqual(1, state.CurrentPlayerIndex);
    }

    [Test]
    public void AdvanceTurn_EndsRound_WhenLoopingBackToCaller()
    {
        var state = new GameState
        {
            Players = new List<PlayerState>
            {
                new PlayerState(0, "A"),
                new PlayerState(1, "B"),
                new PlayerState(2, "C"),
            },
            CurrentPlayerIndex = 1,
            CabbooCallerIndex = 2,
            GamePhases = GamePhase.FinalRound
        };
        
        state.AdvanceTurn();
        Assert.AreEqual(1, state.CurrentPlayerIndex);
        Assert.IsTrue(state.GamePhases == GamePhase.RoundEnded);
    }

    [Test]
    public void ScoreRound_CallerWins_WhenUniqueLowest()
    {
        var caller = new PlayerState(0, "A");
        caller.Slots = new Card[]
        {
            new Card(Suit.Spades, 1),
            new Card(Suit.Spades, 1), 
            new Card(Suit.Spades, 1), 
            new Card(Suit.Spades, 1)
        };
        
        var opponent = new PlayerState(1, "B");
        opponent.Slots = new Card[]
        {
            new Card(Suit.Hearts, 13),
            new Card(Suit.Hearts, 13),
            new Card(Suit.Hearts, 13),
            new Card(Suit.Hearts, 13)
        };
    
        var state = new GameState()
        {
            Players = new List<PlayerState> { caller, opponent },
            CabbooCallerIndex = 0
        };
        
        List<RoundResult> results = RoundScorer.ScoreRound(state);
        
        RoundResult callerResult = results.Find(r => r.PlayerId == 0);
        Assert.AreEqual(4, callerResult.RawScore);
        Assert.AreEqual(5, callerResult.PointsAwarded);
        Assert.IsTrue(callerResult.IsCabbooCaller);
    
    }
    
    [Test]
    public void ScoreRound_CallerLoses_WhenTiedForLowest()
    {
        var caller = new PlayerState(0, "A");
        caller.Slots = new Card[]
        {
            new Card(Suit.Spades, 1),
            new Card(Suit.Spades, 1), 
            new Card(Suit.Spades, 1), 
            new Card(Suit.Spades, 1)
        };
        
        var opponent = new PlayerState(1, "B");
        opponent.Slots = new Card[]
        {
            new Card(Suit.Spades, 1),
            new Card(Suit.Spades, 1),
            new Card(Suit.Spades, 1),
            new Card(Suit.Spades, 1)
        };

        var state = new GameState()
        {
            Players = new List<PlayerState> { caller, opponent },
            CabbooCallerIndex = 0
        };
        
        List<RoundResult> results = RoundScorer.ScoreRound(state);
        
        
        RoundResult callerResult = results.Find(r => r.PlayerId == 0);
        Assert.AreEqual(4, callerResult.RawScore);
        Assert.AreEqual(3, callerResult.PointsAwarded);
        
        RoundResult opponentResult = results.Find(r => r.PlayerId == 1);
        Assert.AreEqual(2, opponentResult.PointsAwarded)
            ;
        Assert.IsTrue(callerResult.IsCabbooCaller);

    }

    [Test]
    public void ScoreRound_CallerLoses_WhenFalseCall()
    {
        var caller = new PlayerState(0, "A");
        caller.Slots = new Card[]
        {
            new Card(Suit.Spades, 1),
            new Card(Suit.Diamonds, 10),
            new Card(Suit.Spades, 1),
            new Card(Suit.Hearts, 1)
        };
        
        var opponent = new PlayerState(1, "B");
        opponent.Slots = new Card[]
        {
            new Card(Suit.Spades, 1),
            new Card(Suit.Spades, 1),
            new Card(Suit.Spades, 1),
            new Card(Suit.Spades, 1)
        };

        var state = new GameState()
        {
            Players = new List<PlayerState> { caller, opponent },
            CabbooCallerIndex = 0
        };
        List<RoundResult> results = RoundScorer.ScoreRound(state);
        RoundResult callerResult = results.Find(r => r.PlayerId == 0);
        Assert.AreEqual(13, callerResult.RawScore);
        Assert.AreEqual(-5, callerResult.PointsAwarded);
        
        RoundResult opponentResult = results.Find(r => r.PlayerId == 1);
        Assert.AreEqual(4, opponentResult.RawScore);
        Assert.AreEqual(3, opponentResult.PointsAwarded);
        Assert.IsTrue(callerResult.IsCabbooCaller);
    }

    
}
