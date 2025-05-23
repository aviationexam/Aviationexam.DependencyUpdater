using System;

namespace Aviationexam.DependencyUpdater.Common;

/// <summary>
/// Repository configuration for NuGet update operations.
/// </summary>
public sealed class RepositoryConfig
{
    /// <summary>
    /// The path to the repository.
    /// </summary>
    public string RepositoryPath { get; }

    /// <summary>
    /// The subdirectory path within the repository.
    /// </summary>
    public string? SubdirectoryPath { get; }

    /// <summary>
    /// The source branch name.
    /// </summary>
    public string? SourceBranchName { get; }

    /// <summary>
    /// Creates a new instance of <see cref="RepositoryConfig"/>.
    /// </summary>
    /// <param name="repositoryPath">The path to the repository</param>
    /// <param name="subdirectoryPath">The subdirectory path within the repository</param>
    /// <param name="sourceBranchName">The source branch name</param>
    public RepositoryConfig(
        string repositoryPath,
        string? subdirectoryPath,
        string? sourceBranchName
    )
    {
        if (string.IsNullOrEmpty(repositoryPath))
        {
            throw new ArgumentException("Repository path cannot be null or empty", nameof(repositoryPath));
        }

        RepositoryPath = repositoryPath;
        SubdirectoryPath = subdirectoryPath;
        SourceBranchName = sourceBranchName;
    }
}
