using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Constants;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Polly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;
using GitConstants = Aviationexam.DependencyUpdater.Constants.GitConstants;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public class RepositoryAzureDevOpsClient(
    AzureDevOpsConfiguration devOpsConfiguration,
    VssConnection connection,
    [FromKeyedServices($"{nameof(GitHttpClient.CreatePullRequestAsync)}-pipeline")]
    ResiliencePipeline<GitPullRequest> createPullRequestAsyncResiliencePipeline,
    ILogger<RepositoryAzureDevOpsClient> logger
) : IRepositoryClient
{
    public async Task<IEnumerable<PullRequest>> ListActivePullRequestsAsync(
        string sourceDirectory,
        string updater,
        CancellationToken cancellationToken
    )
    {
        var gitClient = await connection.GetClientAsync<GitHttpClient>(cancellationToken);

        var pullRequests = await gitClient.GetPullRequestsAsync(
            project: devOpsConfiguration.Project,
            repositoryId: devOpsConfiguration.Repository,
            searchCriteria: new GitPullRequestSearchCriteria
            {
                Status = PullRequestStatus.Active,
            },
            cancellationToken: cancellationToken
        );

        return pullRequests
            .AsValueEnumerable()
            .Where(pr =>
                pr.SourceRefName?.StartsWith($"{GitConstants.HeadsPrefix}{BranchNameGenerator.GetBranchNamePrefix(sourceDirectory, updater)}") == true
                && pr.Labels != null
                && pr.Labels.AsValueEnumerable() is var prLabels
                && prLabels.Any(l => l.Name == PullRequestConstants.TagName)
                && prLabels.Any(l => l.Name == $"{PullRequestConstants.TagName}={updater}")
                && prLabels.Any(l => l.Name == $"{PullRequestConstants.SourceTagName}={sourceDirectory}")
            )
            .Select(x => new PullRequest(
                x.PullRequestId.ToString(),
                x.SourceRefName[GitConstants.HeadsPrefix.Length..],
                x.LastMergeSourceCommit.CommitId,
                x.LastMergeSourceCommit.CommitId == x.LastMergeTargetCommit.CommitId
            ))
            .ToList();
    }

    public async Task<string?> GetPullRequestForBranchAsync(
        string branchName, CancellationToken cancellationToken
    )
    {
        var gitClient = await connection.GetClientAsync<GitHttpClient>(cancellationToken);

        var pullRequests = await gitClient.GetPullRequestsAsync(
            project: devOpsConfiguration.Project,
            repositoryId: devOpsConfiguration.Repository,
            searchCriteria: new GitPullRequestSearchCriteria
            {
                Status = PullRequestStatus.Active,
                SourceRefName = $"{GitConstants.HeadsPrefix}{branchName}",
            },
            cancellationToken: cancellationToken
        );

        var pullRequestId = pullRequests.AsValueEnumerable().FirstOrDefault()?.PullRequestId.ToString();

        if (pullRequestId is not null)
        {
            logger.LogTrace("Found existing pull request: {pullRequestId} for branch {BranchName}", pullRequestId, branchName);
        }
        else
        {
            logger.LogTrace("No pull request found for branch {BranchName}", branchName);
        }

        return pullRequestId;
    }

    public async Task<string> CreatePullRequestAsync(
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
    )
    {
        var gitClient = await connection.GetClientAsync<GitHttpClient>(cancellationToken);

        var pullRequestRequest = new GitPullRequest
        {
            Title = title,
            Description = description,
            SourceRefName = $"{GitConstants.HeadsPrefix}{branchName}",
            TargetRefName = targetBranchName is not null
                ? $"{GitConstants.HeadsPrefix}{targetBranchName}"
                : $"{GitConstants.HeadsPrefix}{PullRequestConstants.DefaultBranch}",
            Reviewers = [.. reviewers.AsValueEnumerable().Select(u => new IdentityRefWithVote { Id = u })],
            WorkItemRefs = milestone is not null ? [new ResourceRef { Id = milestone }] : [],
            CompletionOptions = new GitPullRequestCompletionOptions
            {
                DeleteSourceBranch = true,
                MergeStrategy = GitPullRequestMergeStrategy.Squash,
                AutoCompleteIgnoreConfigIds = [],
                TransitionWorkItems = true,
                MergeCommitMessage = $"{title}\n\n{description}",
            },
            AutoCompleteSetBy = new IdentityRef { Id = devOpsConfiguration.AccountId },
            Labels =
            [
                new WebApiTagDefinition { Name = PullRequestConstants.TagName },
                new WebApiTagDefinition { Name = $"{PullRequestConstants.TagName}={updater}" },
                new WebApiTagDefinition { Name = $"{PullRequestConstants.SourceTagName}={sourceDirectory}" },
                .. labels.AsValueEnumerable().Select(label => new WebApiTagDefinition { Name = label }),
            ],
        };

        var pullRequest = await createPullRequestAsyncResiliencePipeline.ExecuteAsync(static async (context, state) => await state.gitClient.CreatePullRequestAsync(
            gitPullRequestToCreate: state.pullRequestRequest,
            repositoryId: state.devOpsConfiguration.Repository,
            project: state.devOpsConfiguration.Project,
            cancellationToken: context.CancellationToken
        ), ResilienceContextPool.Shared.Get(branchName.Replace('/', '-'), cancellationToken), new { gitClient, devOpsConfiguration, pullRequestRequest });

        logger.LogTrace("Created pull request {pullRequestId} for branch {BranchName}", pullRequest.PullRequestId, branchName);

        var updated = new GitPullRequest
        {
            AutoCompleteSetBy = new IdentityRef { Id = devOpsConfiguration.AccountId },
            CompletionOptions = new GitPullRequestCompletionOptions
            {
                DeleteSourceBranch = true,
                MergeStrategy = GitPullRequestMergeStrategy.Squash,
                AutoCompleteIgnoreConfigIds = [],
                TransitionWorkItems = true,
                MergeCommitMessage = $"{title}\n\n{description}",
            },
        };

        await gitClient.UpdatePullRequestAsync(
            gitPullRequestToUpdate: updated,
            repositoryId: devOpsConfiguration.Repository,
            pullRequestId: pullRequest.PullRequestId,
            project: devOpsConfiguration.Project,
            cancellationToken: cancellationToken
        );

        return pullRequest.PullRequestId.ToString();
    }

    public async Task UpdatePullRequestAsync(
        string pullRequestId,
        string title,
        string description,
        CancellationToken cancellationToken
    )
    {
        var pullRequestIdAsInt = int.Parse(pullRequestId);

        var gitClient = await connection.GetClientAsync<GitHttpClient>(cancellationToken);

        var updated = new GitPullRequest
        {
            Title = title,
            Description = description,
            AutoCompleteSetBy = new IdentityRef { Id = devOpsConfiguration.AccountId },
            CompletionOptions = new GitPullRequestCompletionOptions
            {
                DeleteSourceBranch = true,
                MergeStrategy = GitPullRequestMergeStrategy.Squash,
                AutoCompleteIgnoreConfigIds = [],
                TransitionWorkItems = true,
                MergeCommitMessage = $"{title}\n\n{description}",
            },
        };

        await gitClient.UpdatePullRequestAsync(
            gitPullRequestToUpdate: updated,
            repositoryId: devOpsConfiguration.Repository,
            pullRequestId: pullRequestIdAsInt,
            project: devOpsConfiguration.Project,
            cancellationToken: cancellationToken
        );

        logger.LogTrace("Update pull request {pullRequestId}", pullRequestId);
    }

    public async Task AbandonPullRequestAsync(
        PullRequest pullRequest,
        CancellationToken cancellationToken
    )
    {
        var gitClient = await connection.GetClientAsync<GitHttpClient>(cancellationToken);

        var pullRequestIdInt = int.Parse(pullRequest.PullRequestId);

        // Abandon PR
        await gitClient.UpdatePullRequestAsync(
            new GitPullRequest { Status = PullRequestStatus.Abandoned },
            devOpsConfiguration.Repository,
            pullRequestIdInt,
            devOpsConfiguration.Project,
            cancellationToken
        );

        logger.LogTrace("Abandoned pull request {PullRequestId}", pullRequest.PullRequestId);

        await gitClient.UpdateRefsAsync(
            [
                new GitRefUpdate
                {
                    Name = $"{GitConstants.HeadsPrefix}{pullRequest.BranchName}",
                    OldObjectId = pullRequest.BranchTipCommitId,
                    NewObjectId = new string('0', 40),
                },
            ],
            repositoryId: devOpsConfiguration.Repository,
            projectId: devOpsConfiguration.Project,
            cancellationToken: cancellationToken
        );

        logger.LogTrace("Deleted remote branch {Branch}", pullRequest.BranchName);
    }
}
