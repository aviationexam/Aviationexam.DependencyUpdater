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
    public string? Milestone { get; }

    /// <summary>
    /// The reviewers to assign to the pull request.
    /// </summary>
    public IReadOnlyCollection<string> Reviewers { get; }

    /// <summary>
    /// The commit author name.
    /// </summary>
    public string CommitAuthor { get; }

    /// <summary>
    /// The commit author email.
    /// </summary>
    public string CommitAuthorEmail { get; }

    /// <summary>
    /// Whether to update submodules.
    /// </summary>
    public bool UpdateSubmodules { get; }

    /// <summary>
    /// Creates a new instance of <see cref="GitMetadataConfig"/>.
    /// </summary>
    /// <param name="milestone">The milestone to associate with the pull request</param>
    /// <param name="reviewers">The reviewers to assign to the pull request</param>
    /// <param name="commitAuthor">The commit author name</param>
    /// <param name="commitAuthorEmail">The commit author email</param>
    /// <param name="updateSubmodules">Whether to update submodules</param>
    public GitMetadataConfig(
        string? milestone,
        IReadOnlyCollection<string> reviewers,
        string commitAuthor,
        string commitAuthorEmail,
        bool updateSubmodules = false
    )
    {
        if (string.IsNullOrEmpty(commitAuthor))
        {
            throw new ArgumentException("Commit author cannot be null or empty", nameof(commitAuthor));
        }

        if (string.IsNullOrEmpty(commitAuthorEmail))
        {
            throw new ArgumentException("Commit author email cannot be null or empty", nameof(commitAuthorEmail));
        }

        Milestone = milestone;
        Reviewers = reviewers ?? Array.Empty<string>();
        CommitAuthor = commitAuthor;
        CommitAuthorEmail = commitAuthorEmail;
        UpdateSubmodules = updateSubmodules;
    }
}
