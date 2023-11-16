namespace ArxOne.Debian;

public class DebianRepositoryConfiguration
{
    public string GpgPublicKeyName { get; set; } = "public.gpg";

    public string GpgPath { get; set; }

    public string GpgPrivateKey { get; set; }
}
