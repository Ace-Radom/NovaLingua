using System;

namespace NovaLingua.Lib.Data.DataStructures;

public class LangDataConfig
{
    public bool AutoGenerationUseVariants { get; set; }
    public bool ForceLetterVariantGlobalUnique { get; set; }
    public bool ForceWordInflectionGlobalUnique { get; set; }
    public int MaxConsecutiveVowelsCount
    {
        get => _maxConsecutiveVowelsCount;
        set => _maxConsecutiveVowelsCount = Math.Max(1, value);
    }
    public int MaxConsecutiveConsonantCount
    {
        get => _maxConsecutiveConsonantCount;
        set => _maxConsecutiveConsonantCount = Math.Max(1, value);
    }
    public bool WordCaseInsensitive { get; set; }

    private int _maxConsecutiveVowelsCount;
    private int _maxConsecutiveConsonantCount;
}
