using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Interfaces.Repository;
using System;

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
    /// Optional proxy address for GitHub API authentication.
    /// When set, PR creation requests are routed through this proxy to enable CI triggers.
    /// </summary>
    public Uri? AuthenticationProxyAddress { get; set; }
}
