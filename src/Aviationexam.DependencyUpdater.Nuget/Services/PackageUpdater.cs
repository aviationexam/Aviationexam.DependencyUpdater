using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Configurations;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Writers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class PackageUpdater(
    NugetVersionWriter nugetVersionWriter,
    NugetCli nugetCli,
    IRepositoryClient repositoryClient,
    ILogger<PackageUpdater> logger
)
{
    public async IAsyncEnumerable<string> ProcessPackageUpdatesAsync(
        ISourceVersioning sourceVersioning,
        RepositoryConfig repositoryConfig,
        NugetAuthConfig authConfig,
        GitCredentialsConfiguration gitCredentialsConfiguration,
        GitMetadataConfig gitMetadataConfig,
        Queue<(IReadOnlyCollection<NugetUpdateCandidate> NugetUpdateCandidates, GroupEntry GroupEntry)> groupedPackagesToUpdateQueue,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        string updater,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        // Process package groups in parallel
        var tasks = groupedPackagesToUpdateQueue.Select(groupedPackagesToUpdate => ProcessSinglePackageGroupAsync(
            repositoryConfig,
            authConfig,
            gitCredentialsConfiguration,
            gitMetadataConfig,
            groupedPackagesToUpdate.GroupEntry,
            groupedPackagesToUpdate.NugetUpdateCandidates,
            currentPackageVersions,
            sourceVersioning,
            updater,
            cancellationToken
        ));

        // Wait for all tasks to complete and yield results
        await foreach (var pullRequestIdTask in Task.WhenEach(tasks).WithCancellation(cancellationToken))
        {
            var pullRequestId = await pullRequestIdTask;
            if (pullRequestId is not null)
            {
                yield return pullRequestId;
            }
        }
    }

    private async Task<string?> ProcessSinglePackageGroupAsync(
        RepositoryConfig repositoryConfig,
        NugetAuthConfig authConfig,
        GitCredentialsConfiguration gitCredentialsConfiguration,
        GitMetadataConfig gitMetadataConfig,
        GroupEntry groupEntry,
        IReadOnlyCollection<NugetUpdateCandidate> nugetUpdateCandidates,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        ISourceVersioning sourceVersioning,
        string updater,
        CancellationToken cancellationToken
    )
    {
        using var temporaryDirectory = new TemporaryDirectoryProvider(create: false);
        using var gitWorkspace = sourceVersioning.CreateWorkspace(
            gitCredentialsConfiguration,
            temporaryDirectory.TemporaryDirectory,
            sourceBranchName: repositoryConfig.SourceBranchName,
            branchName: groupEntry.GetBranchName(updater),
            worktreeName: groupEntry.GroupName.Replace('/', '-')
        );

        gitWorkspace.TryPullRebase(
            sourceBranchName: repositoryConfig.SourceBranchName,
            authorName: gitMetadataConfig.CommitAuthor,
            authorEmail: gitMetadataConfig.CommitAuthorEmail
        );

        var updatedPackages = await UpdatePackageVersionsAsync(
            gitWorkspace,
            nugetUpdateCandidates,
            groupPackageVersions: currentPackageVersions.ToDictionary(),
            cancellationToken
        ).ToListAsync(cancellationToken);

        // Get existing pull request if it exists
        var pullRequestId = await repositoryClient.GetPullRequestForBranchAsync(
            branchName: gitWorkspace.GetBranchName(),
            cancellationToken
        );

        // Process pull request based on update status
        return await HandlePullRequestAsync(
            gitWorkspace,
            updatedPackages.GetCommitMessage(),
            pullRequestId,
            repositoryConfig,
            authConfig,
            gitMetadataConfig,
            groupEntry.GetTitle(nugetUpdateCandidates),
            updater,
            cancellationToken
        );
    }

    private async IAsyncEnumerable<NugetUpdateCandidate> UpdatePackageVersionsAsync(
        ISourceVersioningWorkspace gitWorkspace,
        IReadOnlyCollection<NugetUpdateCandidate> packagesToUpdate,
        Dictionary<string, PackageVersion> groupPackageVersions,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var packagesToUpdateQueue = new Queue<(NugetUpdateCandidate NugetUpdateCandidate, int Epoch)>(
            packagesToUpdate.Select(x => (x, 0))
        );
        var epoch = 1;

        while (packagesToUpdateQueue.TryDequeue(out var packageToUpdate))
        {
            if (packageToUpdate.Epoch == epoch)
            {
                LogVersionConflict(packageToUpdate.NugetUpdateCandidate, groupPackageVersions);
                continue;
            }

            var trySetVersion = await nugetVersionWriter.TrySetVersion(
                packageToUpdate.NugetUpdateCandidate,
                gitWorkspace,
                groupPackageVersions,
                cancellationToken
            );

            switch (trySetVersion)
            {
                case ESetVersion.VersionSet:
                    epoch++;
                    yield return packageToUpdate.NugetUpdateCandidate;
                    break;
                case ESetVersion.VersionNotSet:
                    packagesToUpdateQueue.Enqueue(packageToUpdate with { Epoch = epoch });
                    break;
                case ESetVersion.OutOfRepository:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(trySetVersion), trySetVersion, null);
            }
        }
    }

    private void LogVersionConflict(
        NugetUpdateCandidate nugetUpdateCandidate,
        Dictionary<string, PackageVersion> groupPackageVersions
    )
    {
        if (
            !nugetVersionWriter.IsCompatibleWithCurrentVersions(
                nugetUpdateCandidate.PossiblePackageVersion,
                groupPackageVersions,
                out var conflictingPackageVersion
            )
        )
        {
            logger.LogError(
                "Cannot update '{PackageName}' to version '{Version}': it depends on '{ConflictingPackageName}' version '{ConflictingPackageVersionRequired}', but the current solution uses version '{ConflictingPackageVersionCurrent}'",
                nugetUpdateCandidate.NugetDependency.NugetPackage.GetPackageName(),
                nugetUpdateCandidate.PossiblePackageVersion.PackageVersion.GetSerializedVersion(),
                conflictingPackageVersion.Name,
                conflictingPackageVersion.Version.GetSerializedVersion(),
                groupPackageVersions[conflictingPackageVersion.Name].GetSerializedVersion()
            );
        }
        else
        {
            logger.LogError(
                "Cannot set version '{Version}' for package '{PackageName}' due to conflicting version constraints from other packages",
                nugetUpdateCandidate.PossiblePackageVersion.PackageVersion.GetSerializedVersion(),
                nugetUpdateCandidate.NugetDependency.NugetPackage.GetPackageName()
            );
        }
    }

    public async Task<string?> HandlePullRequestAsync(
        ISourceVersioningWorkspace gitWorkspace,
        string? commitMessage,
        string? pullRequestId,
        RepositoryConfig repositoryConfig,
        NugetAuthConfig authConfig,
        GitMetadataConfig gitMetadataConfig,
        string title,
        string updater,
        CancellationToken cancellationToken
    )
    {
        if (
            commitMessage is not null
            && pullRequestId is not null
        )
        {
            // Just update PR title and description if already exists
            await repositoryClient.UpdatePullRequestAsync(
                pullRequestId: pullRequestId,
                title: title,
                description: commitMessage,
                cancellationToken
            );
        }

        if (
            commitMessage is not null
            && gitWorkspace.HasUncommitedChanges()
        )
        {
            // Commit changes and create/update PR
            gitWorkspace.CommitChanges(
                message: commitMessage,
                authorName: gitMetadataConfig.CommitAuthor,
                authorEmail: gitMetadataConfig.CommitAuthorEmail
            );
        }

        await RestoreNugetPackagesAsync(
            gitWorkspace,
            repositoryConfig.SubdirectoryPath,
            authConfig,
            gitMetadataConfig,
            cancellationToken
        );

        var pushed = gitWorkspace.Push(sourceBranchName: repositoryConfig.SourceBranchName);

        if (
            pushed
            && commitMessage is not null
        )
        {
            return await CreateOrUpdatePullRequestAsync(
                gitWorkspace,
                pullRequestId,
                repositoryConfig,
                gitMetadataConfig,
                title,
                commitMessage,
                updater,
                cancellationToken
            );
        }

        return null;
    }

    private async Task RestoreNugetPackagesAsync(
        ISourceVersioningWorkspace gitWorkspace,
        string? subdirectoryPath,
        NugetAuthConfig authConfig,
        GitMetadataConfig gitMetadataConfig,
        CancellationToken cancellationToken
    )
    {
        var workingDirectory = gitWorkspace.GetWorkspaceDirectory();
        if (subdirectoryPath is not null)
        {
            workingDirectory = Path.Join(workingDirectory, subdirectoryPath);
        }

        var restored = await nugetCli.Restore(
            workingDirectory,
            authConfig,
            cancellationToken
        );

        if (restored && gitWorkspace.HasUncommitedChanges())
        {
            gitWorkspace.CommitChanges(
                message: "Update package.lock.json",
                authorName: gitMetadataConfig.CommitAuthor,
                authorEmail: gitMetadataConfig.CommitAuthorEmail
            );
        }
    }

    private async Task<string?> CreateOrUpdatePullRequestAsync(
        ISourceVersioningWorkspace gitWorkspace,
        string? pullRequestId,
        RepositoryConfig repositoryConfig,
        GitMetadataConfig gitMetadataConfig,
        string title,
        string commitMessage,
        string updater,
        CancellationToken cancellationToken
    )
    {
        if (pullRequestId is not null)
        {
            await repositoryClient.UpdatePullRequestAsync(
                pullRequestId: pullRequestId,
                title: title,
                description: commitMessage,
                cancellationToken
            );
            return pullRequestId;
        }

        return await repositoryClient.CreatePullRequestAsync(
            branchName: gitWorkspace.GetBranchName(),
            targetBranchName: repositoryConfig.SourceBranchName,
            title: title,
            description: commitMessage,
            gitMetadataConfig.Milestone,
            gitMetadataConfig.Reviewers,
            updater: updater,
            cancellationToken
        );
    }
}
