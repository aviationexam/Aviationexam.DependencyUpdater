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

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public class RepositoryAzureDevOpsClient(
    IOptionsSnapshot<DevOpsConfiguration> devOpsConfiguration,
    ILogger<RepositoryAzureDevOpsClient> logger
) : IRepositoryClient
{
    private readonly DevOpsConfiguration _config = devOpsConfiguration.Value;

    private readonly VssConnection _connection = new(
        devOpsConfiguration.Value.OrganizationEndpoint,
        new VssBasicCredential(string.Empty, devOpsConfiguration.Value.PersonalAccessToken)
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
                SourceRefName = $"refs/heads/{branchName}",
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

    public async Task CreatePullRequestAsync(
        string branchName,
        string? targetBranchName,
        string title,
        string description,
        string? milestone,
        IReadOnlyCollection<string> reviewers,
        CancellationToken cancellationToken
    )
    {
        var gitClient = await _connection.GetClientAsync<GitHttpClient>(cancellationToken);

        var pullRequestRequest = new GitPullRequest
        {
            Title = title,
            Description = description,
            SourceRefName = $"refs/heads/{branchName}",
            TargetRefName = targetBranchName is not null
                ? $"refs/heads/{targetBranchName}"
                : "refs/heads/main",
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
            AutoCompleteSetBy = new IdentityRef { DisplayName = "DependencyUpdater" },
            Labels =
            [
                new WebApiTagDefinition { Name = "dependency-updater" },
            ],
        };

        var pullRequest = await gitClient.CreatePullRequestAsync(
            gitPullRequestToCreate: pullRequestRequest,
            repositoryId: _config.Repository,
            project: _config.Project,
            cancellationToken: cancellationToken
        );

        logger.LogTrace("Created pull request {pullRequestId} for branch {BranchName}", pullRequest.PullRequestId, branchName);
    }

    public Task UpdatePullRequestAsync(
        string pullRequestId,
        string title,
        string description,
        string? milestone,
        IReadOnlyCollection<string> reviewers,
        CancellationToken cancellationToken
    )
    {
        throw new System.NotImplementedException();
    }
}
