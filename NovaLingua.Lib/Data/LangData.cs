using System;
using System.Collections.Generic;
using NovaLingua.Lib.Data.DataStructures;

namespace NovaLingua.Lib.Data;

public class LangData
{
    public string LangName { get; set; } = "";
    public string LangVersion { get; set; } = "";
    public LocalLanguage LocalLang { get; set; }
    public string LangDescription { get; set; } = "";
    public long CreateTimeTs
    {
        get => _createTimeTs;
        set => _createTimeTs = Math.Max(0, value);
    }
    public long LastModifyTimeTs
    {
        get => _lastModifyTimeTs;
        set => _lastModifyTimeTs = Math.Max(0, value);
    }

    public LangDataConfig Config { get; set; } = new()
    {
        AutoGenerationUseVariants = false,
        ForceLetterVariantGlobalUnique = true,
        ForceWordUnique = true,
        ForceWordInflectionGlobalUnique = true,
        WordCaseInsensitive = true
    };
    public DoubleLinkedHashMap<string, LangDataLetter> Alphabet { get; set; } = new();
    public DoubleLinkedHashMap<string, LangDataWord> WordList { get; set; } = new();
    public List<LangDataTodo> Todos { get; set; } = [];

    private long _createTimeTs;
    private long _lastModifyTimeTs;
}
