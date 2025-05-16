using Aviationexam.DependencyUpdater.Constants;
using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitConstants = Aviationexam.DependencyUpdater.Constants.GitConstants;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public class RepositoryAzureDevOpsClient(
    IOptionsSnapshot<DevOpsConfiguration> devOpsConfiguration,
    ILogger<RepositoryAzureDevOpsClient> logger
) : IRepositoryClient
{
    private readonly DevOpsConfiguration _config = devOpsConfiguration.Value;

    private readonly VssConnection _connection = new(
        devOpsConfiguration.Value.OrganizationEndpoint,
        new VssHttpMessageHandler(
            new VssCredentials(new VssBasicCredential(string.Empty, devOpsConfiguration.Value.PersonalAccessToken)),
            new VssClientHttpRequestSettings()
        ),
        [new LoggingHandler(logger)]
    );

    public async Task<string?> GetPullRequestForBranchAsync(
        string branchName, CancellationToken cancellationToken
    )
    {
        var gitClient = await _connection.GetClientAsync<GitHttpClient>(cancellationToken);

        var pullRequests = await gitClient.GetPullRequestsAsync(
            project: _config.Project,
            repositoryId: _config.Repository,
            searchCriteria: new GitPullRequestSearchCriteria
            {
                Status = PullRequestStatus.Active,
                SourceRefName = $"{GitConstants.HeadsPrefix}{branchName}",
            },
            cancellationToken: cancellationToken
        );

        var pullRequestId = pullRequests.FirstOrDefault()?.PullRequestId.ToString();

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
        string updater,
        CancellationToken cancellationToken
    )
    {
        var gitClient = await _connection.GetClientAsync<GitHttpClient>(cancellationToken);

        var pullRequestRequest = new GitPullRequest
        {
            Title = title,
            Description = description,
            SourceRefName = $"{GitConstants.HeadsPrefix}{branchName}",
            TargetRefName = targetBranchName is not null
                ? $"{GitConstants.HeadsPrefix}{targetBranchName}"
                : $"{GitConstants.HeadsPrefix}{PullRequestConstants.DefaultBranch}",
            Reviewers = [.. reviewers.Select(u => new IdentityRefWithVote { Id = u })],
            WorkItemRefs = milestone is not null ? [new ResourceRef { Id = milestone }] : [],
            CompletionOptions = new GitPullRequestCompletionOptions
            {
                DeleteSourceBranch = true,
                MergeStrategy = GitPullRequestMergeStrategy.Squash,
                AutoCompleteIgnoreConfigIds = [],
                TransitionWorkItems = true,
                MergeCommitMessage = description,
            },
            AutoCompleteSetBy = new IdentityRef { Id = _config.AccountId },
            Labels =
            [
                new WebApiTagDefinition { Name = PullRequestConstants.TagName },
                new WebApiTagDefinition { Name = $"{PullRequestConstants.TagName}={updater}" },
            ],
        };

        var pullRequest = await gitClient.CreatePullRequestAsync(
            gitPullRequestToCreate: pullRequestRequest,
            repositoryId: _config.Repository,
            project: _config.Project,
            cancellationToken: cancellationToken
        );

        logger.LogTrace("Created pull request {pullRequestId} for branch {BranchName}", pullRequest.PullRequestId, branchName);

        var updated = new GitPullRequest
        {
            AutoCompleteSetBy = new IdentityRef { Id = _config.AccountId },
            CompletionOptions = new GitPullRequestCompletionOptions
            {
                DeleteSourceBranch = true,
                MergeStrategy = GitPullRequestMergeStrategy.Squash,
                AutoCompleteIgnoreConfigIds = [],
                TransitionWorkItems = true,
                MergeCommitMessage = description,
            },
        };

        await gitClient.UpdatePullRequestAsync(
            gitPullRequestToUpdate: updated,
            repositoryId: _config.Repository,
            pullRequestId: pullRequest.PullRequestId,
            project: _config.Project,
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

        var gitClient = await _connection.GetClientAsync<GitHttpClient>(cancellationToken);

        var updated = new GitPullRequest
        {
            Title = title,
            Description = description,
            AutoCompleteSetBy = new IdentityRef { Id = _config.AccountId },
            CompletionOptions = new GitPullRequestCompletionOptions
            {
                DeleteSourceBranch = true,
                MergeStrategy = GitPullRequestMergeStrategy.Squash,
                AutoCompleteIgnoreConfigIds = [],
                TransitionWorkItems = true,
                MergeCommitMessage = description,
            },
        };

        await gitClient.UpdatePullRequestAsync(
            gitPullRequestToUpdate: updated,
            repositoryId: _config.Repository,
            pullRequestId: pullRequestIdAsInt,
            project: _config.Project,
            cancellationToken: cancellationToken
        );

        logger.LogTrace("Update pull request {pullRequestId}", pullRequestId);
    }
}
