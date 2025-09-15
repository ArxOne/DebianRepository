
using System;
using System.Text;
using ArxOne.Debian.Cache;

namespace ArxOne.Debian;

using System.Security.Cryptography;

public record DebianRepositoryConfiguration
{
    public string WebRoot { get; init; } = "/debian";

    public string PoolRoot { get; init; } = "pool/"; // is set below WebRoot, must be separated because it is handled to redirect downloads

    public string Brand { get; init; } = "MyRepo";
    public string CacheName { get; init; } = "cache";
    public string StorageRoot { get; init; }

    public Encoding StanzaEncoding { get; init; } = new UTF8Encoding(false);

    public string GpgPublicKeyFileName { get; init; } = "public.gpg";

    public string GpgPath { get; init; } = "gpg";

    public string? GpgPrivateKey { get; init; }

    public string[] AllArchitectures { get; init; } = ["amd64", "i386", "armel", "armhf"];

    private FileCache? _fileCache;

    public HashAlgorithmName[] Hashes { get; init; } = { HashAlgorithmName.SHA256, HashAlgorithmName.SHA512 };

    public FileCache? FileCache
    {
        get { return _fileCache ??= new FileCache { CacheName = CacheName }; }
        init { _fileCache = value; }
    }

    public Func<Uri>? GetRequestUri { get; init; }

    public DebianRepositoryConfiguration(string storageRoot = "")
    {
        StorageRoot = storageRoot;
    }

    public Gpg Gpg()
    {
        var gpg = new Gpg(GpgPath);
        var gpgPrivateKey = GpgPrivateKey;
        if (gpgPrivateKey is not null)
            gpg.AddPrivateKey(gpgPrivateKey);
        return gpg;
    }
}
