using ArxOne.Debian.Stanza;
using NUnit.Framework;
#pragma warning disable NUnit2005

namespace DebianRepositoryTest;

[TestFixture]
public class StanzaMapperTest
{
    public record Mapped(string S = "", int I = 0, byte[]? B = null, long SomeThing = 0);

    [Test]
    public void ExtractStanza()
    {
        var stanzaMapper = new StanzaMapper();
        var m = new Mapped("hop", 456, new byte[] { 0x67, 0x89, 0x01, 0xab }, 1);
        var stanza = stanzaMapper.Extract(m);
        Assert.AreEqual("hop", stanza["S"].Text);
        Assert.AreEqual("456", stanza["I"].Text);
        Assert.AreEqual("678901ab", stanza["B"].Text);
        Assert.AreEqual("1", stanza["Some-Thing"].Text);
    }

    [Test]
    public void MapStanza()
    {
        var stanza = new Stanza
        {
            { "S", "here" },
            { "I", "18" },
            { "B", "0102030412141830313233aaaaaa" },
            { "Some-Thing", "8" }
        };

        var stanzaMapper = new StanzaMapper();
        var m = new Mapped();
        stanzaMapper.Map(m, stanza);

        Assert.AreEqual("here", m.S);
        Assert.AreEqual(18, m.I);
        Assert.AreEqual(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x12, 0x14, 0x18, 0x30, 0x31, 0x32, 0x33, 0xaa, 0xaa, 0xaa }, m.B);
        Assert.AreEqual(8, m.SomeThing);
    }
}
