using System.Collections.Generic;
using NUnit.Framework;

public class ScoreboardTests
{
    [Test]
    public void ApplyResultScore_AccumulatesAcrossMultipleRounds()
    {
        var scoreBoard = new ScoreBoard();

        var round1 = new List<RoundResult>()
        {
            new RoundResult { PlayerId = 0, PointsAwarded = 5 },
            new RoundResult { PlayerId = 1, PointsAwarded = 0 },
        };
        
        scoreBoard.ApplyResultScore(round1);

        var round2 = new List<RoundResult>()
        {
            new RoundResult { PlayerId = 0, PointsAwarded = 3 },
            new RoundResult { PlayerId = 1, PointsAwarded = -5 },
        };
        
        scoreBoard.ApplyResultScore(round2);
        
        Assert.AreEqual(8, scoreBoard.GetTotalScore(0));
        Assert.AreEqual(-5, scoreBoard.GetTotalScore(1));
    }
}