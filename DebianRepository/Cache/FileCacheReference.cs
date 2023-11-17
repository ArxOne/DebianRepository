namespace ArxOne.Debian.Cache;

public record FileCacheReference(string Name, string Distribution, string? Component = null, string? Arch = null);