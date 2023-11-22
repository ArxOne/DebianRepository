using System;

namespace ArxOne.Debian;

public record DebianRepositoryRoute(string Path, Delegate? Handler, string? ContentType, string? Redirection = null)
{
    public void Deconstruct(out string path, out Delegate? handler, out string? contentType)
    {
        path = Path;
        handler = Handler;
        contentType = ContentType;
    }
}
