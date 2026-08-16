using System.Collections.Generic;

public class ScoreBoard
{
    private readonly Dictionary<int, int> _totals = new Dictionary<int, int>();
    
    public void ApplyResultScore(List<RoundResult> results)
    {
        
        foreach (var result in results)
        {
            _totals.TryGetValue(result.PlayerId, out int currentTotal);
            _totals[result.PlayerId] = currentTotal + result.PointsAwarded;
        }
    }

    public int GetTotalScore(int playerId)
    {
        _totals.TryGetValue(playerId, out int total);
        return total;
    }
}