using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Interfaces;

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

    Task AbandonPullRequestAsync(
        PullRequest pullRequest,
        CancellationToken cancellationToken
    );

    Task EnsurePackageVersionIsAvailableAsync(
        string getPackageName,
        string getSerializedVersion,
        CancellationToken cancellationToken
    );
}
