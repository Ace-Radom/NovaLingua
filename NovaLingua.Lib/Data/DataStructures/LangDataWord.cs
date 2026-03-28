using System.Collections.Generic;

namespace NovaLingua.Lib.Data.DataStructures;

public class LangDataWord : AbstractLangDataWord
{
    public class WordDefinition
    {
        public WordClass Class { get; set; }
        public string Definition { get; set; } = "";
    }

    public List<WordDefinition> Definitions { get; set; } = [];
    public DoubleLinkedHashMap<string, LangDataWordInflection> Inflections { get; set; } = new();
}
