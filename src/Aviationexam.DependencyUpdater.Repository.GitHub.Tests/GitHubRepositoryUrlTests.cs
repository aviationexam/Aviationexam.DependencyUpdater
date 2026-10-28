using System;
using Xunit;

namespace Aviationexam.DependencyUpdater.Repository.GitHub.Tests;

public class GitHubRepositoryUrlTests
{
    [Theory]
    [InlineData("https://github.com/FluentValidation/FluentValidation", "FluentValidation", "FluentValidation")]
    [InlineData("https://github.com/FluentValidation/FluentValidation.git", "FluentValidation", "FluentValidation")]
    [InlineData("https://www.github.com/owner/repo/", "owner", "repo")]
    [InlineData("https://github.com/owner/repo/tree/main/src", "owner", "repo")]
    [InlineData("http://github.com/owner/repo", "owner", "repo")]
    public void TryParse_GitHubUrls_ReturnsOwnerAndRepository(string url, string expectedOwner, string expectedRepository)
    {
        // Act
        var result = GitHubRepositoryUrl.TryParse(new Uri(url), out var owner, out var repository);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedOwner, owner);
        Assert.Equal(expectedRepository, repository);
    }

    [Theory]
    [InlineData("https://dev.azure.com/aviationexam/project/_git/repo")]
    [InlineData("https://gitlab.com/owner/repo")]
    [InlineData("https://github.com/owner")]
    [InlineData("https://github.com/")]
    [InlineData("git@github.com:owner/repo.git")]
    public void TryParse_OtherUrls_ReturnsFalse(string url)
    {
        // Act
        var result = GitHubRepositoryUrl.TryParse(new Uri(url, UriKind.RelativeOrAbsolute), out _, out _);

        // Assert
        Assert.False(result);
    }
}
