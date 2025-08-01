﻿
using System;
using System.Text;
using ArxOne.Debian.Cache;

namespace ArxOne.Debian;

public class DebianRepositoryConfiguration
{
    public string WebRoot { get; set; } = "/debian";

    public string PoolRoot { get; set; } = "pool/"; // is set below WebRoot, must be separated because it is handled to redirect downloads

    public string Brand { get; set; } = "MyRepo";
    public string CacheName { get; set; } = "cache";
    public string StorageRoot { get; }

    public Encoding StanzaEncoding { get; set; } = new UTF8Encoding(false);

    public string GpgPublicKeyFileName { get; set; } = "public.gpg";

    public string GpgPath { get; set; } = "gpg";

    public string? GpgPrivateKey { get; set; }

    public string[] AllArchitectures { get; set; } = ["amd64", "i386", "armel", "armhf"];

    private FileCache? _fileCache ;

    public FileCache? FileCache
    {
        get { return _fileCache ??= new(CacheName); }
        set { _fileCache = value; }
    }

    public Func<Uri>? GetRequestUri { get; set; }

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
