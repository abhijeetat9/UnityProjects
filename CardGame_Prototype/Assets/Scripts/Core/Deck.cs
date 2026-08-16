using System;
using System.Collections.Generic;

public class Deck
{
    private readonly List<Card> _drawPile = new List<Card>();
    private readonly List<Card> _discardPile = new List<Card>();

    public int DrawPileCount => _drawPile.Count;
    public int DiscardPileCount => _discardPile.Count;
    public bool HasDiscardTop => _discardPile.Count > 0;
    public void BuildStandardDeck()
    {
        _drawPile.Clear();
        _discardPile.Clear();

        Suit[] realSuits = { Suit.Spades, Suit.Clubs, Suit.Diamonds, Suit.Hearts };

        foreach (Suit suit in realSuits)
        {
            for (int rank = 1; rank <= 13; rank++)
            {
                _drawPile.Add(new Card(suit, rank));
            }
        }

        _drawPile.Add(new Card(Suit.Joker, 0));
        _drawPile.Add(new Card(Suit.Joker, 0));
    }

    public void Shuffle(Random rng)
    {
        for (int i = _drawPile.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (_drawPile[i], _drawPile[j]) = (_drawPile[j], _drawPile[i]);
        }
    }

    public Card DrawCard()
    {
        if (_drawPile.Count == 0)
        {
            ReshuffleDiscardIntoDraw();
        }

        int lastIndex = _drawPile.Count - 1;
        Card card = _drawPile[lastIndex];
        _drawPile.RemoveAt(lastIndex);
        return card;
    }

    public void DiscardCard(Card card)
    {
        _discardPile.Add(card);
    }

    public Card GetDiscardTop()
    {
        return _discardPile[_discardPile.Count - 1];
    }

    public Card TakeDiscardTop()
    {
        if (_discardPile.Count == 0)
        {
            throw new InvalidOperationException("Cannot take: not enough cards in the discard pile.");
        }

        int lastIndex = _discardPile.Count - 1;
        Card card = _discardPile[lastIndex];
        _discardPile.RemoveAt(lastIndex);
        return card;
    }

    private void ReshuffleDiscardIntoDraw()
    {
        if (_discardPile.Count <= 1)
        {
            throw new InvalidOperationException(
                "Cannot reshuffle: not enough cards in the discard pile to refill the draw pile.");
        }

        Card top = _discardPile[_discardPile.Count - 1];
        _discardPile.RemoveAt(_discardPile.Count - 1);

        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
        _discardPile.Add(top);

        Shuffle(new Random());
    }
}
