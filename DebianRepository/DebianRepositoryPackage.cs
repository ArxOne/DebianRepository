using ArxOne.Debian.Version;

namespace ArxOne.Debian;

public class DebianRepositoryPackage
{
    // -- description (from .deb)
    public string Package { get; set; }
    public string Version { get; set; }
    public string Architecture { get; set; }
    public string? Priority { get; set; }
    public string? Section { get; set; }
    public long? InstalledSize { get; set; }
    public string? Maintainer { get; set; }
    public string? Depends { get; set; }
    public string[]? Description { get; set; }
    public string? Homepage { get; set; }

    private DebianVersion? _debianVersion;
    public DebianVersion? DebianVersion => _debianVersion ??= DebianVersion.TryParse(Version);

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
