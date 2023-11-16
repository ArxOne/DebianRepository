using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ArxOne.Debian.Stanza;

[DebuggerDisplay($"{{{nameof(Text)}}}")]
public class StanzaValue
{
    private readonly IReadOnlyList<string> _value;

    public string FirstLine => _value[0];
    public string Text => string.Join(Environment.NewLine, _value);

    public StanzaValue(IEnumerable<string> value)
    {
        _value = value.ToImmutableArray();
    }
}
