using System;

public static class ScoreCalculator
{
    public static int ScoreOf(Card card)
    {
        if (card.Suit == Suit.Joker) return -1;

        return card.Rank switch
        {
            13 => (card.Suit == Suit.Hearts || card.Suit == Suit.Diamonds) ? 13 : 0,
            int r when r >= 1 && r <= 12 => r,
            _ => throw new ArgumentOutOfRangeException(nameof(card.Rank), "Invalid card rank")
        };
    }
}