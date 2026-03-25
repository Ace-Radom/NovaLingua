using System;
using System.Collections.Generic;

namespace NovaLingua.Lib.Extensions;

public static class DictionaryExtensions
{
    public static bool TryUpdateValue<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        TKey key,
        Func<TValue, TValue> updateFunc
    )
        where TKey : notnull
    {
        if (dict.TryGetValue(key, out var value))
        {
            var newValue = updateFunc(value);
            dict[key] = newValue;
            return true;
        }
        return false;
    }

    public static bool TryUpdateValue<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        TKey key,
        Action<TValue> updateAction
    )
        where TKey : notnull
        where TValue : class
    {
        if (dict.TryGetValue(key, out var value))
        {
            updateAction(value);
            return true;
        }
        return false;
    }
}
