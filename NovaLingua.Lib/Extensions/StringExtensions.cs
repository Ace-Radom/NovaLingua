using System.Text.RegularExpressions;

namespace NovaLingua.Lib.Extensions;

public static partial class StringExtensions
{
    public static bool IsValidLetterId(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }
        return GetLetterIdPattern().IsMatch(value);
    }

    [GeneratedRegex(@"^[0-9a-fA-F]{32}$")]
    private static partial Regex GetLetterIdPattern();
}
