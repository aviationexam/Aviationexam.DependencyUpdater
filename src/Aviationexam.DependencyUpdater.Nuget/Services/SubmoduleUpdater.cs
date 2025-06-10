using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Constants;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Configurations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class SubmoduleUpdater(
    IRepositoryClient repositoryClient,
    PackageUpdater packageUpdater
)
{
    public async Task<IEnumerable<string>> UpdateSubmodulesAsync(
        ISourceVersioning sourceVersioning,
        RepositoryConfig repositoryConfig,
        NugetAuthConfig authConfig,
        GitCredentialsConfiguration gitCredentialsConfiguration,
        GitMetadataConfig gitMetadataConfig,
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
        await Parallel.ForEachAsync(sourceVersioning.GetSubmodules(), parallelOptions, async (submodule, token) =>
        {
            var submoduleEntry = gitMetadataConfig.UpdateSubmodules.SingleOrDefault(x => x.Path == submodule);
            if (submoduleEntry is null)
            {
                return;
            }

            var branchName = $"{GitConstants.UpdaterBranchPrefix}{updater}/submodule/{submodule}";
            using var temporaryDirectory = new TemporaryDirectoryProvider(create: false);
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
