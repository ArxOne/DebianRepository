using System.IO;
using System.Linq;

namespace ArxOne.Debian.Stanza;

public class StanzaWriter
{
    private readonly TextWriter _textWriter;

    public StanzaWriter(TextWriter textWriter)
    {
        _textWriter = textWriter;
    }

    public void Write(Stanza stanza)
    {
        foreach (var (key, value) in stanza)
        {
            _textWriter.WriteLine($"{key}: {value.FirstLine}");
            foreach (var remainingLine in value.Lines.Skip(1))
                _textWriter.WriteLine($" {remainingLine}");
        }
        _textWriter.WriteLine();
    }
}
