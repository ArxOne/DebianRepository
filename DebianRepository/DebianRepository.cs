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
    private readonly DebianRepositoryConfiguration _configuration;
    private readonly IReadOnlyList<DebianRepositoryDistributionSource> _sources;

    public IReadOnlyList<DebianRepositoryDistribution> Distributions { get; }

    public DebianRepository(DebianRepositoryConfiguration configuration, IEnumerable<DebianRepositoryDistributionSource> sources)
    {
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

            foreach (var archPackages in packages.PackagesByPath.Values.GroupBy(p => p.RepositoryPackage.Architecture))
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
            var debRelativeFilePath = debFilePath[(_configuration.StorageRoot.Length + 1)..];
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

    private IEnumerable<byte[]> ComputeHashes(Stream s, params HashAlgorithm[] hashAlgorithms)
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
}
