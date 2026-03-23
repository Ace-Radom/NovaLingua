using System.Collections.Generic;

namespace NovaLingua.Lib.Data.DataStructures;

internal class AlphabetData
{
    public required int Version { get; set; }
    public required List<LetterData> Vowels { get; set; }
    public required List<LetterData> Consonants { get; set; }

    public static AlphabetData Empty => new()
    {
        Version = 1,
        Vowels = [],
        Consonants = []
    };
}

internal class LetterData
{
    public required string Id { get; set; }
    public required string Letter { get; set; }
    public required string LetterUpper { get; set; }
    public required int PlacementRule { get; set; }
    public required bool HasSpecialUsage { get; set; }
    public required string SpecialUsageDescription { get; set; }
    public required bool AllowInAutoGeneration { get; set; }
    public required int AutoGenerationRareness { get; set; }
}
