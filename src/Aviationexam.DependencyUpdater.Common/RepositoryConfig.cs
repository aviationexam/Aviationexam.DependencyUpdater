using System.IO;

namespace Aviationexam.DependencyUpdater.Common;

/// <summary>
/// Repository configuration for NuGet update operations.
/// </summary>
public sealed class RepositoryConfig
{
    /// <summary>
    /// The path to the repository.
    /// </summary>
    public required string RepositoryPath { get; init; }

    /// <summary>
    /// The subdirectory path within the repository.
    /// </summary>
    public string? SubdirectoryPath { get; init; }

    /// <summary>
    /// The source branch name.
    /// </summary>
    public string? SourceBranchName { get; init; }

    public string TargetDirectoryPath => Path.Join(RepositoryPath, SubdirectoryPath);
}
