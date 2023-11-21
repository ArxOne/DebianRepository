using System;
using System.IO;

namespace ArxOne.Debian;

public record DebianRepositoryDistributionSource(string Distribution, string Component, string SourceRelativeDirectory, Func<Stream, byte[]?> GetRawControl);
