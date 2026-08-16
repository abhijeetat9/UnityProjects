public class RoundResult
{
    public int PlayerId;
    public int RawScore; // sum of the 4 cards players are holding 
    public int PointsAwarded; // scoreboard points for a round: +5, -5, +3, +2 
    public bool IsCabbooCaller;
}