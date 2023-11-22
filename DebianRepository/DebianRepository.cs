using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ArxOne.Debian.Stanza;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ArxOne.Debian.Cache;
using ArxOne.Debian.Utility;

namespace ArxOne.Debian;

public class DebianRepository
{
    public string Description { get; }
    private readonly DebianRepositoryConfiguration _configuration;
    private readonly IReadOnlyList<DebianRepositoryDistributionSource> _sources;

    public IReadOnlyList<DebianRepositoryDistribution> Distributions { get; }

    public DebianRepository(DebianRepositoryConfiguration configuration, IEnumerable<DebianRepositoryDistributionSource> sources, string description = "Arx One Debian Repository server (https://github.com/ArxOne/DebianRepository)")
    {
        Description = description;
        _configuration = configuration;
        _sources = sources.ToImmutableList();
        Distributions = ReadDistributions().ToImmutableList();
    }

    private sealed class Packages
    {
        private readonly FileCacheReference _fileCacheReference;

        public sealed record Package(DebianRepositoryPackage RepositoryPackage)
        {
            public bool IsUsed { get; set; }
        }

        public Dictionary<string, Package> PackagesByPath { get; } = new();

        public bool IsDirty { get; set; }

        public Packages(FileCache? fileCache, string distribution, string component)
        {
            _fileCacheReference = new FileCacheReference("packages-db", distribution, component);
            using var packagesStream = fileCache.Get(_fileCacheReference);
            using var packagesReader = new StreamReader(packagesStream);
            var packagesSerializer = new StanzaSerializer();
            for (; ; )
            {
                var dbPackage = packagesSerializer.Deserialize<DebianRepositoryPackage>(packagesReader);
                if (dbPackage is null)
                    break;
                var package = new Package(dbPackage);
                PackagesByPath[package.RepositoryPackage.Filename!] = package;
            }
        }

        public void Save(FileCache? fileCache)
        {
            fileCache.Set(_fileCacheReference, delegate (Stream s)
            {
                using var packagesWriter = new StreamWriter(s);
                var packagesSerializer = new StanzaSerializer();
                foreach (var package in PackagesByPath.Values)
                    packagesSerializer.Serialize(package.RepositoryPackage, packagesWriter);
            });
        }
    }

    private IEnumerable<DebianRepositoryDistribution> ReadDistributions()
    {
        var distributions = new Dictionary<string /* distribution */, Dictionary<string /* component */, Dictionary<string /* arch */, List<DebianRepositoryPackage>>>>();
        var allPackages = new Dictionary<(string Distribution, string Component), Packages>();
        foreach (var source in _sources)
        {
            if (!allPackages.TryGetValue((source.Distribution, source.Component), out var packages))
            {
                packages = new(_configuration.FileCache, source.Distribution, source.Component);
                allPackages[(source.Distribution, source.Component)] = packages;
            }

            LoadPackages(source, packages);

            var distribution = distributions.Get(source.Distribution);
            var component = distribution.Get(source.Component);

            foreach (var archPackages in packages.PackagesByPath.Values
                         .SelectMany(rp => GetArchitectures(rp.RepositoryPackage.Architectures).Select(a => (Architecture: a, rp.RepositoryPackage)))
                         .GroupBy(p => p.Architecture))
                component.Get(archPackages.Key).AddRange(archPackages.Select(p => p.RepositoryPackage));
        }

        foreach (var packages in allPackages.Values)
        {
            var removedPackages = packages.PackagesByPath.Where(kv => !kv.Value.IsUsed).Select(kv => kv.Key).ToImmutableArray();
            if (removedPackages.Any())
            {
                foreach (var removedPackage in removedPackages)
                    packages.PackagesByPath.Remove(removedPackage);
                packages.IsDirty = true;
            }

            if (packages.IsDirty)
                packages.Save(_configuration.FileCache);
        }

        return BuildDistributions(distributions);
    }

    private IEnumerable<string> GetArchitectures(IEnumerable<string> architectures)
    {
        var architecturesSet = new HashSet<string>(architectures);
        if (architecturesSet.Contains("all"))
        {
            architecturesSet.Remove("all");
            foreach (var allArchitecture in _configuration.AllArchitectures)
                architecturesSet.Add(allArchitecture);
        }
        return architecturesSet;
    }

