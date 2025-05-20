using Aviationexam.DependencyUpdater.Constants;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GitConstants = Aviationexam.DependencyUpdater.Constants.GitConstants;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public class RepositoryAzureDevOpsClient(
    IOptionsSnapshot<DevOpsConfiguration> devOpsConfiguration,
    VssConnection connection,
    HttpClient httpClient,
    ILogger<RepositoryAzureDevOpsClient> logger
) : IRepositoryClient
{
    private readonly DevOpsConfiguration _config = devOpsConfiguration.Value;

    public async Task<IEnumerable<PullRequest>> ListActivePullRequestsAsync(
        string updater,
        CancellationToken cancellationToken
    )
    {
        var gitClient = await connection.GetClientAsync<GitHttpClient>(cancellationToken);

        var pullRequests = await gitClient.GetPullRequestsAsync(
            project: _config.Project,
            repositoryId: _config.Repository,
            searchCriteria: new GitPullRequestSearchCriteria
            {
                Status = PullRequestStatus.Active,
            },
            cancellationToken: cancellationToken
        );

        return pullRequests
            .Where(pr =>
                pr.SourceRefName?.StartsWith($"{GitConstants.HeadsPrefix}{GitConstants.UpdaterBranchPrefix}{updater}/") == true
                && pr.Labels?.Any(l => l.Name == PullRequestConstants.TagName) == true
                && pr.Labels?.Any(l => l.Name == $"{PullRequestConstants.TagName}={updater}") == true
            )
            .Select(x => new PullRequest(
                x.PullRequestId.ToString(),
                x.SourceRefName[GitConstants.HeadsPrefix.Length..],
                x.LastMergeSourceCommit.CommitId
            ));
    }

    public async Task<string?> GetPullRequestForBranchAsync(
        string branchName, CancellationToken cancellationToken
    )
    {
        var gitClient = await connection.GetClientAsync<GitHttpClient>(cancellationToken);

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
        var gitClient = await connection.GetClientAsync<GitHttpClient>(cancellationToken);

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
                MergeCommitMessage = $"{title}\n\n{description}",
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
                MergeCommitMessage = $"{title}\n\n{description}",
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

        var gitClient = await connection.GetClientAsync<GitHttpClient>(cancellationToken);

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
                MergeCommitMessage = $"{title}\n\n{description}",
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
            _config.Repository,
            pullRequestIdInt,
            _config.Project,
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
            repositoryId: _config.Repository,
            projectId: _config.Project,
            cancellationToken: cancellationToken
        );

        logger.LogTrace("Deleted remote branch {Branch}", pullRequest.BranchName);
    }

    public async Task EnsurePackageVersionIsAvailableAsync(
        string packageName,
        string packageVersion,
        CancellationToken cancellationToken
    )
    {
        var request = new HierarchyQueryRequest
        {
            ContributionIds = ["ms.azure-artifacts.upstream-versions-data-provider"],
            DataProviderContext = new DataProviderContext
            {
                Properties = new Properties
                {
                    ProjectId = _config.Project,
                    FeedId = _config.NugetFeedId,
                    Protocol = "NuGet",
                    PackageName = packageName,
                    SourcePage = new SourcePage
                    {
                        Url = new Uri(_config.OrganizationEndpoint, $"/{_config.Organization}/{_config.Project}/_artifacts/feed/nuget-feed/NuGet/{packageName}/upstreams").ToString(),
                        RouteId = "ms.azure-artifacts.artifacts-route",
                        RouteValues = new RouteValues
                        {
                            Project = packageName,
                            Wildcard = $"feed/nuget-feed/NuGet/{packageName}/upstreams",
                            Controller = "ContributedPage",
                            Action = "Execute",
                            ServiceHost = _config.NugetServiceHost,
                        },
                    },
                },
            },
        };
        var jsonBody = JsonSerializer.Serialize(request, AzureArtifactsJsonContext.Default.HierarchyQueryRequest);

        var requestUri = new Uri(_config.OrganizationEndpoint, $"/{_config.Organization}/_apis/Contribution/HierarchyQuery/project/{_config.Project}");

        using var hierarchyRequestContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using var hierarchyRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
        hierarchyRequest.Content = hierarchyRequestContent;
        hierarchyRequest.Headers.Add("Accept", "application/json;api-version=5.0-preview.1;excludeUrls=true;enumsAsNumbers=true;msDateFormat=true;noArrayWrap=true");

        using var hierarchyResponse = await httpClient.SendAsync(
            hierarchyRequest,
            cancellationToken
        );

        var hierarchyResponseContent = await hierarchyResponse.Content.ReadAsStringAsync(cancellationToken);

        var b = hierarchyResponseContent;
    }
}
