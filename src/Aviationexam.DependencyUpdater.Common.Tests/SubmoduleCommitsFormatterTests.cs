using Aviationexam.DependencyUpdater.Interfaces;
using System.Linq;
using Xunit;

namespace Aviationexam.DependencyUpdater.Common.Tests;

public class SubmoduleCommitsFormatterTests
{
    private const string FromSha = "735163f31420141fabb13622b470ae3e7623a32c";
    private const string ToSha = "247ad4577743af2aca29cb260d00726de777caf9";

    [Theory]
    [InlineData("https://github.com/aviationexam/repo.git", "https://github.com/aviationexam/repo")]
    [InlineData("git@github.com:aviationexam/repo.git", "https://github.com/aviationexam/repo")]
    [InlineData("ssh://git@github.com/aviationexam/repo.git", "https://github.com/aviationexam/repo")]
    [InlineData("https://aviationexam@dev.azure.com/aviationexam/project/_git/repo", "https://dev.azure.com/aviationexam/project/_git/repo")]
    [InlineData("git@ssh.dev.azure.com:v3/aviationexam/project/repo", "https://dev.azure.com/aviationexam/project/_git/repo")]
    [InlineData("https://gitlab.com/group/sub/repo.git/", "https://gitlab.com/group/sub/repo")]
    [InlineData("https://git.example.com:8443/owner/repo.git", "https://git.example.com:8443/owner/repo")]
    [InlineData("http://git.example.com/owner/repo.git", "http://git.example.com/owner/repo")]
    [InlineData("https://dev.azure.com/org/My%20Project/_git/My%20Repo", "https://dev.azure.com/org/My%20Project/_git/My%20Repo")]
    public void GetRepositoryWebUrl_MapsRemoteUrls(string remoteUrl, string expected)
    {
        // Act
        var result = SubmoduleCommitsFormatter.GetRepositoryWebUrl(remoteUrl);

        // Assert
        Assert.Equal(expected, result?.AbsoluteUri);
    }

