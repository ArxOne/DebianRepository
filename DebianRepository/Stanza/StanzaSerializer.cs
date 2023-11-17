using System;
using System.IO;

namespace ArxOne.Debian.Stanza;

public class StanzaSerializer
{
    private readonly StanzaMapper _mapper = new();

    public void Serialize<T>(T o, TextWriter textWriter) => Serialize(o, typeof(T), textWriter);
    public void Serialize(object o, Type t, TextWriter textWriter)
    {
        var writer = new StanzaWriter(textWriter);
        var stanza = _mapper.Extract(o, t);
        writer.Write(stanza);
    }

    public T? Deserialize<T>(TextReader textReader) => (T?)Deserialize(typeof(T), textReader);

    public object? Deserialize(Type t, TextReader textReader)
    {
        var reader = new StanzaReader(textReader);
        var stanza = reader.ReadNext();
        if (stanza is null)
            return null;
        var instance = Activator.CreateInstance(t);
        _mapper.Map(instance, t, stanza);
        return instance;
    }
}
