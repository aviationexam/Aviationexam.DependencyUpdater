using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Constants;
using Aviationexam.DependencyUpdater.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;
using Polly;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;
using PullRequest = Aviationexam.DependencyUpdater.Interfaces.PullRequest;

namespace Aviationexam.DependencyUpdater.Repository.GitHub;

public class RepositoryGitHubClient(
    GitHubConfiguration gitHubConfiguration,
    IGitHubClient gitHubClient,
    [FromKeyedServices($"{nameof(IGitHubClient.PullRequest.Create)}-pipeline")]
    ResiliencePipeline<Octokit.PullRequest> createPullRequestAsyncResiliencePipeline,
    ILogger<RepositoryGitHubClient> logger
) : IRepositoryClient
{
    private const string MainLabelColor = "0E8A16"; // Green
    private const string MetadataLabelColor = "FBCA04"; // Yellow

    private ISet<string>? _labelCache;

    public async Task<IEnumerable<PullRequest>> ListActivePullRequestsAsync(
        string sourceDirectory,
        string updater,
        CancellationToken cancellationToken
    )
    {
        var pullRequestRequest = new PullRequestRequest
        {
            State = ItemStateFilter.Open,
        };

        var pullRequests = await gitHubClient.PullRequest.GetAllForRepository(
            gitHubConfiguration.Owner,
            gitHubConfiguration.Repository,
            pullRequestRequest
        );

        var branchPrefix = BranchNameGenerator.GetBranchNamePrefix(sourceDirectory, updater);
        var requiredLabels = new[]
        {
            PullRequestConstants.TagName,
            $"{PullRequestConstants.TagName}={updater}",
            $"{PullRequestConstants.SourceTagName}={sourceDirectory}",
        };

        return pullRequests
            .AsValueEnumerable()
            .Where(pr =>
                pr.Head.Ref.StartsWith(branchPrefix)
                && requiredLabels.AsValueEnumerable().All(requiredLabel =>
                    pr.Labels.AsValueEnumerable().Any(label => label.Name == requiredLabel)
                )
            )
            .Select(pr => new PullRequest(
                pr.Number.ToString(),
                pr.Head.Ref,
                pr.Head.Sha,
                pr.Head.Sha == pr.Base.Sha
            ))
            .ToList();
    }

    public async Task<string?> GetPullRequestForBranchAsync(
        string branchName,
        CancellationToken cancellationToken
    )
    {
        var pullRequestRequest = new PullRequestRequest
        {
            State = ItemStateFilter.Open,
            Head = $"{gitHubConfiguration.Owner}:{branchName}",
        };

        var pullRequests = await gitHubClient.PullRequest.GetAllForRepository(
            gitHubConfiguration.Owner,
            gitHubConfiguration.Repository,
            pullRequestRequest
        );

        var pullRequestNumber = pullRequests.AsValueEnumerable().FirstOrDefault()?.Number.ToString();

        if (pullRequestNumber is not null)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Found existing pull request: {pullRequestId} for branch {BranchName}", pullRequestNumber, branchName);
            }
        }
        else
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("No pull request found for branch {BranchName}", branchName);
            }
        }

        return pullRequestNumber;
    }

    public async Task<string> CreatePullRequestAsync(
        string branchName,
        string? targetBranchName,
        string title,
        string description,
        string? milestone,
        IReadOnlyCollection<string> reviewers,
        string sourceDirectory,
        string updater,
        CancellationToken cancellationToken
    )
    {
        // Ensure required labels exist
        var labelsToApply = new[]
        {
            PullRequestConstants.TagName,
            $"{PullRequestConstants.TagName}={updater}",
            $"{PullRequestConstants.SourceTagName}={sourceDirectory}"
        };

        await EnsureLabelsExistAsync(labelsToApply, cancellationToken);

        // Create pull request
        var newPullRequest = new NewPullRequest(title, branchName, targetBranchName ?? PullRequestConstants.DefaultBranch)
        {
            Body = description,
            MaintainerCanModify = true,
        };

        var pullRequest = await createPullRequestAsyncResiliencePipeline.ExecuteAsync(
            static async (_, state) => await state.gitHubClient.PullRequest.Create(
                state.gitHubConfiguration.Owner,
                state.gitHubConfiguration.Repository,
                state.newPullRequest
            ),
            ResilienceContextPool.Shared.Get(branchName.Replace('/', '-'), cancellationToken),
            new { gitHubClient, gitHubConfiguration, newPullRequest }
        );

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Created pull request {pullRequestId} for branch {BranchName}", pullRequest.Number, branchName);
        }

        // Apply labels
        await gitHubClient.Issue.Labels.AddToIssue(
            gitHubConfiguration.Owner,
            gitHubConfiguration.Repository,
            pullRequest.Number,
            labelsToApply
        );

        // Add reviewers
        if (reviewers.Count > 0)
        {
            var assigneesRequest = new AssigneesUpdate(
                [.. reviewers]
            );

            try
            {
                await gitHubClient.Issue.Assignee.AddAssignees(
                    gitHubConfiguration.Owner,
                    gitHubConfiguration.Repository,
                    pullRequest.Number,
                    assigneesRequest
                );
            }
            catch (ApiException ex)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(ex, "Failed to add assignees to pull request {pullRequestId}", pullRequest.Number);
                }
            }
        }

        // Handle milestone
        if (milestone is not null)
        {
            try
            {
                var issueUpdate = new IssueUpdate();

                // Try to parse as milestone ID
                if (int.TryParse(milestone, out var milestoneId))
                {
                    issueUpdate.Milestone = milestoneId;
                }
                else
                {
                    // Search by milestone title
                    var milestones = await gitHubClient.Issue.Milestone.GetAllForRepository(
                        gitHubConfiguration.Owner,
                        gitHubConfiguration.Repository
                    );

                    var matchingMilestone = milestones.AsValueEnumerable().FirstOrDefault(m => m.Title == milestone);
                    if (matchingMilestone is not null)
                    {
                        issueUpdate.Milestone = matchingMilestone.Number;
                    }
                    else
                    {
                        if (logger.IsEnabled(LogLevel.Warning))
                        {
                            logger.LogWarning("Milestone '{Milestone}' not found for pull request {pullRequestId}", milestone, pullRequest.Number);
                        }
                    }
                }

                if (issueUpdate.Milestone.HasValue)
                {
                    await gitHubClient.Issue.Update(
                        gitHubConfiguration.Owner,
                        gitHubConfiguration.Repository,
                        pullRequest.Number,
                        issueUpdate
                    );
                }
            }
            catch (ApiException ex)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(ex, "Failed to set milestone for pull request {pullRequestId}", pullRequest.Number);
                }
            }
        }

        return pullRequest.Number.ToString();
    }

    public async Task UpdatePullRequestAsync(
        string pullRequestId,
        string title,
        string description,
        CancellationToken cancellationToken
    )
    {
        var pullRequestNumber = int.Parse(pullRequestId);

        var pullRequestUpdate = new PullRequestUpdate
        {
            Title = title,
            Body = description,
        };

        await gitHubClient.PullRequest.Update(
            gitHubConfiguration.Owner,
            gitHubConfiguration.Repository,
            pullRequestNumber,
            pullRequestUpdate
        );

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Updated pull request {pullRequestId}", pullRequestId);
        }
    }

    public async Task AbandonPullRequestAsync(
        PullRequest pullRequest,
        CancellationToken cancellationToken
    )
    {
        var pullRequestNumber = int.Parse(pullRequest.PullRequestId);

        // Close PR
        var pullRequestUpdate = new PullRequestUpdate
        {
            State = ItemState.Closed,
        };

        await gitHubClient.PullRequest.Update(
            gitHubConfiguration.Owner,
            gitHubConfiguration.Repository,
            pullRequestNumber,
            pullRequestUpdate
        );

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Closed pull request {PullRequestId}", pullRequest.PullRequestId);
        }

        // Delete branch
        try
        {
            await gitHubClient.Git.Reference.Delete(
                gitHubConfiguration.Owner,
                gitHubConfiguration.Repository,
                $"heads/{pullRequest.BranchName}"
            );

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Deleted remote branch {Branch}", pullRequest.BranchName);
            }
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Cannot delete branch {Branch} - it may be protected or have outstanding reviews", pullRequest.BranchName);
            }
        }
        catch (ApiException ex)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(ex, "Failed to delete remote branch {Branch}", pullRequest.BranchName);
            }
        }
    }

    private async Task EnsureLabelsExistAsync(
        string[] labels,
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        CancellationToken cancellationToken
    )
    {
        // Initialize cache if needed
        if (_labelCache is null)
        {
            var existingLabels = await gitHubClient.Issue.Labels.GetAllForRepository(
                gitHubConfiguration.Owner,
                gitHubConfiguration.Repository
            );

            _labelCache = existingLabels.AsValueEnumerable().Select(l => l.Name).ToHashSet();
        }

        // Create missing labels
        foreach (var labelName in labels)
        {
            if (!_labelCache.Contains(labelName))
            {
                try
                {
                    var color = labelName == PullRequestConstants.TagName ? MainLabelColor : MetadataLabelColor;
                    var newLabel = new NewLabel(labelName, color)
                    {
                        Description = $"Managed by {PullRequestConstants.TagName}",
                    };

                    await gitHubClient.Issue.Labels.Create(
                        gitHubConfiguration.Owner,
                        gitHubConfiguration.Repository,
                        newLabel
                    );

                    _labelCache.Add(labelName);
                    if (logger.IsEnabled(LogLevel.Trace))
                    {
                        logger.LogTrace("Created label: {LabelName}", labelName);
                    }
                }
                catch (ApiException ex)
                {
                    if (logger.IsEnabled(LogLevel.Warning))
                    {
                        logger.LogWarning(ex, "Failed to create label {LabelName}", labelName);
                    }
                }
            }
        }
    }
}
