using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using ArxOne.Debian.Stanza;

namespace ArxOne.Debian;

public class DebianRepositoryDistribution
{
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

    public DebianRepositoryDistribution(string distributionName, params DebianRepositoryComponent[] components)
    {
        DistributionName = distributionName;
        Components = components.ToImmutableArray();
    }

    public void AddSource(string path, string componentName)
    {
        foreach (var debFilePath in Directory.GetFiles(path))
        {
            using var debStream = File.OpenRead(debFilePath);
            var debReader = new DebReader(debStream);
            try
            {
                var (control, files) = debReader.Read();
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
