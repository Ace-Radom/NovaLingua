using System;

namespace NovaLingua.Lib.Extensions;

public static class LetterPlacementRuleExtensions
{
    public static LetterPlacementRule ToLetterPlacementRule(this int value)
    {
        if (Enum.IsDefined(typeof(LetterPlacementRule), value))
        {
            return (LetterPlacementRule)value;
        }
        return LetterPlacementRule.Unknown;
    }
}
