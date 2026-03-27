namespace NovaLingua.Lib.Data.DataStructures;

public abstract class AbstractDoubleLinkedHashMapNode<T> where T : class
{
    public T? Prev { get; set; }
    public T? Next { get; set; }
    public uint Order { get; set; }
}
