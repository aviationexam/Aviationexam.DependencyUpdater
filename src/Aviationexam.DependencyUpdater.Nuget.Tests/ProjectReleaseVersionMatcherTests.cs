using Aviationexam.DependencyUpdater.Interfaces.ReleaseNotes;
using Aviationexam.DependencyUpdater.Nuget.Services;
using NuGet.Versioning;
using System.Linq;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class ProjectReleaseVersionMatcherTests
{
    [Theory]
    [InlineData("v1.2.3", "v1.2.3", "1.2.3")]
    [InlineData("Release 1.2.3", "release-1.2.3", "1.2.3")]
    [InlineData("", "1.2.3-beta.1", "1.2.3-beta.1")]
    [InlineData("Some title without version", "v2.0.0", "2.0.0")]
    public void MatchReleases_RepoScopedNames_ParsesVersion(string name, string tagName, string expectedVersion)
    {
        // Act
        var result = ProjectReleaseVersionMatcher.MatchReleases([new ProjectRelease(name, tagName, "body")], "Some.Package");

        // Assert
        var (version, release) = Assert.Single(result);
        Assert.Equal(NuGetVersion.Parse(expectedVersion), version);
        Assert.Equal(tagName, release.TagName);
    }

    [Fact]
    public void MatchReleases_WithoutParsableVersion_IsSkipped()
    {
        // Act
        var result = ProjectReleaseVersionMatcher.MatchReleases([new ProjectRelease("Nightly", "nightly", "body")], "Some.Package");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MatchReleases_PackageScopedReleases_TakePrecedenceOverRepoScoped()
    {
        // Arrange
        ProjectRelease[] releases =
        [
            new("Some.Package v1.0.0", "Some.Package/1.0.0", "package 1.0.0"),
            new("Other.Package v1.0.0", "Other.Package/1.0.0", "other 1.0.0"),
            new("v1.0.0", "v1.0.0", "repo 1.0.0"),
            new("v1.1.0", "v1.1.0", "repo 1.1.0"),
        ];

        // Act
        var result = ProjectReleaseVersionMatcher.MatchReleases(releases, "Some.Package");

        // Assert
        Assert.Equal("package 1.0.0", result[NuGetVersion.Parse("1.0.0")].Body);
        Assert.Equal("repo 1.1.0", result[NuGetVersion.Parse("1.1.0")].Body);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void MatchReleases_OtherPackageScopedReleases_UsedOnlyWithoutOwnReleases()
    {
        // Arrange
        ProjectRelease[] releases =
        [
            new("Other.Package v1.0.0", "Other.Package-v1.0.0", "other 1.0.0"),
            new("v1.1.0", "v1.1.0", "repo 1.1.0"),
        ];

        // Act
        var result = ProjectReleaseVersionMatcher.MatchReleases(releases, "Some.Package");

        // Assert
        Assert.Equal(["1.0.0", "1.1.0"], result.Keys.OrderBy(x => x).Select(x => x.ToNormalizedString()));
        Assert.Equal("other 1.0.0", result[NuGetVersion.Parse("1.0.0")].Body);
    }
}
