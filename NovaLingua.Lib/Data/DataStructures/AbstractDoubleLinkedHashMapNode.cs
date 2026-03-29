using System;

namespace NovaLingua.Lib.Data.DataStructures;

public abstract class AbstractDoubleLinkedHashMapNode<T> where T : class
{
    public T? Prev { get; set; }
    public T? Next { get; set; }
    public int Order
    {
        get => _order;
        set => _order = Math.Max(0, value);
    }

    private int _order;
}
