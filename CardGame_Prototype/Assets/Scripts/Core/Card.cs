public struct Card
{
    public Suit Suit;
    public int Rank;

    public Card(Suit suit, int rank)
    {
        Suit = suit;
        Rank = rank;
    }

    public override string ToString()
    {
        if (Suit == Suit.Joker)
            return "Joker";

        string rankName = Rank switch
        {
            1 => "Ace",
            11 => "Jack",
            12 => "Queen",
            13 => "King",
            _ => Rank.ToString()
        };

        return $"{rankName} of {Suit}";
    }
}
