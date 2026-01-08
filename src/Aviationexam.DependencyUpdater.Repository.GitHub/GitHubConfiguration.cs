using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Interfaces.Repository;

namespace Aviationexam.DependencyUpdater.Repository.GitHub;

public sealed class GitHubConfiguration : IRepositoryPlatformConfiguration
{
    public EPlatformSelection Platform => EPlatformSelection.GitHub;

    /// <summary>
    /// The GitHub repository owner (organization or user account name).
    /// </summary>
    public required string Owner { get; set; }

    /// <summary>
    /// The name of the GitHub repository to operate on.
    /// </summary>
    public required string Repository { get; set; }

    /// <summary>
    /// The GitHub personal access token (PAT) used for authentication.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Close and reopen pull request after creation to trigger CI workflows.
    /// This is a workaround for GitHub Actions GITHUB_TOKEN limitation where
    /// workflows do not trigger on pull requests created by the token.
    /// </summary>
    public bool CyclePullRequestOnCreation { get; set; }
}
