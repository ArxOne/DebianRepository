using System;
using System.Collections.Generic;

namespace ArxOne.Debian.Version;

public class DebianVersionComparer : IComparer<DebianVersion>, IEqualityComparer<DebianVersion>
{
    public static readonly DebianVersionComparer Default = new();

    public int Compare(DebianVersion? x, DebianVersion? y)
    {
        if (x is null)
            return y is null ? 0 : -1;
        if (y is null)
            return 1;
        var epochCompare = x.Epoch.CompareTo(y.Epoch);
        if (epochCompare != 0)
            return epochCompare;

        var upstreamVersionCompare = VersionCompare(x.UpstreamVersion, y.UpstreamVersion);
        if (upstreamVersionCompare != 0)
            return upstreamVersionCompare;

        return VersionCompare(x.DebianRevision, y.DebianRevision);
    }

    private int VersionCompare(string? a, string? b)
    {
        if (a is null)
            return b is null ? 0 : -1;
        if (b is null)
            return 1;
        // shortcut
        if (a == b)
            return 0;

        var (aHeader, aRemainder) = GetAlphaHeader(a);
        var (bHeader, bRemainder) = GetAlphaHeader(b);
        var headersCompare = CompareHeader(aHeader, bHeader);
        if (headersCompare != 0)
            return headersCompare;
        return CompareRemainders(aRemainder, bRemainder);
    }

    private static int CompareRemainders(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
    {
        var a0 = FirstOrDefault(a);
        var b0 = FirstOrDefault(b);
        if (a0 == '.')
            return CompareRemainders(a[1..], b);
        if (b0 == '.')
            return CompareRemainders(a, b[1..]);
        if ((char.IsDigit(a0) || a0 == 0) && (char.IsDigit(b0) || b0 == 0))
            return CompareIntRemainders(a, b);
        return CompareHeader(a, b);
    }

    private static int CompareIntRemainders(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
    {
        var aIndex = GetIndexOf(a, c => !char.IsDigit(c)) ?? a.Length;
        var bIndex = GetIndexOf(b, c => !char.IsDigit(c)) ?? b.Length;
        var aValue = aIndex == 0 ? 0 : int.Parse(a[..aIndex]);
        var bValue = bIndex == 0 ? 0 : int.Parse(b[..bIndex]);
        var compare = aValue - bValue;
        if (compare != 0)
            return compare;
        if (aIndex == a.Length && bIndex == b.Length)
            return 0;
        return CompareRemainders(a[aIndex..], b[bIndex..]);
    }

    private static int CompareHeader(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
    {
        // “~” starts earlier
        var a0 = FirstOrDefault(a);
        var b0 = FirstOrDefault(b);
        if (a0 == '~')
        {
            if (b0 == '~')
                return CompareHeader(a[1..], b[1..]);
            return -1;
        }
        if (b0 == '~')
            return 1;

        // letters then
        if (char.IsLetter(a0))
        {
            if (char.IsLetter(b0))
                return CompareOrNextHeader(a, b);
            return 1;
        }
        if (char.IsLetter(b0))
            return -1;

        // not sure of it
        return CompareOrNextHeader(a, b);
    }

    private static int CompareOrNextHeader(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
    {
        var a0 = FirstOrDefault(a);
        var r = a0 - FirstOrDefault(b);
        if (r != 0)
            return r;
        if (a0 == 0)
            return 0;
        return CompareHeader(a[1..], b[1..]);
    }

    private static char FirstOrDefault(ReadOnlySpan<char> s)
    {
        if (s.Length == 0)
            return (char)0;
        return s[0];
    }

    private static (string Header, string Remainder) GetAlphaHeader(string v)
    {
        var index = GetIndexOf(v, char.IsDigit);
        return index switch
        {
            null => (v, ""),
            0 => ("", v),
            _ => (v[..index.Value], v[index.Value..])
        };
    }

    private static int? GetIndexOf(ReadOnlySpan<char> s, Predicate<char> predicate)
    {
        for (int index = 0; index < s.Length; index++)
        {
            if (predicate(s[index]))
                return index;
        }

        return null;
    }

    public bool Equals(DebianVersion? x, DebianVersion? y)
    {
        return Compare(x, y) == 0;
    }

    public int GetHashCode(DebianVersion version)
    {
        return version.Epoch.GetHashCode() ^ version.UpstreamVersion.GetHashCode() ^ (version.DebianRevision?.GetHashCode() ?? -1);
    }
}
