using System;
using System.IO;
using System.Text;
using ArxOne.Debian.Cache;

namespace ArxOne.Debian;

public class DebianRepositoryConfiguration : IDisposable
{
    public string WebRoot { get; set; } = "/debian";
    public string StorageRoot { get; }

    public Encoding StanzaEncoding { get; set; } = new UTF8Encoding(false);

    public string GpgPublicKeyName { get; set; } = "public.gpg";

    private string _gpgPath = "gpg";
    public string GpgPath
    {
        get { return _gpgPath; }
        set
        {
            _gpg?.Dispose();
            _gpgPath = value;
        }
    }

    public string GpgPublicKey
    {
        get
        {
            string? publicKey = null;
            Gpg.Invoke("--export --armor", ref publicKey);
            return File.ReadAllText(publicKey);
        }
    }

    private Gpg? _gpg;
    public Gpg Gpg => _gpg ??= new Gpg(GpgPath);

    public string[] AllArchitectures { get; set; } = new string[] { "amd64", "i386", "armel", "armhf" };

    public FileCache? FileCache { get; set; } = new("cache");

    public DebianRepositoryConfiguration(string storageRoot)
    {
        StorageRoot = storageRoot;
    }

    protected virtual void Dispose(bool disposing)
    {
        try
        {
            _gpg?.Dispose();
        }
        // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable S2486
#pragma warning disable S108
        // ReSharper disable once CatchAllClause
        catch { }
#pragma warning restore S2486
#pragma warning restore S108
    }

    #region destructor
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    ~DebianRepositoryConfiguration()
    {
        Dispose(false);
    }
    #endregion

    public void LoadPrivateKey(string privateKey)
    {
        Gpg.AddPrivateKey(privateKey);
    }
}
