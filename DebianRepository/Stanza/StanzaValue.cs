using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ArxOne.Debian.Stanza;

[DebuggerDisplay($"{{{nameof(Text)}}}")]
public class StanzaValue
{
    public IReadOnlyList<string> Lines { get; }
    public string FirstLine => Lines[0];
    public string Text => string.Join(Environment.NewLine, Lines);

    public StanzaValue(IEnumerable<string> lines)
    {
        Lines = lines.ToImmutableArray();
    }
}
