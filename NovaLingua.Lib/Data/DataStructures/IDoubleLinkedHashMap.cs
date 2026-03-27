using System.Collections.Generic;

namespace NovaLingua.Lib.Data.DataStructures;

public interface IDoubleLinkedHashMap<T> where T : notnull
{
    public T Head { get; set; }
    public T Tail { get; set; }
    public Dictionary<T, IDoubleLinkedHashMapNode<T>> Nodes { get; set; }

    public bool Check(bool setOrder = false)
    {
        if (Nodes.Count == 0)
        {
            if (IsDefault(Head) && IsDefault(Tail))
            {
                return true;
            } // empty map, no head or tail
            return false;
        } // empty map
        if (IsDefault(Head) || IsDefault(Tail))
        {
            return false;
        } // map not empty but head / tail not specified
        if (Nodes.Count == 1 && !Equals(Head, Tail))
        {
            return false;
        } // one obj in map but different head & tail
        if (!Nodes.ContainsKey(Head) || !Nodes.ContainsKey(Tail))
        {
            return false;
        } // head / tail not in nodes

        uint count = 0;
        T ptr = Head;
        T prevPtr = default!;
        while (true)
        {
            count++;
            if (count > Nodes.Count)
            {
                return false;
            } // loop

            if (Nodes.TryGetValue(ptr, out var node))
            {
                if (!Equals(node.Prev, prevPtr))
                {
                    return false;
                } // wrong prev
                if (Equals(ptr, Tail))
                {
                    if (!IsDefault(node.Next))
                    {
                        return false;
                    } // tail shouldn't have next
                    if (setOrder)
                    {
                        node.Order = count - 1;
                    } // set order if needed
                    break;
                } // reach tail
                if (IsDefault(node.Next))
                {
                    return false;
                } // havn't reached tail, no next
                if (setOrder)
                {
                    node.Order = count - 1;
                } // set order if needed
                prevPtr = ptr;
                ptr = node.Next;
            }
            else
            {
                return false;
            } // id doesn't exist
        } // walk linked list

        if (count != Nodes.Count)
        {
            return false;
        } // loop
        return true;

        #region LocalFunction

        static bool IsDefault(T value) => EqualityComparer<T>.Default.Equals(value, default!);
        static bool Equals(T lhs, T rhs) => EqualityComparer<T>.Default.Equals(lhs, rhs);

        #endregion LocalFunction

    }
}
