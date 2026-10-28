using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using NuGet.Versioning;
using System.Linq;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class NugetReleaseNotesFetcherTests
{
    private static readonly NugetRegistrationLeaf[] Leaves =
    [
        Leaf("0.1.3208"),
        Leaf("0.1.3210"),
        Leaf("0.1.3211"),
        Leaf("0.1.3212-feature.1"),
        Leaf("0.1.3213", listed: false),
        Leaf("0.1.3215"),
        Leaf("0.1.3215"),
        Leaf("0.1.3216"),
        Leaf("0.1.3217-beta.1"),
    ];

    [Fact]
    public void SelectVersions_ReturnsListedStableVersionsInRangeOnce()
    {
        // Act
        var result = NugetReleaseNotesFetcher.SelectVersions(Leaves, Version("0.1.3210"), Version("0.1.3215"));

        // Assert
        Assert.Equal(["0.1.3211", "0.1.3215"], result.Select(x => x.Version.ToNormalizedString()));
    }

    [Fact]
    public void SelectVersions_PrereleaseTarget_IncludesPrereleaseVersions()
    {
        // Act
        var result = NugetReleaseNotesFetcher.SelectVersions(Leaves, Version("0.1.3215"), Version("0.1.3217-beta.1"));

        // Assert
        Assert.Equal(["0.1.3216", "0.1.3217-beta.1"], result.Select(x => x.Version.ToNormalizedString()));
    }

    [Fact]
    public void SelectVersions_UnknownCurrentVersion_ReturnsOnlyTarget()
    {
        // Act
        var result = NugetReleaseNotesFetcher.SelectVersions(Leaves, fromVersion: null, Version("0.1.3215"));

        // Assert
        Assert.Equal(["0.1.3215"], result.Select(x => x.Version.ToNormalizedString()));
    }

    private static NugetRegistrationLeaf Leaf(string version, bool listed = true) => new(NuGetVersion.Parse(version), listed, null);

    private static PackageVersion Version(string version) => NuGetVersion.Parse(version).MapToPackageVersion();
}
