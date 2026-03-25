using System;

namespace NovaLingua.Lib.Data.DataStructures;

public abstract class AbstractLangDataLetter
{
    public string Letter { get; set; } = "";
    public string LetterUppercase { get; set; } = "";
    public string PrevLetterId { get; set; } = "";
    public string NextLetterId { get; set; } = "";
    public string Comment { get; set; } = "";
    public long AddTimeTs
    {
        get => _addTimeTs;
        set => _addTimeTs = Math.Max(0, value);
    }

    private long _addTimeTs;
}
