using System;
using System.Collections.Generic;

namespace NovaLingua.Lib.Data.DataStructures;

public class LangDataLetter : AbstractLangDataLetter
{
    public LetterType Type { get; set; }
    public Dictionary<string, LangDataLetterVariant> Variants { get; set; } = [];
    public string HeadVariantId { get; set; } = "";
    public string TailVariantId { get; set; } = "";

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
