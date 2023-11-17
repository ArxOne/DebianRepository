using System.IO;
using System.Linq;
using ArxOne.Debian.Stanza;
using NUnit.Framework;
#pragma warning disable NUnit2005

namespace DebianRepositoryTest;

[TestFixture]
public class StanzaReaderTest
{
    [Test]
    public void ReadSingleStanza()
    {
        var s = @"Origin: . stable
Label: . stable
Archive: stable
Suite: stable
Architecture: i386
Component: non-free
";

        using var sr = new StringReader(s);
        var stanzaReader = new StanzaReader(sr);
        var stanza = stanzaReader.Read().Single();
        Assert.AreEqual(". stable", stanza["Origin"].Text);
        Assert.AreEqual(". stable", stanza["oRiGin"].Text);
    }

    [Test]
    public void ReadSingleMultilineStanza()
    {
        var s = @"A: ah
B: bé
 bêêê
C: c’est
 s’est
";

        using var sr = new StringReader(s);
        var stanzaReader = new StanzaReader(sr);
        var stanza = stanzaReader.Read().Single();
        Assert.AreEqual(2, stanza["b"].Lines.Count);
        Assert.AreEqual("bé", stanza["b"].Lines[0]);
        Assert.AreEqual("bêêê", stanza["b"].Lines[1]);
        Assert.AreEqual(2, stanza["c"].Lines.Count);
        Assert.AreEqual("c’est", stanza["c"].Lines[0]);
        Assert.AreEqual("s’est", stanza["c"].Lines[1]);
    }

    [Test]
    public void ReadMulitpletanzas()
    {
        var s = @"Package: arxone-backup
Priority: optional
Section: non-free/admin
Installed-Size: 15219477
Maintainer: Support <support@arx.one>
Architecture: all
Version: 10.0.17913.1222
Depends: dotnet-runtime-6.0 (>=6.0)
Filename: pool/non-free/a/arxone-backup/arxone-backup_10.0.17913.1222_all.deb
Size: 5747420
MD5sum: 24d1a842e0948a4919120bebb1b6e4be
SHA1: b6e4a836ff135d9679997f672eaa088582895e83
SHA256: 57bc35d6df1183a5dc6fe6850747bfc7f23a1467b954240d703579008db5e1ef
SHA512: 7e4128a534388f5a61a9dbbf412b3d0cf4c95cfe26e7734fd4e5dfbb51f492fc001236bf1a0739645d5bf576ad7256082a76c139569f90fd7820ed4db3119d7c
Description: Arx One backup
 Secures data by saving it to remote server
 after compression and encryption
Homepage: https://arx.one/sauvegarde

Package: arxone-backup
Priority: optional
Section: non-free/admin
Installed-Size: 15221525
Maintainer: Support <support@arx.one>
Architecture: all
Version: 10.0.17907.1539
Depends: dotnet-runtime-6.0 (>=6.0)
Filename: pool/non-free/a/arxone-backup/arxone-backup_10.0.17907.1539_all.deb
Size: 5748386
MD5sum: e558cc2d73ad9b8112979aa363202a94
SHA1: 7a82c19a789d948d0df97bd1bbc0848f037b88ad
SHA256: f7834af9b8b31f48690a2cff3a097baa5d851622e3f459ec97b383175c8785ef
SHA512: fc05bd8599f3a93830a03b5c12dd11025a6f37d9ed69c5c4fec332190e4734614e409916c56b573371e5012a6c8d36b26290e21c608c911b9a31e789cd942884
Description: Arx One backup
 Secures data by saving it to remote server
 after compression and encryption
Homepage: https://arx.one/sauvegarde



";

        using var sr = new StringReader(s);
        var stanzaReader = new StanzaReader(sr);
        var stanzas = stanzaReader.Read().ToArray();
        Assert.AreEqual(2, stanzas.Length);
    }
}