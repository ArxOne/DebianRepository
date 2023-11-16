using System.Collections.Generic;

namespace ArxOne.Debian;

public class DebianRepositoryComponentArchitecture
{
    public string SourceDirectory { get; set; }
    public string Distribution { get; set; }
    public string Arch { get; set; }

    public IReadOnlyList<DebianRepositoryPackage> Packages { get; set; }

    public DebianRepositoryComponentArchitecture(string arch)
    {
        Arch = arch;
    }
}
