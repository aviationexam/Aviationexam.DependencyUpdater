using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using NuGet.Versioning;
using System.Collections.Generic;
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

    [Fact]
    public void ResolveRepositoryUrl_PrefersNewestNuspecRepositoryOverOlderProjectUrls()
    {
        // Arrange
        NugetRegistrationLeaf[] versions =
        [
            new(NuGetVersion.Parse("1.0.0"), true, null, "https://docs.example.com/"),
            new(NuGetVersion.Parse("1.1.0"), true, null, "https://docs.example.com/"),
            new(NuGetVersion.Parse("2.0.0"), true, null, null),
        ];
        var nuspecs = new Dictionary<NuGetVersion, NugetNuspecMetadata?>
        {
            [NuGetVersion.Parse("1.0.0")] = new(null, null, "https://docs.example.com/"),
            [NuGetVersion.Parse("2.0.0")] = new(null, "https://github.com/owner/repo.git", "https://docs.example.com/"),
        };

        // Act
        var result = NugetReleaseNotesFetcher.ResolveRepositoryUrl(versions, nuspecs);

        // Assert
        Assert.Equal("https://github.com/owner/repo.git", result);
    }

    [Fact]
    public void ResolveRepositoryUrl_WithoutRepository_FallsBackToNewestProjectUrl()
    {
        // Arrange
        NugetRegistrationLeaf[] versions =
        [
            new(NuGetVersion.Parse("1.0.0"), true, null, "https://old.example.com/"),
            new(NuGetVersion.Parse("2.0.0"), true, null, "https://new.example.com/"),
        ];
        var nuspecs = new Dictionary<NuGetVersion, NugetNuspecMetadata?>
        {
            [NuGetVersion.Parse("2.0.0")] = new(null, null, null),
        };

        // Act
        var result = NugetReleaseNotesFetcher.ResolveRepositoryUrl(versions, nuspecs);

        // Assert
        Assert.Equal("https://new.example.com/", result);
    }

    private static NugetRegistrationLeaf Leaf(string version, bool listed = true) => new(NuGetVersion.Parse(version), listed, null);

    private static PackageVersion Version(string version) => NuGetVersion.Parse(version).MapToPackageVersion();
}
