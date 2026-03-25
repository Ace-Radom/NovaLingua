using System;

namespace NovaLingua.Lib.Extensions;

public static class LetterTypeExtensions
{
    public static LetterType ToLetterType(this int value)
    {
        if (Enum.IsDefined(typeof(LetterType), value))
        {
            return (LetterType)value;
        }
        return LetterType.Unknown;
    }
}
