using System.Collections.Generic;

namespace ArxOne.Debian;

public class DebianRepositoryComponent
{
    public string ComponentName { get; set; }

    public IReadOnlyList<DebianRepositoryComponentArchitecture> Architectures { get; set; }

    public DebianRepositoryComponent(string componentNameName)
    {
        ComponentName = componentNameName;
    }
}
