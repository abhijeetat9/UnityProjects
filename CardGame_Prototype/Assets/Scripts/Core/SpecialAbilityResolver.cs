public static class SpecialAbilityResolver
{
    public static SpecialAbility GetAbility(Card card)
    {
        return card.Rank switch
        {
            7 or 8 => SpecialAbility.PeekTarget,
            9 or 10 => SpecialAbility.PeekOwn,
            11 => SpecialAbility.SkipNext,
            12 => SpecialAbility.BlindSwap,
            13 when (card.Suit == Suit.Hearts || card.Suit == Suit.Diamonds) => SpecialAbility.LookAndSwap,
            _ => SpecialAbility.None
        };
    }
}