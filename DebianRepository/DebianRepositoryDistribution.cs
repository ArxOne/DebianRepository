using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArxOne.Debian.Cache;
using ArxOne.Debian.Stanza;

namespace ArxOne.Debian;

public class DebianRepositoryDistribution
{
    private readonly DebianRepositoryConfiguration _configuration;
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

    public string DistributionName { get; }
    public IReadOnlyList<DebianRepositoryComponent> Components { get; }

    private readonly IDictionary<string, DebianRepositoryPackage> _packagesByPath = new Dictionary<string, DebianRepositoryPackage>();

    public DebianRepositoryDistribution(string distributionName, DebianRepositoryConfiguration configuration)
    {
        _configuration = configuration;
        DistributionName = distributionName;

        var packages = new List<DebianRepositoryPackage>();
        using var packagesStream = _configuration.FileCache.Get(new FileCacheReference("all-packages", DistributionName));
        using var packagesReader = new StreamReader(packagesStream);
        var serializer = new StanzaSerializer();
        for (; ; )
        {
            var package = serializer.Deserialize<DebianRepositoryPackage>(packagesReader);
            if (package is null)
                break;
            packages.Add(package);
        }

        foreach (var package in packages)
            _packagesByPath[package.Filename] = package;
    }

    public void AddSource(string path, string componentName)
    {
        foreach (var debFilePath in Directory.GetFiles(Path.Combine(_configuration.StorageRoot, path)))
        {
            using var debStream = File.OpenRead(debFilePath);
            var debReader = new DebReader(debStream);
            try
            {
                var (control, _) = debReader.Read();
                using var controlReader = new StringReader(control[""]);
                var stanzaReader = new StanzaReader(controlReader);
                var stanza = stanzaReader.Read().First();
            }
            catch (FormatException)
            {
            }
        }
    }
}
