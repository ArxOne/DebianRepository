namespace ArxOne.Debian;

public class DebianRepositoryPackage
{
    public string Package { get; set; }
    public string Priority { get; set; }
    public string Section { get; set; }
    // Installed-Size
    public long InstalledSize { get; set; }
    public string Maintainer { get; set; }
    public string Architecture { get; set; }
    public string Version { get; set; }
    public string Depends { get; set; }
    public string Filename { get; set; }
    public long Size { get; set; }
    public string MD5sum { get; set; }
    public string SHA1 { get; set; }
    public string SHA256 { get; set; }
    public string SHA512 { get; set; }
    public string[] Description { get; set; }
    public string Homepage { get; set; }
}