namespace ArxOne.Debian;

public class DebianRepositoryConfiguration
{
    public string Root { get; set; } = "/debian";

    public string GpgPublicKeyName { get; set; } = "public.gpg";

    public string GpgPath { get; set; }

    public string GpgPrivateKey { get; set; }
}
