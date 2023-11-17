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

        public Serialized(string? name = null, string[]? description = null, string? version = null)
        {
            Name = name;
            Description = description;
            Version = version;
        }
    }

    [Test]
    public void DeserializeSingle()
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

    [Test]
    public void SerializeSingle()
    {
        var r = new Serialized("name", new[] { "some", "description" }, "α");

        var serializer = new StanzaSerializer();
        var stringWriter = new StringWriter();
        serializer.Serialize(r, stringWriter);

        var s = stringWriter.ToString();
        Assert.AreEqual(@"Name: name
Description: some
 description
Version: α

", s);
    }
}
