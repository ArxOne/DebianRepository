using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ArxOne.Debian.Stanza;

/// <summary>
/// Same as dictionary, but ordered
/// </summary>
/// <seealso cref="System.Collections.Generic.IDictionary&lt;System.String, ArxOne.Debian.Stanza.StanzaValue&gt;" />
public class Stanza : IDictionary<string, StanzaValue>
{
    private static readonly IEqualityComparer<string> Comparer = StringComparer.InvariantCultureIgnoreCase;
    private readonly List<string> _keys = new();
    private readonly IDictionary<string, StanzaValue> _dictionary = new Dictionary<string, StanzaValue>(Comparer);

    public int Count => _dictionary.Count;
    public bool IsReadOnly => false;

    public ICollection<string> Keys => GetOrderedKeyValuePairs().Select(kv => kv.Key).ToImmutableArray();
    public ICollection<StanzaValue> Values => GetOrderedKeyValuePairs().Select(kv => kv.Value).ToImmutableArray();

    public StanzaValue this[string key]
    {
        get { return _dictionary[key]; }
        set
        {
            if (!_dictionary.ContainsKey(key))
                _keys.Add(key);
            else
            {
                for (int i = 0; i < _keys.Count; i++)
                    if (Comparer.Equals(_keys[i], key))
                        _keys[i] = key;
            }
            _dictionary[key] = value;
        }
    }

    public Stanza()
    { }

    public Stanza(IEnumerable<(string Key, IEnumerable<string> Values)> dictionary)
    {
        foreach (var (key, values) in dictionary)
            Add(key, new StanzaValue(values));
    }

    public IEnumerator<KeyValuePair<string, StanzaValue>> GetEnumerator()
    {
        return GetOrderedKeyValuePairs().GetEnumerator();
    }

    private IEnumerable<KeyValuePair<string, StanzaValue>> GetOrderedKeyValuePairs()
    {
        return _keys.Select(k => new KeyValuePair<string, StanzaValue>(k, _dictionary[k]));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<string, StanzaValue> item)
    {
        _dictionary.Add(item);
        _keys.Add(item.Key);
    }

    public void Clear()
    {
        _dictionary.Clear();
        _keys.Clear();
    }

    public bool Contains(KeyValuePair<string, StanzaValue> item)
    {
        return _dictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, StanzaValue>[] array, int arrayIndex)
    {
        foreach (var keyValuePair in GetOrderedKeyValuePairs())
            array[arrayIndex++] = keyValuePair;
    }

    public bool Remove(KeyValuePair<string, StanzaValue> item)
    {
        var removed = _dictionary.Remove(item);
        if (removed)
            _keys.RemoveAll(k => Comparer.Equals(k, item.Key));
        return removed;
    }

    public void Add(string key, StanzaValue value)
    {
        _dictionary.Add(key, value);
        _keys.Add(key);
    }

    public bool ContainsKey(string key)
    {
        return _dictionary.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        var removed = _dictionary.Remove(key);
        if (removed)
            _keys.RemoveAll(k => Comparer.Equals(k, key));
        return removed;
    }

    public bool TryGetValue(string key, out StanzaValue value)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        return _dictionary.TryGetValue(key, out value);
#pragma warning restore CS8601 // Possible null reference assignment.
    }
}
