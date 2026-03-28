using System;

namespace NovaLingua.Lib.Extensions;

public static class WordClassExtensions
{
    public static WordClass ToWordClass(this int value)
    {
        if (Enum.IsDefined(typeof(WordClass), value))
        {
            return (WordClass)value;
        }
        return WordClass.Unknown;
    }
}
