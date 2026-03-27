namespace NovaLingua.Lib.Data.DataStructures;

public interface IDoubleLinkedHashMapNode<T> where T : notnull
{
    public T Prev { get; set; }
    public T Next { get; set; }
    public uint Order { get; set; }
}
