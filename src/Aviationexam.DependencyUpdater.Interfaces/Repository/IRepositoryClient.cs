using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Interfaces.Repository;

public interface IRepositoryClient
{
    Task<IEnumerable<PullRequest>> ListActivePullRequestsAsync(
        string sourceDirectory,
        string updater,
        CancellationToken cancellationToken
    );

    Task<string?> GetPullRequestForBranchAsync(
        string branchName,
        CancellationToken cancellationToken
    );

    Task<string> CreatePullRequestAsync(
        string branchName,
        string? targetBranchName,
        string title,
        string description,
        string? milestone,
        IReadOnlyCollection<string> reviewers,
        IReadOnlyCollection<string> labels,
        string sourceDirectory,
        string updater,
        CancellationToken cancellationToken
    );

    Task UpdatePullRequestAsync(
        string pullRequestId,
        string title,
        string description,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Approves any workflow runs that are pending maintainer approval for the given commit.
    /// This is typically required for workflow runs triggered by first-time contributors or
    /// when approval is otherwise required by repository policy.
    /// Platforms that do not implement this concept should treat this as a no-op.
    /// </summary>
    /// <param name="headSha">The commit SHA whose pending workflow runs should be approved.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ApprovePendingWorkflowRunsAsync(
        string headSha,
        CancellationToken cancellationToken
    );

    Task AbandonPullRequestAsync(
        PullRequest pullRequest,
        CancellationToken cancellationToken
    );

    Task ClosePullRequestAsync(
        string pullRequestId,
        CancellationToken cancellationToken
    );

    Task ReopenPullRequestAsync(
        string pullRequestId,
        CancellationToken cancellationToken
    );
}
