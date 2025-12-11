using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Repository.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class PullRequestManager(
    IRepositoryClient repositoryClient,
    ILogger<PullRequestManager> logger
)
{
    public async Task CleanupAbandonedPullRequestsAsync(
        string sourceDirectory,
        string updater,
        IReadOnlyCollection<string> knownPullRequests,
        CancellationToken cancellationToken
    )
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Known pull requests {PullRequestsId}", string.Join(", ", knownPullRequests));
        }

        foreach (var pullRequest in await repositoryClient.ListActivePullRequestsAsync(sourceDirectory, updater, cancellationToken))
        {
            if (
                pullRequest.IsEmptyBranch
                || !knownPullRequests.AsValueEnumerable().Contains(pullRequest.PullRequestId)
            )
            {
                logger.LogDebug("Abandoning pull request {PullRequestId}", pullRequest.PullRequestId);

                await repositoryClient.AbandonPullRequestAsync(pullRequest, cancellationToken);
            }
        }
    }
}