    [Fact]
    public void GetCommitAndCompareUrl_KeepEscapedPath()
    {
        // Arrange
        var repositoryUrl = SubmoduleCommitsFormatter.GetRepositoryWebUrl("https://dev.azure.com/org/My%20Project/_git/repo");

        // Act
        var commitUrl = SubmoduleCommitsFormatter.GetCommitUrl(repositoryUrl, ToSha);
        var compareUrl = SubmoduleCommitsFormatter.GetCompareUrl(repositoryUrl, FromSha, ToSha);

        // Assert
        Assert.Equal($"https://dev.azure.com/org/My%20Project/_git/repo/commit/{ToSha}", commitUrl);
        Assert.Equal($"https://dev.azure.com/org/My%20Project/_git/repo/branchCompare?baseVersion=GC{FromSha}&targetVersion=GC{ToSha}&_a=commits", compareUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("../relative/repo.git")]
    [InlineData("/srv/git/repo.git")]
    public void GetRepositoryWebUrl_UnsupportedRemotes_ReturnsNull(string? remoteUrl)
    {
        // Act
        var result = SubmoduleCommitsFormatter.GetRepositoryWebUrl(remoteUrl);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCompareUrl_GitHub()
    {
        // Act
        var result = SubmoduleCommitsFormatter.GetCompareUrl(SubmoduleCommitsFormatter.GetRepositoryWebUrl("https://github.com/owner/repo.git"), FromSha, ToSha);

        // Assert
        Assert.Equal($"https://github.com/owner/repo/compare/{FromSha}...{ToSha}", result);
    }

    [Fact]
    public void GetCompareUrl_AzureDevOps()
    {
        // Act
        var result = SubmoduleCommitsFormatter.GetCompareUrl(SubmoduleCommitsFormatter.GetRepositoryWebUrl("https://dev.azure.com/org/project/_git/repo"), FromSha, ToSha);

        // Assert
        Assert.Equal($"https://dev.azure.com/org/project/_git/repo/branchCompare?baseVersion=GC{FromSha}&targetVersion=GC{ToSha}&_a=commits", result);
    }

    [Theory]
    [InlineData("Fix the thing\n\nLonger body", "Fix the thing")]
    [InlineData("  Trim me  \r\nbody", "Trim me")]
    [InlineData("Use *bold* and `code` and <tag>", "Use \\*bold* and \\`code` and \\<tag>")]
    [InlineData("Merged PR 22542: Fix #12 by @octocat", "Merged PR 22542: Fix #\u200B12 by @\u200Boctocat")]
    [InlineData("file_name.cs and snake_case stay", "file_name.cs and snake_case stay")]
    public void GetTitle_UsesFirstLineEscapedAndScrubbed(string message, string expected)
    {
        // Act
        var result = SubmoduleCommitsFormatter.GetTitle(message);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetTitle_LongTitle_IsTruncated()
    {
        // Act
        var result = SubmoduleCommitsFormatter.GetTitle(new string('a', 100));

        // Assert
        Assert.Equal(SubmoduleCommitsFormatter.MaxTitleLength, result.Length);
        Assert.EndsWith("...", result);
    }

    [Fact]
    public void BuildSection_WithoutCommits_ReturnsNull()
    {
        // Act
        var result = SubmoduleCommitsFormatter.BuildSection(new SubmoduleUpdate("sub", "https://github.com/owner/repo", FromSha, ToSha, []));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void BuildSection_FewCommits_ListsAllWithCompareLink()
    {
        // Arrange
        var update = new SubmoduleUpdate("sub", "git@github.com:owner/repo.git", FromSha, ToSha, [
            new SubmoduleCommit(ToSha, "Second change\n\nbody"),
            new SubmoduleCommit("1111111aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "First change"),
        ]);

        // Act
        var result = SubmoduleCommitsFormatter.BuildSection(update);

        // Assert
        Assert.Equal(
            [
                "## Commits",
                "",
                $"- [`247ad45`](https://github.com/owner/repo/commit/{ToSha}) Second change",
                "- [`1111111`](https://github.com/owner/repo/commit/1111111aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa) First change",
                $"- See full diff in [compare view](https://github.com/owner/repo/compare/{FromSha}...{ToSha})",
            ],
            result!.ReplaceLineEndings("\n").Split('\n')
        );
    }

    [Fact]
    public void BuildSection_ManyCommits_ListsNewestTenAndPointsToCompareView()
    {
        // Arrange
        var commits = Enumerable.Range(0, 15)
            .Select(i => new SubmoduleCommit($"{i:x}".PadLeft(40, '0'), $"Change {i}"))
            .ToList();
        var update = new SubmoduleUpdate("sub", "https://dev.azure.com/org/project/_git/repo", FromSha, ToSha, commits);

        // Act
        var result = SubmoduleCommitsFormatter.BuildSection(update);

        // Assert
        var lines = result!.ReplaceLineEndings("\n").Split('\n');
        Assert.Equal(SubmoduleCommitsFormatter.MaxCommits + 3, lines.Length);
        Assert.Equal($"- [`0000000`](https://dev.azure.com/org/project/_git/repo/commit/{commits[0].Sha}) Change 0", lines[2]);
        Assert.Equal("- Additional commits viewable in [compare view](https://dev.azure.com/org/project/_git/repo/branchCompare?baseVersion=GC" + FromSha + "&targetVersion=GC" + ToSha + "&_a=commits)", lines[^1]);
    }

    [Fact]
    public void BuildSection_UnknownRemote_ListsCommitsWithoutLinks()
    {
        // Arrange
        var commits = Enumerable.Range(0, 12)
            .Select(i => new SubmoduleCommit($"{i:x}".PadLeft(40, '0'), $"Change {i}"))
            .ToList();
        var update = new SubmoduleUpdate("sub", "../local/repo.git", FromSha, ToSha, commits);

        // Act
        var result = SubmoduleCommitsFormatter.BuildSection(update);

        // Assert
        var lines = result!.ReplaceLineEndings("\n").Split('\n');
        Assert.Equal("- `0000000` Change 0", lines[2]);
        Assert.Equal("- … and 2 more commits", lines[^1]);
    }
}
