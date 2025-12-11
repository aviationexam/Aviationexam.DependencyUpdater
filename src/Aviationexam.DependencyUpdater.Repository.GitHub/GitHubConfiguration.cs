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
}
