using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using ArxOne.Debian.Cache;
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
    public IReadOnlyList<DebianRepositoryDistributionComponent> Components { get; }

    public DebianRepositoryDistribution(string distributionName, IEnumerable<DebianRepositoryDistributionComponent> components)
    {
        DistributionName = distributionName;
        Components = components.ToImmutableArray();
    }
}
