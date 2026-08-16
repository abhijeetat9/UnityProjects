public class PlayerState
{
    public int PlayerId;
    public string PlayerName;
    
    public Card[] Slots =  new Card[4];
    public bool[] HasPeeked =  new bool[4];

    public bool CalledCabboo;
    
    public PlayerState(int playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
    }

    public void RevealInitialPeek()
    {
        HasPeeked[2] = true;
        HasPeeked[3] = true;
    }

    public int CalculateScore()
    {
        int total = 0;
        foreach (Card card in Slots)
        {
            total += ScoreCalculator.ScoreOf(card);
        }
        return total;
    }
}