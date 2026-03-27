using System;

namespace NovaLingua.Lib.Data.DataStructures;

public class LangDataLetter : AbstractLangDataLetter
{
    public LetterType Type { get; set; }
    public DoubleLinkedHashMap<string, LangDataLetterVariant> Variants { get; set; } = new();

    public int MaxInWordCount
    {
        get => _maxInWordCount;
        set => _maxInWordCount = Math.Max(1, value);
    }
    public LetterPlacementRule PlacementRule { get; set; }
    public bool AllowInAutoGeneration { get; set; }
    public int AutoGenerationRate
    {
        get => _autoGenerationRate;
        set => _autoGenerationRate = Math.Clamp(value, 1, 16);
    }

    private int _maxInWordCount;
    private int _autoGenerationRate;
}
