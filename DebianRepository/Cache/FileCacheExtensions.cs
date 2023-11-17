using System;
using System.IO;

namespace ArxOne.Debian.Cache;

public static class FileCacheExtensions
{
    public static Stream Get(this FileCache? cache, FileCacheReference reference)
    {
        return cache?.DoGet(reference) ?? new MemoryStream();
    }

    public static void Set(this FileCache? cache, FileCacheReference reference, Action<Stream> action)
    {
        cache?.DoSet(reference, action);
    }

    public static void Clear(this FileCache? cache, FileCacheReference reference)
    {
        cache?.DoClear(reference);
    }
}
