using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Interfaces.Repository;
using Aviationexam.DependencyUpdater.Nuget.Configurations;
using Aviationexam.DependencyUpdater.Nuget.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class SubmoduleUpdater(
    IRepositoryClient repositoryClient,
    PackageUpdater packageUpdater,
    ILogger<SubmoduleUpdater> logger
)
{
    public async Task<IEnumerable<string>> UpdateSubmodulesAsync(
        ISourceVersioning sourceVersioning,
        RepositoryConfig repositoryConfig,
        NugetAuthConfig authConfig,
        GitCredentialsConfiguration gitCredentialsConfiguration,
        GitMetadataConfig gitMetadataConfig,
        bool executeRestore,
        string? restoreDirectory,
        string updater,
        CancellationToken cancellationToken
    )
    {
        var results = new ConcurrentBag<string>();

        // Limit parallelism to avoid overwhelming system resources
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2),
            CancellationToken = cancellationToken,
        };

        // Process dependencies in parallel
        await ProcessCollectionHelper.ForEachAsync(sourceVersioning.GetSubmodules(), parallelOptions, async (submodule, token) =>
        {
            var submoduleEntry = gitMetadataConfig.UpdateSubmodules.AsValueEnumerable().SingleOrDefault(x => x.Path == submodule);
            if (submoduleEntry is null)
            {
                return;
            }

            var branchName = BranchNameGenerator.GetBranchNameForSubmodule(submodule, repositoryConfig, updater);

            using var temporaryDirectory = new TemporaryDirectoryProvider(logger, create: false);
            using var gitWorkspace = sourceVersioning.CreateWorkspace(
                gitCredentialsConfiguration,
                temporaryDirectory.TemporaryDirectory,
                sourceBranchName: repositoryConfig.SourceBranchName,
                branchName: branchName,
                worktreeName: branchName.Replace('/', '-')
            );
            gitWorkspace.TryPullRebase(
                sourceBranchName: repositoryConfig.SourceBranchName,
                authorName: gitMetadataConfig.CommitAuthor,
                authorEmail: gitMetadataConfig.CommitAuthorEmail
            );

            var submoduleUpdate = gitWorkspace.UpdateSubmodule(submodule, submoduleEntry.Branch, repositoryConfig.SourceBranchName);

            var commitMessage = GetCommitMessage(submodule, submoduleUpdate);
            var description = GetDescription(commitMessage, submoduleUpdate);

            // Get existing pull request if it exists
            var pullRequestId = await repositoryClient.GetPullRequestForBranchAsync(
                branchName: gitWorkspace.GetBranchName(),
                token
            );

            // Process pull request based on update status
            pullRequestId = await packageUpdater.HandlePullRequestAsync(
                gitWorkspace,
                commitMessage,
                description,
                pullRequestId,
                repositoryConfig,
                authConfig,
                gitMetadataConfig,
                executeRestore,
                restoreDirectory,
                commitMessage,
                updater,
                token
            );

            if (pullRequestId is not null)
            {
                results.Add(pullRequestId);
            }
        });

        return results;
    }

    public static string GetCommitMessage(
        string submodule,
        SubmoduleUpdate? submoduleUpdate
    ) => submoduleUpdate is { FromCommit: { } fromCommit, ToCommit: var toCommit } && fromCommit != toCommit
        ? $"Bump submodule {submodule} from {fromCommit[..7]} to {toCommit[..7]}"
        : $"Bump submodule {submodule}";

    private string GetDescription(
        string commitMessage,
        SubmoduleUpdate? submoduleUpdate
    )
    {
        if (submoduleUpdate is null)
        {
            return commitMessage;
        }

        try
        {
            return ReleaseNotesFormatter.AppendToDescription(
                commitMessage,
                SubmoduleCommitsFormatter.BuildSection(submoduleUpdate),
                repositoryClient.MaxPullRequestDescriptionLength,
                SubmoduleCommitsFormatter.TruncationNotice
            );
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(ex, "Failed to describe commits of submodule {Submodule}", submoduleUpdate.Path);
            }

            return commitMessage;
        }
    }
}
