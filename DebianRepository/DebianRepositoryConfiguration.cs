﻿using System;
using System.IO;
using System.Text;
using ArxOne.Debian.Cache;

namespace ArxOne.Debian;

public class DebianRepositoryConfiguration
{
    public string WebRoot { get; set; } = "/debian";

    public string PoolRoot { get; set; } = "pool/"; // is set below WebRoot

    public string StorageRoot { get; }

    public Encoding StanzaEncoding { get; set; } = new UTF8Encoding(false);

    public string GpgPublicKeyName { get; set; } = "public.gpg";

    public string GpgPath { get; set; } = "gpg";

    public string GpgPrivateKey { get; set; }

    public string[] AllArchitectures { get; set; } = new string[] { "amd64", "i386", "armel", "armhf" };

    public FileCache? FileCache { get; set; } = new("cache");

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
