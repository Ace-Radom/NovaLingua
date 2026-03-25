using System;

namespace NovaLingua.Lib.Data.DataStructures;

public class LangDataLetterVariant
{
    public string Letter { get; set; } = "";
    public string LetterUppercase { get; set; } = "";
    public int AlphabeticOrder { get; set; }
    public string Comment { get; set; } = "";
    public long AddTimeTs
    {
        get => _addTimeTs;
        set => _addTimeTs = Math.Max(0, value);
    }

    private long _addTimeTs;
}
