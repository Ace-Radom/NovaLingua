using System;
using System.Collections.Generic;

namespace NovaLingua.Lib.Data.DataStructures;

public class DoubleLinkedHashMap<TKey, TValue>
    where TKey : class
    where TValue : class, IDoubleLinkedHashMapNode<TKey>
{
    public TKey? Head { get; private set; } = null;
    public TKey? Tail { get; private set; } = null;
    public Dictionary<TKey, TValue> Nodes { get; private set; } = [];

    public int Count => Nodes.Count;
    public bool IsChecked => _isChecked;

    public bool Check(bool setOrder = false)
    {
        _isChecked = false;
        // force check, reset _isChecked state

        if (Nodes.Count == 0)
        {
            if (Head is null && Tail is null)
            {
                return true;
            } // empty map, no head or tail
            return false;
        } // empty map
        if (Head is null || Tail is null)
        {
            return false;
        } // map not empty but head / tail not specified
        if (Nodes.Count == 1 && !CompareKeyEquals(Head, Tail))
        {
            return false;
        } // one obj in map but different head & tail
        if (!Nodes.ContainsKey(Head) || !Nodes.ContainsKey(Tail))
        {
            return false;
        } // head / tail not in nodes

        uint count = 0;
        TKey? ptr = Head;
        TKey? prevPtr = null;
        List<TValue> validPath = setOrder ? new() : null!;
        while (true)
        {
            count++;
            if (count > Nodes.Count)
            {
                return false;
            } // loop

            if (Nodes.TryGetValue(ptr, out var node))
            {
                if (!CompareKeyEquals(node.Prev, prevPtr))
                {
                    return false;
                } // wrong prev
                if (setOrder)
                {
                    validPath.Add(node);
                } // add to path for setting orders later
                if (CompareKeyEquals(ptr, Tail))
                {
                    if (node.Next is not null)
                    {
                        return false;
                    } // tail shouldn't have next
                    break;
                } // reach tail
                if (node.Next is null)
                {
                    return false;
                } // havn't reached tail, no next
                prevPtr = ptr;
                ptr = node.Next;
            }
            else
            {
                return false;
            } // key doesn't exist
        } // walk linked list

        if (count != Nodes.Count)
        {
            return false;
        } // loop

        if (setOrder)
        {
            for (int i = 0; i < validPath.Count; i++)
            {
                validPath[i].Order = (uint)i;
            }
        } // set order if needed

        _isChecked = true;
        return true;
    }
    
    public bool CheckIfNeeded(bool setOrder = false)
    {
        if (_isChecked)
        {
            return true;
        }
        return Check(setOrder);
    }

    public bool TryAdd(TKey key, TKey prevKey, TKey nextKey, TValue value)
    {
        if (!CheckIfNeeded())
        {
            return false;
        } // linked list broken

        if (Nodes.ContainsKey(key))
        {
            return false;
        } // key exists
        if (Nodes.TryGetValue(prevKey, out var prev) && Nodes.TryGetValue(nextKey, out var next))
        {
            if (!CompareKeyEquals(prev.Next, nextKey) || !CompareKeyEquals(prevKey, next.Prev))
            {
                return false;
            } // prev & next are not neighbours
            return TryInsertNoCheck(key, prevKey, prev, nextKey, next, value);
        }
        else
        {
            return false;
        } // prev / next doesn't exist
    }

    public bool TryAddAfter(TKey key, TKey prevKey, TValue value)
    {
        if (!CheckIfNeeded())
        {
            return false;
        } // linked list broken

        if (Nodes.ContainsKey(key))
        {
            return false;
        } // key exists
        if (Nodes.TryGetValue(prevKey, out var prev))
        {
            var nextKey = prev.Next;
            if (nextKey is null)
            {
                return TryInsertTailNoCheck(key, prev, value);
            } // insert tail
            if (Nodes.TryGetValue(nextKey, out var next))
            {
                return TryInsertNoCheck(key, prevKey, prev, nextKey, next, value);
            } // insert
            else
            {
                throw new InvalidOperationException("DoubleLinkedHashMap.TryAddAfter(): next key doesn't exist after successful check");
                // this should never happen
            } // next key doesn't exist
        }
        else
        {
            return false;
        } // prev doesn't exist
    }

    public bool TryAddBefore(TKey key, TKey nextKey, TValue value)
    {
        if (!CheckIfNeeded())
        {
            return false;
        } // linked list broken

        if (Nodes.ContainsKey(key))
        {
            return false;
        } // key exists
        if (Nodes.TryGetValue(nextKey, out var next))
        {
            var prevKey = next.Prev;
            if (prevKey is null)
            {
                return TryInsertHeadNoCheck(key, next, value);
            } // insert head
            if (Nodes.TryGetValue(prevKey, out var prev))
            {
                return TryInsertNoCheck(key, prevKey, prev, nextKey, next, value);
            } // insert
            else
            {
                throw new InvalidOperationException("DoubleLinkedHashMap.TryAddBefore(): prev key doesn't exist after successful check");
                // this should never happen
            } // prev key doesn't exist
        }
        else
        {
            return false;
        } // next key doesn't exist
    }

    public bool TryAddHead(TKey key, TValue value)
    {
        if (!CheckIfNeeded())
        {
            return false;
        } // linked list broken

        if (Nodes.Count == 0)
        {
            value.Prev = null;
            value.Next = null;
            Head = key;
            Tail = key;
            Nodes.Add(key, value);
            return true;
        } // empty hash map

        if (Nodes.ContainsKey(key))
        {
            return false;
        } // key exists
        if (Nodes.TryGetValue(Head!, out var head))
        {
            return TryInsertHeadNoCheck(key, head, value);
        }
        else
        {
            throw new InvalidOperationException("DoubleLinkedHashMap.TryAddHead(): head key doesn't exist in an unempty map after successful check");
            // this should never happen
        } // head key doesn't exist
    }

    public bool TryAddTail(TKey key, TValue value)
    {
        if (!CheckIfNeeded())
        {
            return false;
        } // linked list broken

        if (Nodes.Count == 0)
        {
            value.Prev = null;
            value.Next = null;
            Head = key;
            Tail = key;
            Nodes.Add(key, value);
            return true;
        } // empty hash map

        if (Nodes.ContainsKey(key))
        {
            return false;
        } // key exists
        if (Nodes.TryGetValue(Tail!, out var tail))
        {
            return TryInsertTailNoCheck(key, tail, value);
        }
        else
        {
            throw new InvalidOperationException("DoubleLinkedHashMap.TryAddTail(): tail key doesn't exist in an unempty map after successful check");
            // this should never happen
        } // tail key doesn't exist
    }

    private bool TryInsertNoCheck(TKey key, TKey prevKey, TValue prev, TKey nextKey, TValue next, TValue value)
    {
        value.Prev = prevKey;
        value.Next = nextKey;
        prev.Next = key;
        next.Prev = key;
        Nodes.Add(key, value);
        // TODO: recounts order
        return true;
    }

    private bool TryInsertHeadNoCheck(TKey key, TValue head, TValue value)
    {
        value.Prev = null;
        value.Next = Head;
        head.Prev = key;
        Head = key;
        Nodes.Add(key, value);
        // TODO: recounts order
        return true;
    }

    private bool TryInsertTailNoCheck(TKey key, TValue tail, TValue value)
    {
        value.Prev = Tail;
        value.Next = null;
        tail.Next = key;
        Tail = key;
        Nodes.Add(key, value);
        // TODO: recounts order
        return true;
    }

    static private bool CompareKeyEquals(TKey? lhs, TKey? rhs) => EqualityComparer<TKey>.Default.Equals(lhs, rhs);

    private bool _isChecked = false;
}
