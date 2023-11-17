using System.IO;
using ArxOne.Debian.Stanza;
using NUnit.Framework;

#pragma warning disable NUnit2005

namespace DebianRepositoryTest;

[TestFixture]
public class StanzaWriterTest
{
    [Test]
    public void WriteSingleStanza()
    {
        var stanza = new Stanza
        {
            { "A", "Ah" },
            { "b", "Bé" },
        };

        using var stringWriter = new StringWriter();
        var stanzaWriter = new StanzaWriter(stringWriter);
        stanzaWriter.Write(stanza);
        var result = stringWriter.ToString();
        Assert.AreEqual(@"A: Ah
b: Bé

", result);
    }
}