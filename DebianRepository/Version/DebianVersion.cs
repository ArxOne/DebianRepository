using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ArxOne.Debian.Version;

[DebuggerDisplay("{Literal}")]
public partial class DebianVersion
{
    public long Epoch { get; }
    public string UpstreamVersion { get; }
    public string? DebianRevision { get; }

    public string Literal
    {
        get
        {
            if (Epoch == 0 && DebianRevision is null)
                return UpstreamVersion;
            if (Epoch > 0)
            {
                if (DebianRevision is null)
                    return $"{Epoch}:{UpstreamVersion}";
                return $"{Epoch}:{UpstreamVersion}-{DebianRevision}";
            }
            return $"{UpstreamVersion}-{DebianRevision}";
        }
    }

    [GeneratedRegex(@"((?<epoch>[0-9]+)\:)?(?<upstream_version_and_debian_revision>[A-Za-z0-9+.~-]+)?")]
    private static partial Regex VersionEx();

    public static DebianVersion? TryParse(string literal)
    {
        var match = VersionEx().Match(literal);
        if (!match.Success)
            return null;
        var epochLiteral = match.Groups["epoch"].Value;
        var epoch = epochLiteral.Length > 0 ? long.Parse(epochLiteral) : 0;
        var version = match.Groups["upstream_version_and_debian_revision"].Value;
        var lastHyphenIndex = version.LastIndexOf('-');
        var (upstreamVersion, debianRevision) = lastHyphenIndex >= 0 ? (version[..lastHyphenIndex], version[(lastHyphenIndex + 1)..]) : (version, null);
        return new DebianVersion(epoch, upstreamVersion, debianRevision);
    }

    private DebianVersion(long epoch, string upstreamVersion, string? debianRevision)
    {
        Epoch = epoch;
        UpstreamVersion = upstreamVersion;
        DebianRevision = debianRevision;
    }
}