using System.Collections.Generic;

namespace ArxOne.Debian.Utility;

internal static class DictionaryExtensions
{
    public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        where TValue : new()
    {
        if (dictionary.TryGetValue(key, out var value))
            return value;
        value = new();
        dictionary.Add(key, value);
        return value;
    }
}