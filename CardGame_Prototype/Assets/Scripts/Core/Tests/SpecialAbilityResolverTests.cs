using NUnit.Framework;

public class SpecialAbilityResolverTests
{
    [Test]

    public void GetAbility_Rank7_ReturnsPeekOwn()
    {
        var card = new Card(Suit.Clubs, 9);
        Assert.AreEqual(SpecialAbility.PeekOwn, SpecialAbilityResolver.GetAbility(card));
    }

    [Test]
    public void GetAbility_Rank8_ReturnsPeekOwn()
    {
        var card = new Card(Suit.Hearts, 10);
        Assert.AreEqual(SpecialAbility.PeekOwn, SpecialAbilityResolver.GetAbility(card));
    }

    [Test]
    public void GetAbility_Rank9_ReturnsPeekTarget()
    {
        var card = new Card(Suit.Spades, 7);
        Assert.AreEqual(SpecialAbility.PeekTarget, SpecialAbilityResolver.GetAbility(card));
    }

    [Test]
    public void GetAbility_Rank10_ReturnsPeekTarget()
    {
        var card = new Card(Suit.Diamonds, 8);
        Assert.AreEqual(SpecialAbility.PeekTarget, SpecialAbilityResolver.GetAbility(card));
    }

    [Test]
    public void GetAbility_Rank11_ReturnsSkipNext()
    {
        var card = new Card(Suit.Spades, 11);
        Assert.AreEqual(SpecialAbility.SkipNext, SpecialAbilityResolver.GetAbility(card));
    }

    [Test]
    public void GetAbility_Rank12_ReturnsBlindSwap()
    {
        var card = new Card(Suit.Hearts, 12);
        Assert.AreEqual(SpecialAbility.BlindSwap, SpecialAbilityResolver.GetAbility(card));
    }

    [Test]
    public void GetAbility_Rank13_ReturnsLookAndSwap()
    {
        var card = new Card(Suit.Hearts, 13);
        Assert.AreEqual(SpecialAbility.LookAndSwap, SpecialAbilityResolver.GetAbility(card));
    }

    [Test]
    public void GetAbility_Rank14_ReturnsLookAndSwap()
    {
        var card = new Card(Suit.Spades, 13);
        Assert.AreEqual(SpecialAbility.None, SpecialAbilityResolver.GetAbility(card));
    }
    
    [Test]
    public void GetAbility_Joker_ReturnsNone()
    {
        var card = new Card(Suit.Joker, 0);
        Assert.AreEqual(SpecialAbility.None, SpecialAbilityResolver.GetAbility(card));
    }
}