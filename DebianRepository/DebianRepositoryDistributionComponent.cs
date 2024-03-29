﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ArxOne.Debian;

public class DebianRepositoryDistributionComponent
{
    public string ComponentName { get; }

    public IReadOnlyDictionary<string, DebianRepositoryDistributionComponentArchitecture> Architectures { get; }

    public DebianRepositoryDistributionComponent(string componentNameName, IEnumerable<DebianRepositoryDistributionComponentArchitecture> architectures)
    {
        ComponentName = componentNameName;
        Architectures = architectures.ToImmutableDictionary(a => a.Arch);
    }
}
