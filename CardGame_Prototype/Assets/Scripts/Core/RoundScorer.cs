using System;
using System.Collections.Generic;
using System.Linq;

public static class RoundScorer
{
    public static List<RoundResult> ScoreRound(GameState state)
    {
        if (state.CabbooCallerIndex == null)
        {
            throw new InvalidOperationException("Cannot score a round with no Cabboo caller set.");
        }
        
        var results = new List<RoundResult>();

        for (int i = 0; i < state.Players.Count; i++)
        {
            var playerState = state.Players[i];
            int rawScore = playerState.CalculateScore();
            results.Add(new RoundResult
            {
                PlayerId = playerState.PlayerId,
                RawScore = rawScore,
                PointsAwarded = 0
            });
        }
        
        int minScore = results.Min(r => r.RawScore);
        int tieCount = results.Count(r => r.RawScore == minScore);
        
        PlayerState callerPlayer = state.Players[state.CabbooCallerIndex.Value];
        RoundResult callerResult = results.Find(r => r.PlayerId == callerPlayer.PlayerId);
        
        callerResult.IsCabbooCaller = true;

        if (callerResult.RawScore == minScore && tieCount == 1)
        {
            callerResult.PointsAwarded = 5;
        }
        else if (callerResult.RawScore == minScore && tieCount > 1)
        {
            callerResult.PointsAwarded = 3;

            foreach (var r in results)
            {
                if(r.PlayerId != callerResult.PlayerId && r.RawScore == minScore)
                {
                    r.PointsAwarded = 2;
                }
            }
        }
        else
        {
            callerResult.PointsAwarded = -5;
            foreach (var r in results)
            {
                if(r.RawScore == minScore) r.PointsAwarded = 3;
            }
        }
        return results;
    }
}