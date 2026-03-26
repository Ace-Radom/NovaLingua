using System;

namespace NovaLingua.Lib.Data.DataStructures;

public class LangDataTodo
{
    public string Msg { get; set; } = "";
    public long AddTimeTs
    {
        get => _addTimeTs;
        set => _addTimeTs = Math.Max(0, value);
    }

    private long _addTimeTs;
}
