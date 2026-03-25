using System;

namespace NovaLingua.Lib.Extensions;

public static class LocalLanguageExtensions
{
    public static LocalLanguage ToLocalLanguage(this int value)
    {
        if (Enum.IsDefined(typeof(LocalLanguage), value))
        {
            return (LocalLanguage)value;
        }
        return LocalLanguage.Unknown;
    }
}
