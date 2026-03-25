namespace NovaLingua.Lib.Data.DataStructures.Json;

internal interface IJsonDataStructure<T> where T : IJsonDataStructure<T>
{
    public abstract bool IsEmpty { get; }
    public static abstract T Empty { get; }
    public static abstract string TypeName { get; }
}
