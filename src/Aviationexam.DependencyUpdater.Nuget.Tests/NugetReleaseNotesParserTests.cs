using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Parsers;
using System.IO;
using System.Text;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class NugetReleaseNotesParserTests
{
    [Fact]
    public void ParseRegistrationIndex_InlinedPage_ReturnsLeaves()
    {
        // Arrange
        const string json = """
            {
              "items": [
                {
                  "@id": "https://feed/registrations2/pkg/index.json#page/1.0.0/1.0.2",
                  "items": [
                    { "catalogEntry": { "id": "Pkg", "version": "1.0.0", "listed": true, "projectUrl": "https://github.com/owner/pkg" } },
                    { "catalogEntry": { "id": "Pkg", "version": "1.0.1", "listed": false, "releaseNotes": "Fixed a bug" } },
                    { "catalogEntry": { "id": "Pkg", "version": "1.0.2", "releaseNotes": "" } },
                    { "catalogEntry": "https://feed/catalog/not-inlined.json" }
                  ]
                }
              ]
            }
            """;

        // Act
        var result = NugetReleaseNotesParser.ParseRegistrationIndex(ToStream(json));

        // Assert
        Assert.Empty(result.ExternalPageUrls);
        Assert.Collection(
            result.Leaves,
            leaf =>
            {
                Assert.Equal("1.0.0", leaf.Version.ToNormalizedString());
                Assert.True(leaf.Listed);
                Assert.Null(leaf.ReleaseNotes);
                Assert.Equal("https://github.com/owner/pkg", leaf.ProjectUrl);
            },
            leaf =>
            {
                Assert.Equal("1.0.1", leaf.Version.ToNormalizedString());
                Assert.False(leaf.Listed);
                Assert.Equal("Fixed a bug", leaf.ReleaseNotes);
            },
            leaf =>
            {
                Assert.Equal("1.0.2", leaf.Version.ToNormalizedString());
                Assert.True(leaf.Listed);
                Assert.Null(leaf.ReleaseNotes);
            }
        );
    }

    [Fact]
    public void ParseRegistrationIndex_ExternalPages_ReturnsPageUrls()
    {
        // Arrange
        const string json = """
            {
              "items": [
                { "@id": "https://feed/registrations2/pkg/page/1.0.0/1.5.0.json", "lower": "1.0.0", "upper": "1.5.0" },
                { "@id": "https://feed/registrations2/pkg/page/2.0.0/2.5.0.json", "lower": "2.0.0", "upper": "2.5.0" }
              ]
            }
            """;

        // Act
        var result = NugetReleaseNotesParser.ParseRegistrationIndex(ToStream(json));

        // Assert
        Assert.Empty(result.Leaves);
        Assert.Equal(
            [
                "https://feed/registrations2/pkg/page/1.0.0/1.5.0.json",
                "https://feed/registrations2/pkg/page/2.0.0/2.5.0.json",
            ],
            result.ExternalPageUrls
        );
    }

    [Fact]
    public void ParseRegistrationPage_ReturnsLeaves()
    {
        // Arrange
        const string json = """
            {
              "items": [
                { "catalogEntry": { "id": "Pkg", "version": "2.0.0-beta.1", "listed": true, "releaseNotes": "Beta" } }
              ]
            }
            """;

        // Act
        var result = NugetReleaseNotesParser.ParseRegistrationPage(ToStream(json));

        // Assert
        var leaf = Assert.Single(result);
        Assert.Equal("2.0.0-beta.1", leaf.Version.ToNormalizedString());
        Assert.Equal("Beta", leaf.ReleaseNotes);
    }

    [Fact]
    public void ParseNuspecMetadata_ReturnsReleaseNotesAndRepository()
    {
        // Arrange
        const string nuspec = """
            <?xml version="1.0" encoding="utf-8"?>
            <package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
              <metadata>
                <id>Aviationexam.Core.Common.Contracts</id>
                <version>0.1.3215</version>
                <projectUrl>https://github.com/aviationexam/project</projectUrl>
                <releaseNotes>SharedCore Common 0.1.3215
            Changes since 0.1.3211:
            - !22542 Generate SharedCore release notes from merged PRs
            Compare: https://dev.azure.com/aviationexam/x/_git/y/branchCompare?baseVersion=GC1&amp;targetVersion=GC2</releaseNotes>
                <repository type="git" url="https://github.com/aviationexam/repository.git" commit="247ad4577743af2aca29cb260d00726de777caf9" />
              </metadata>
            </package>
            """;

        // Act
        var result = NugetReleaseNotesParser.ParseNuspecMetadata(ToStream(nuspec));

        // Assert
        Assert.Equal(
            """
            SharedCore Common 0.1.3215
            Changes since 0.1.3211:
            - !22542 Generate SharedCore release notes from merged PRs
            Compare: https://dev.azure.com/aviationexam/x/_git/y/branchCompare?baseVersion=GC1&targetVersion=GC2
            """.ReplaceLineEndings("\n"),
            result.ReleaseNotes?.ReplaceLineEndings("\n")
        );
        Assert.Equal("https://github.com/aviationexam/repository.git", result.RepositoryUrl);
        Assert.Equal("https://github.com/aviationexam/project", result.ProjectUrl);
    }

    [Theory]
    [InlineData("<package><metadata><id>Pkg</id></metadata></package>")]
    [InlineData("<package><metadata><releaseNotes>   </releaseNotes><repository type=\"git\" /></metadata></package>")]
    public void ParseNuspecMetadata_WithoutValues_ReturnsNulls(string nuspec)
    {
        // Act
        var result = NugetReleaseNotesParser.ParseNuspecMetadata(ToStream(nuspec));

        // Assert
        Assert.Equal(new NugetNuspecMetadata(null, null, null), result);
    }

    private static MemoryStream ToStream(string content) => new(Encoding.UTF8.GetBytes(content));
}
