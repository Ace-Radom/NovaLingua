namespace NovaLingua.Lib.Data.DataStructures;

internal interface IDataStructure<T> where T : IDataStructure<T>
{
    public abstract bool IsEmpty { get; }
    public static abstract T Empty { get; }
    public static abstract string Type { get; }
}
