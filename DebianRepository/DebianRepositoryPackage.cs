namespace ArxOne.Debian;

public class DebianRepositoryPackage
{
    // -- key
    public string Package { get; set; }
    public string Version { get; set; }
    public string Architecture { get; set; }

    // -- description (from .deb)
    public string? Priority { get; set; }
    public string? Section { get; set; }
    public long? InstalledSize { get; set; }
    public string? Maintainer { get; set; }
    public string? Depends { get; set; }
    public string[]? Description { get; set; }
    public string? Homepage { get; set; }

    // -- information for Packages file
    public string? Filename { get; set; }
    public long? Size { get; set; }
    // ReSharper disable once InconsistentNaming
    public byte[]? MD5sum { get; set; }
    // ReSharper disable once InconsistentNaming
    public byte[]? SHA1 { get; set; }
    // ReSharper disable once InconsistentNaming
    public byte[]? SHA256 { get; set; }
    // ReSharper disable once InconsistentNaming
    public byte[]? SHA512 { get; set; }

    public DebianRepositoryPackage(string package, string version, string architecture)
    {
        Package = package;
        Version = version;
        Architecture = architecture;
    }

    private DebianRepositoryPackage() { }
}