    private IEnumerable<DebianRepositoryDistribution> BuildDistributions(Dictionary<string /* distribution */, Dictionary<string /* component */,
        Dictionary<string /* arch */, List<DebianRepositoryPackage>>>> distributions)
    {
        foreach (var distribution in distributions)
            yield return new(distribution.Key, BuildComponents(distribution.Value));
    }

    private IEnumerable<DebianRepositoryDistributionComponent> BuildComponents(Dictionary<string /* component */,
        Dictionary<string /* arch */, List<DebianRepositoryPackage>>> components)
    {
        foreach (var component in components)
            yield return new(component.Key, BuildArchitectures(component.Value));
    }

    private IEnumerable<DebianRepositoryDistributionComponentArchitecture> BuildArchitectures(Dictionary<string /* arch */, List<DebianRepositoryPackage>> architectures)
    {
        foreach (var architecture in architectures)
            yield return new(architecture.Key, architecture.Value);
    }

    private void LoadPackages(DebianRepositoryDistributionSource source, Packages packages)
    {
        foreach (var debFilePath in Directory.GetFiles(Path.Combine(_configuration.StorageRoot, source.SourceRelativeDirectory)))
        {
            var debRelativeFilePath = _configuration.PoolRoot
                                      + debFilePath[_configuration.StorageRoot.Length..]
                                          .TrimStart('/', '\\')
                                          .Replace('\\', '/'); // because Windows
            if (packages.PackagesByPath.TryGetValue(debRelativeFilePath, out var package))
            {
                package.IsUsed = true;
                continue;
            }

            using var debStream = File.OpenRead(debFilePath);
            try
            {
                var rawControl = source.GetRawControl(debStream);
                if (rawControl is null)
                    continue;
                using var controlStream = new MemoryStream(rawControl);
                using var controlReader = new StreamReader(controlStream, _configuration.StanzaEncoding);
                var controlSerializer = new StanzaSerializer();
                var control = controlSerializer.Deserialize<DebianRepositoryPackage>(controlReader);
                if (control is not null)
                {
                    debStream.Seek(0, SeekOrigin.Begin);
                    using var md5Hash = MD5.Create();
                    using var sha1Hash = SHA1.Create();
                    using var sha256Hash = SHA256.Create();
                    using var sha512Hash = SHA512.Create();

                    (control.MD5sum, control.SHA1, control.SHA256, control.SHA512) = ComputeHashes(debStream, md5Hash, sha1Hash, sha256Hash, sha512Hash);

                    control.Filename = debRelativeFilePath;
                    control.Size = debStream.Length;

                    package = new Packages.Package(control) { IsUsed = true };
                    packages.PackagesByPath[debRelativeFilePath] = package;
                    packages.IsDirty = true;
                }
            }
            catch (FormatException)
            {
            }
        }
    }

