using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace ArxOne.Debian.Stanza;

public class StanzaReader
{
    private readonly TextReader _textReader;

    public StanzaReader(TextReader textReader)
    {
        _textReader = textReader;
    }

    public IEnumerable<Stanza> Read()
    {
        for (; ; )
        {
            var stanza = ReadNext();
            if (stanza is null)
                yield break;
            yield return stanza;
        }
    }

    public Stanza? ReadNext()
    {
        var keyValues = ReadKeyValues(_textReader).ToImmutableArray();
        if (keyValues.Length == 0)
            return null;
        return new Stanza(keyValues.Select(kv => (kv.Key, (IEnumerable<string>)kv.Values)));
    }

    private static IEnumerable<(string Key, IReadOnlyList<string> Values)> ReadKeyValues(TextReader textReader)
    {
        string? currentKey = null;
        var values = new List<string>();
        foreach (var (key, value) in ReadKeyValue(textReader))
        {
            if (key is not null && key != currentKey)
            {
                if (currentKey is not null)
                    yield return (currentKey, values);
                currentKey = key;
                values = new() { value };
            }
            else
            {
                values.Add(value);
            }
        }
        if (values.Count > 0 && currentKey is not null)
            yield return (currentKey, values);
    }

    private static IEnumerable<(string? Key, string Value)> ReadKeyValue(TextReader textReader)
    {
        for (; ; )
        {
            var line = textReader.ReadLine();
            if (line is null)
                break;
            if (line == "")
                break;
            if (line.StartsWith(' '))
                yield return (null, line.TrimStart(' '));
            else
            {
                var index = line.IndexOf(": ", StringComparison.InvariantCulture);
                if (index != -1)
                    yield return (line[..index], line[(index + 2)..]);
            }
        }
    }
}
