using System.Collections.Generic;
using System.Collections.Immutable;

namespace ArxOne.Debian;

public class DebianRepository
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

    public string Distribution { get; }
    public IReadOnlyList<string> Components { get; }

    public DebianRepository(string distribution, params string[] components)
    {
        Distribution = distribution;
        Components = components.ToImmutableArray();
    }
}
