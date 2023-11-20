using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ArxOne.Debian.Version;
using NUnit.Framework;

namespace DebianRepositoryTest;

#pragma warning disable NUnit2005

[TestFixture]
public class DebianVersionComparerTest
{
    // https://www.debian.org/doc/debian-policy/ch-controlfields.html#s-f-version

    [Test]
    public void SortUpstreamVersionsAlphaParts()
    {
        // laziness of actually enumerating all possible combinations
        for (int i = 0; i < (5 * 4 * 3 * 2) * 3; i++)
        {
            var versionsSpan = new Span<DebianVersion>(new string[] { "", "a", "~", "~~", "~~a" }.Select(l => DebianVersion.TryParse(l)!).ToArray());
            RandomNumberGenerator.Shuffle(versionsSpan);
            var allVersions = new List<DebianVersion>(versionsSpan.ToArray());
            allVersions.Sort(DebianVersionComparer.Default);
            //  ~~, ~~a, ~, the empty part, a
            Assert.AreEqual("~~", allVersions[0].Literal);
            Assert.AreEqual("~~a", allVersions[1].Literal);
            Assert.AreEqual("~", allVersions[2].Literal);
            Assert.AreEqual("", allVersions[3].Literal);
            Assert.AreEqual("a", allVersions[4].Literal);
        }
    }

    [Test]
    [TestCase("1.0", "1.0.0", 0)]
    [TestCase("1.1", "1.0", 1)]
    [TestCase("1.1", "1.0", 1)]
    [TestCase("2.06-3~deb11u6", "2.06-3~deb11u7", -1)]
    public void Compare(string a, string b, int expectedSign)
    {
        var aVersion = DebianVersion.TryParse(a);
        var bVersion = DebianVersion.TryParse(b);
        var compare = DebianVersionComparer.Default.Compare(aVersion, bVersion);
        Assert.AreEqual(expectedSign, Math.Sign(compare));
    }
}
