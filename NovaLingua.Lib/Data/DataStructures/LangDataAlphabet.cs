using System.Collections.Generic;

namespace NovaLingua.Lib.Data.DataStructures;

public class LangDataAlphabet
{
    public Dictionary<string, LangDataLetter> Letters { get; set; } = [];
    public string HeadLetterId { get; set; } = "";
    public string TailLetterId { get; set; } = "";
}
