using System.IO;
using ArxOne.Debian.Stanza;
using NUnit.Framework;
#pragma warning disable NUnit2005

namespace DebianRepositoryTest;

[TestFixture]
public class StanzaSerializerTest
{
    public record Serialized
    {
        public string? Name { get; init; }
        public string[]? Description { get; init; }
        public string? Version { get; init; }

        public Serialized()
        { }

        public Serialized(string? Name = null, string[]? Description = null, string? Version = null)
        {
            this.Name = Name;
            this.Description = Description;
            this.Version = Version;
        }

        public void Deconstruct(out string? Name, out string[]? Description, out string? Version)
        {
            Name = this.Name;
            Description = this.Description;
            Version = this.Version;
        }
    }

    [Test]
    public void Read()
    {
        var s = @"Name: nymos
Description: a
 b
Version: 1.0
";
        var serializer = new StanzaSerializer();
        var stringReader = new StringReader(s);
        var r = serializer.Deserialize<Serialized>(stringReader);
        Assert.IsNotNull(r);
        Assert.AreEqual("nymos", r.Name);
        Assert.AreEqual(new[] { "a", "b" }, r.Description);
        Assert.AreEqual("1.0", r.Version);
    }
}
