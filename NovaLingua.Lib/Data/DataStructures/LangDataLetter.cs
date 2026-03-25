using System;
using System.Collections.Generic;

namespace NovaLingua.Lib.Data.DataStructures;

public class LangDataLetter
{
    public LetterType Type { get; set; }
    public string Letter { get; set; } = "";
    public string LetterUppercase { get; set; } = "";
    public int AlphabeticOrder { get; set; }
    public string Comment { get; set; } = "";
    public Dictionary<string, LangDataLetterVariant> Variants { get; set; } = [];
    public long AddTimeTs
    {
        get => _addTimeTs;
        set => _addTimeTs = Math.Max(0, value);
    }

    public int MaxCountInWord { get; set; }
    public LetterPlacementRule PlacementRule { get; set; }
    public bool AllowInAutoGeneration { get; set; }
    public int AutoGenerationRate
    {
        get => _autoGenerationRate;
        set => _autoGenerationRate = Math.Clamp(value, 1, 16);
    }

    private long _addTimeTs;
    private int _autoGenerationRate;
}
