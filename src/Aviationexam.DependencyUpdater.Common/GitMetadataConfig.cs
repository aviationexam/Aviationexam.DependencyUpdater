using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Common;

/// <summary>
/// Git metadata configuration for NuGet update operations.
/// </summary>
public sealed class GitMetadataConfig
{
    /// <summary>
    /// The milestone to associate with the pull request.
    /// </summary>
    public string? Milestone { get; init; }

    /// <summary>
    /// The reviewers to assign to the pull request.
    /// </summary>
    public required IReadOnlyCollection<string> Reviewers { get; init; }

    /// <summary>
    /// The commit author name.
    /// </summary>
    public required string CommitAuthor { get; init; }

    /// <summary>
    /// The commit author email.
    /// </summary>
    public required string CommitAuthorEmail { get; init; }

    /// <summary>
    /// Whether to update submodules.
    /// </summary>
    public required bool UpdateSubmodules { get; init; }
}
