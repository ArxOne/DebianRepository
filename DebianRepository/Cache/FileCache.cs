using System;
using System.IO;

namespace ArxOne.Debian.Cache;

public class FileCache
{
    private readonly string _rootPath;
    private readonly object _lock = new();

    public FileCache(string name, string? rootPath = null)
    {
        _rootPath = rootPath ?? Path.Combine(Path.GetTempPath(), "debian-repository", name);
    }

    private string GetPath(FileCacheReference reference)
    {
        var path = Path.Combine(_rootPath, reference.Distribution);
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
            for (var parentDirectory = Path.GetDirectoryName(path); parentDirectory != _rootPath; parentDirectory = Path.GetDirectoryName(parentDirectory))
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
