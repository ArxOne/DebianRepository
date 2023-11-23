using System;

namespace ArxOne.Debian;

public record DebianRepositoryRoute(string Path, Delegate? Handler, string? ContentType, string? Redirection = null);
