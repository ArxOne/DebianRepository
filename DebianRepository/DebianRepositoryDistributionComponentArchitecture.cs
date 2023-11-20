using System.Collections.Generic;
using System.Collections.Immutable;

namespace ArxOne.Debian;

public class DebianRepositoryDistributionComponentArchitecture
{
    public string Arch { get; }

    public IReadOnlyList<DebianRepositoryPackage> Packages { get; }

    public DebianRepositoryDistributionComponentArchitecture(string arch, IEnumerable<DebianRepositoryPackage> packages)
    {
        Arch = arch;
        Packages = packages.ToImmutableArray();
    }
}