    private static IEnumerable<byte[]> ComputeHashes(Stream s, params HashAlgorithm[] hashAlgorithms)
    {
        var buffer = new byte[1 << 20];
        for (; ; )
        {
            var bytesRead = s.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                foreach (var hashAlgorithm in hashAlgorithms)
                    hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
                return hashAlgorithms.Select(h => h.Hash!);
            }
            foreach (var hashAlgorithm in hashAlgorithms)
                hashAlgorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
        }
    }

    public DebianRepository Refresh() => new DebianRepository(_configuration, _sources);

    /*
     Serve:
       /key.gpg
       /dists/{Distribution}
       /dists/{Distribution}/Release
       /dists/{Distribution}/Release.gpg
       /dists/{Distribution}/InRelease
       /dists/{Distribution}/{Component}/binary-{Arch}
       /dists/{Distribution}/{Component}/binary-{Arch}/Packages
       /dists/{Distribution}/{Component}/binary-{Arch}/Packages.gz
       /dists/{Distribution}/{Component}/binary-{Arch}/Packages.bz2
       /dists/{Distribution}/{Component}/binary-{Arch}/Release
    */

    private record FileHash(byte[] Hash, long Length, string Name)
    {
        public override string ToString()
        {
            return $"{Convert.ToHexString(Hash).ToLower()} {Length,10} {Name}";
        }
    }

    public IEnumerable<DebianRepositoryRoute> GetRoutes()
    {
        var textMimeType = "text/plain";
        var publicKey = _configuration.GpgPublicKey;
        yield return new($"{_configuration.WebRoot}/{_configuration.GpgPublicKeyName}", () => publicKey, textMimeType);

        var hashes = new (string Name, HashAlgorithm HashAlgorithm)[]
        {
            ("MD5Sum", MD5.Create()),
            ("SHA1", SHA1.Create()),
            ("SHA256", SHA256.Create()),
            ("SHA512", SHA512.Create()),
        };
        foreach (var distribution in Distributions)
        {
            var distributionPath = $"{_configuration.WebRoot}/dists/{distribution.DistributionName}";
            var hashesList = new Dictionary<string, List<FileHash>>();
            var architectures = new HashSet<string>();
            var components = new HashSet<string>();
            foreach (var component in distribution.Components)
            {
                components.Add(component.ComponentName);
                foreach (var architecture in component.Architectures)
                {
                    architectures.Add(architecture.Arch);
                    foreach (var (name, content, contentType) in architecture.GetFiles(distribution, component, _configuration.StanzaEncoding))
                    {
                        var relativePath = $"{component.ComponentName}/binary-{architecture.Arch}/{name}";
                        foreach (var (hashName, hashAlgorithm) in hashes)
                            hashesList.Get(hashName).Add(new FileHash(hashAlgorithm.ComputeHash(content), content.Length, relativePath));
                        yield return new($"{distributionPath}/{relativePath}", () => content, contentType);
                    }
                }
            }
            var (releaseContent, releaseGpgContent, inReleaseContent) = GetReleasesContent(distribution, components, architectures, hashesList);
            yield return new($"{distributionPath}/Release", () => releaseContent, textMimeType);
            yield return new($"{distributionPath}/Release.gpg", () => releaseGpgContent, textMimeType);
            yield return new($"{distributionPath}/InRelease", () => inReleaseContent, textMimeType);
        }

        yield return new($"{_configuration.WebRoot}/{_configuration.PoolRoot}{{*poolPath}}", null, null, $"{_configuration.StorageRoot}/{{poolPath}}");

        foreach (var (_, hashAlgorithm) in hashes)
#pragma warning disable S3966 // stoopid SonarQube
            hashAlgorithm.Dispose();
#pragma warning restore S3966

        _configuration.Gpg.Cleanup();
    }

    private (byte[] releaseContent, byte[] releaseGpgContent, byte[] inReleaseContent) GetReleasesContent(DebianRepositoryDistribution distribution,
        HashSet<string> components, HashSet<string> architectures, Dictionary<string, List<FileHash>> hashesList)
    {
        var releaseContent = GetReleaseContent(distribution, components, architectures, hashesList);
        var tempReleases = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempReleases);
        var tempReleasePath = Path.Combine(tempReleases, "Release");
        var tempReleaseGpgPath = Path.Combine(tempReleases, "Release.gpg");
        var tempInReleasePath = Path.Combine(tempReleases, "InRelease");
        File.WriteAllBytes(tempReleasePath, releaseContent);
        _configuration.Gpg.Invoke($"--detach-sig --armor {tempReleasePath}", ref tempReleaseGpgPath);
        _configuration.Gpg.Invoke($"-a -s --clearsign {tempReleasePath}", ref tempInReleasePath);
        var releaseGpgContent = File.ReadAllBytes(tempReleaseGpgPath);
        var inReleaseContent = File.ReadAllBytes(tempInReleasePath);
        Directory.Delete(tempReleases, true);
        return (releaseContent, releaseGpgContent, inReleaseContent);
    }

    private byte[] GetReleaseContent(DebianRepositoryDistribution distribution, HashSet<string> components, HashSet<string> architectures,
        Dictionary<string, List<FileHash>> hashesList)
    {
        using var contentStream = new MemoryStream();
        using (var writer = new StreamWriter(contentStream, _configuration.StanzaEncoding))
        {
            writer.WriteLine($"Origin: . {distribution.DistributionName}");
            writer.WriteLine($"Label: . {distribution.DistributionName}");
            writer.WriteLine($"Suite: {distribution.DistributionName}");
            writer.WriteLine($"Codename: {distribution.DistributionName}");
            writer.WriteLine($"Date: {DateTimeOffset.Now:R}");
            writer.WriteLine($"Architectures: {string.Join(" ", architectures)}");
            writer.WriteLine($"Components: {string.Join(" ", components)}");
            writer.WriteLine($"Description: {Description}"); // one day, make it multi-line
            foreach (var hashList in hashesList)
            {
                writer.WriteLine($"{hashList.Key}:");
                foreach (var fileHash in hashList.Value)
                    writer.WriteLine($" {fileHash}");
            }

            writer.WriteLine();
        }

        return contentStream.ToArray();
    }
}
