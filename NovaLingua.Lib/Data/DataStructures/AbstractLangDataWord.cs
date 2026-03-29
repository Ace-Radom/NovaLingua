using System;
using System.Collections.Generic;

namespace NovaLingua.Lib.Data.DataStructures;

public abstract class AbstractLangDataWord : AbstractDoubleLinkedHashMapNode<string>
{
    public class Letter
    {
        public string LetterId { get; set; } = "";
        public string VariantId { get; set; } = "";
        public bool UseUppercase { get; set; }
    }

    public List<Letter> Letters { get; set; } = [];
    public string WordStringPreview { get; set; } = "";
    public string Comment { get; set; } = "";
    public long AddTimeTs
    {
        get => _addTimeTs;
        set => _addTimeTs = Math.Max(0, value);
    }

    private long _addTimeTs;
}
