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

            gitWorkspace.UpdateSubmodule(submodule, submoduleEntry.Branch);

            // Get existing pull request if it exists
            var pullRequestId = await repositoryClient.GetPullRequestForBranchAsync(
                branchName: gitWorkspace.GetBranchName(),
                token
            );

            // Process pull request based on update status
            pullRequestId = await packageUpdater.HandlePullRequestAsync(
                gitWorkspace,
                $"Bump submodule {submodule}",
                pullRequestId,
                repositoryConfig,
                authConfig,
                gitMetadataConfig,
                executeRestore,
                restoreDirectory,
                $"Bump submodule {submodule}",
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
}
