using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Interfaces;

public interface IRepositoryClient
{
    Task<string?> GetPullRequestForBranchAsync(
        string branchName,
        CancellationToken cancellationToken
    );

    Task CreatePullRequestAsync(
        string branchName,
        string title,
        string description,
        long milestone,
        IReadOnlyCollection<string> reviewers,
        CancellationToken cancellationToken
    );

    Task UpdatePullRequestAsync(
        string pullRequestId,
        string title,
        string description,
        long milestone,
        IReadOnlyCollection<string> reviewers,
        CancellationToken cancellationToken
    );
}
