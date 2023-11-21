using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using ArxOne.Debian.Stanza;
using ArxOne.Debian.Version;
using SharpCompress.Compressors.BZip2;
using CompressionMode = SharpCompress.Compressors.CompressionMode;

namespace ArxOne.Debian;

public class DebianRepositoryDistributionComponentArchitecture
{
    public string Arch { get; }

    public IReadOnlyList<DebianRepositoryPackage> Packages { get; }

    public DebianRepositoryDistributionComponentArchitecture(string arch, IEnumerable<DebianRepositoryPackage> packages)
    {
        Arch = arch;
        Packages = packages.OrderByDescending(p => p.DebianVersion, DebianVersionComparer.Default).ToImmutableArray();
    }

    public IEnumerable<(string Name, byte[] Content, string ContentType)> GetFiles(DebianRepositoryDistribution distribution, DebianRepositoryDistributionComponent component, Encoding stanzaEncoding)
    {
        yield return GetReleaseFile(distribution, component, stanzaEncoding);
        var (packagesFileName, packageContent, contentType) = GetPackagesFile(stanzaEncoding);
        yield return (packagesFileName, packageContent, contentType);
        yield return Pack(packagesFileName, packageContent, "gz", s => new GZipStream(s, CompressionLevel.SmallestSize), "application/gzip");
        yield return Pack(packagesFileName, packageContent, "bz2", s => new BZip2Stream(s, CompressionMode.Compress, false), "application/x-bzip2");
    }

    private (string Name, byte[] Content, string ContenType) Pack(string fileName, byte[] content, string extension, Func<Stream, Stream> createPacker, string contentType)
    {
        using var packedStream = new MemoryStream();
        using (var packStream = createPacker(packedStream))
            packStream.Write(content);
        return (fileName + "." + extension, packedStream.ToArray(), contentType);
    }

    private (string Name, byte[] Content, string ContentType) GetReleaseFile(DebianRepositoryDistribution distribution, DebianRepositoryDistributionComponent component, Encoding stanzaEncoding)
    {
        return ("Release", WriteContent(stanzaEncoding, delegate (TextWriter writer)
        {
            writer.WriteLine($"Origin: . {distribution.DistributionName}");
            writer.WriteLine($"Label: . {distribution.DistributionName}");
            writer.WriteLine($"Archive: {distribution.DistributionName}");
            writer.WriteLine($"Suite: {distribution.DistributionName}");
            writer.WriteLine($"Architecture: {Arch}");
            writer.WriteLine($"Component: {component.ComponentName}");
        }), "text/plain");
    }

    private (string Name, byte[] Content, string ContentType) GetPackagesFile(Encoding stanzaEncoding)
    {
        var serializer = new StanzaSerializer();
        return ("Packages", WriteContent(stanzaEncoding, delegate (TextWriter writer)
        {
            foreach (var package in Packages)
                serializer.Serialize(package, writer);
        }), "text/plain");
    }

    private byte[] WriteContent(Encoding encoding, Action<TextWriter> contentWriter)
    {
        using var memoryStream = new MemoryStream();
        using (var memoryWriter = new StreamWriter(memoryStream, encoding))
            contentWriter(memoryWriter);
        return memoryStream.ToArray();
    }
}
