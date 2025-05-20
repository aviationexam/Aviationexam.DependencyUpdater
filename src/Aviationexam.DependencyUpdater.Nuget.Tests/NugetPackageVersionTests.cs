using Aviationexam.DependencyUpdater.Common;
using System;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class NugetPackageVersionTests
{
    [Theory]
    [MemberData(nameof(CompareData))]
    public void CompareWorks(
        PackageVersion left, PackageVersion right,
        int expected,
        bool isGreater, bool isEqual, bool isSmaller
    )
    {
        Assert.Equal(expected, left.CompareTo(right));
        Assert.Equal(isGreater, left > right);
        Assert.Equal(isGreater || isEqual, left >= right);
        Assert.Equal(isEqual, left == right);
        Assert.Equal(isSmaller, left < right);
        Assert.Equal(isSmaller || isEqual, left <= right);
    }

    public static TheoryData<PackageVersion, PackageVersion, int, bool, bool, bool> CompareData() => new()
    {
        {
            new PackageVersion(new Version("1.0.0"), false, [], NugetReleaseLabelComparer.Instance),
            new PackageVersion(new Version("1.0.0"), false, [], NugetReleaseLabelComparer.Instance),
            0,
            false,
            true,
            false
        },
        {
            new PackageVersion(new Version("1.0.1"), false, [], NugetReleaseLabelComparer.Instance),
            new PackageVersion(new Version("1.0.0"), false, [], NugetReleaseLabelComparer.Instance),
            1,
            true,
            false,
            false
        },
        {
            new PackageVersion(new Version("1.0.0"), false, [], NugetReleaseLabelComparer.Instance),
            new PackageVersion(new Version("1.0.1"), false, [], NugetReleaseLabelComparer.Instance),
            -1,
            false,
            false,
            true
        },
        {
            new PackageVersion(new Version("1.0.0"), false, [], NugetReleaseLabelComparer.Instance),
            new PackageVersion(new Version("1.0.0"), true, ["beta"], NugetReleaseLabelComparer.Instance),
            1,
            true,
            false,
            false
        },
        {
            new PackageVersion(new Version("1.0.0"), true, ["beta"], NugetReleaseLabelComparer.Instance),
            new PackageVersion(new Version("1.0.0"), false, [], NugetReleaseLabelComparer.Instance),
            -1,
            false,
            false,
            true
        },
        {
            new PackageVersion(new Version("1.0.0"), true, ["beta"], NugetReleaseLabelComparer.Instance),
            new PackageVersion(new Version("1.0.0"), true, ["beta"], NugetReleaseLabelComparer.Instance),
            0,
            false,
            true,
            false
        },
        {
            new PackageVersion(new Version("1.0.0"), true, ["beta", "2"], NugetReleaseLabelComparer.Instance),
            new PackageVersion(new Version("1.0.0"), true, ["beta", "1"], NugetReleaseLabelComparer.Instance),
            1,
            true,
            false,
            false
        },
        {
            new PackageVersion(new Version("1.0.0"), true, ["beta", "1"], NugetReleaseLabelComparer.Instance),
            new PackageVersion(new Version("1.0.0"), true, ["beta", "2"], NugetReleaseLabelComparer.Instance),
            -1,
            false,
            false,
            true
        },
        {
            new PackageVersion(new Version("1.0.0"), true, ["alpha"], NugetReleaseLabelComparer.Instance),
            new PackageVersion(new Version("1.0.0"), true, ["beta"], NugetReleaseLabelComparer.Instance),
            -1,
            false,
            false,
            true
        },
        {
            new PackageVersion(new Version("1.0.0"), true, ["alpha", "2"], NugetReleaseLabelComparer.Instance),
            new PackageVersion(new Version("1.0.0"), true, ["beta", "1"], NugetReleaseLabelComparer.Instance),
            -1,
            false,
            false,
            true
        },
        {
            new PackageVersion(new Version("1.0.0"), true, ["beta", "1"], NugetReleaseLabelComparer.Instance),
            new PackageVersion(new Version("1.0.0"), true, ["alpha", "2"], NugetReleaseLabelComparer.Instance),
            1,
            true,
            false,
            false
        },
    };
}
