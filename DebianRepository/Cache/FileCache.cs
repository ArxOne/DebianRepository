using System;
using System.IO;
using System.Xml.Linq;

namespace ArxOne.Debian.Cache;

public class FileCache
{
    private readonly object _lock = new();

    private readonly string _cacheName;

    public string CacheName
    {
        get { return _cacheName; }
        init
        {
            CachePath = Path.Combine(Path.GetTempPath(), "debian-repository", _cacheName = value);
        }
    }

    public string CachePath { get; init; }

    private string GetPath(FileCacheReference reference)
    {
        var path = Path.Combine(CachePath, reference.Distribution);
        if (reference.Component is not null)
        {
            path = Path.Combine(path, reference.Component);
            if (reference.Arch is not null)
            {
                path = Path.Combine(path, reference.Arch);
            }
        }
        path = Path.Combine(path, reference.Name);
        return path;
    }

    internal Stream DoGet(FileCacheReference reference)
    {
        lock (_lock)
        {
            var path = GetPath(reference);
            if (File.Exists(path))
                return new MemoryStream(File.ReadAllBytes(path));
            return new MemoryStream();
        }
    }

    internal void DoSet(FileCacheReference reference, Action<Stream> action)
    {
        lock (_lock)
        {
            var path = GetPath(reference);
            var parentDirectory = Path.GetDirectoryName(path);
            if (!Directory.Exists(parentDirectory))
                Directory.CreateDirectory(parentDirectory);
            using var fileStream = File.Create(path);
            action(fileStream);
        }
    }

    internal void DoClear(FileCacheReference reference)
    {
        var path = GetPath(reference);
        File.Delete(path);
        try
        {
            for (var parentDirectory = Path.GetDirectoryName(path); parentDirectory != CachePath; parentDirectory = Path.GetDirectoryName(parentDirectory))
            {
                Directory.Delete(parentDirectory);
            }
        }
        catch (IOException)
        {
            // the directory is not empty, stop
        }
    }
}
