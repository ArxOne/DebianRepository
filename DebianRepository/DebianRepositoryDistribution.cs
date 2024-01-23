using System.Collections.Generic;
using System.Collections.Immutable;

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
    public IReadOnlyDictionary<string, DebianRepositoryDistributionComponent> Components { get; }

    public byte[] ReleaseContent { get; internal set; }
    public byte[] ReleaseGpgContent { get; internal set; }
    public byte[] InReleaseContent { get; internal set; }

    public DebianRepositoryDistribution(string distributionName, IEnumerable<DebianRepositoryDistributionComponent> components)
    {
        DistributionName = distributionName;
        Components = components.ToImmutableDictionary(c => c.ComponentName);
    }
}
