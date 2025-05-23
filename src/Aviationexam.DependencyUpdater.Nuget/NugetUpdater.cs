using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class NugetUpdater(
    NugetFinder nugetFinder,
    NugetConfigParser nugetConfigParser,
    NugetDirectoryPackagesPropsParser nugetDirectoryPackagesPropsParser,
    NugetCsprojParser nugetCsprojParser,
    NugetVersionFetcherFactory nugetVersionFetcherFactory,
    NugetVersionFetcher nugetVersionFetcher,
    FutureVersionResolver futureVersionResolver,
    IgnoreResolverFactory ignoreResolverFactory,
    GroupResolverFactory groupResolverFactory,
    TargetFrameworksResolver targetFrameworksResolver,
    IgnoredDependenciesResolver ignoredDependenciesResolver,
    ISourceVersioningFactory sourceVersioningFactory,
    NugetVersionWriter nugetVersionWriter,
    NugetCli nugetCli,
    IRepositoryClient repositoryClient,
    ILogger<NugetUpdater> logger
)
{
    public async Task ProcessUpdatesAsync(
        RepositoryConfig repositoryConfig,
        GitMetadataConfig gitMetadataConfig,
        NugetPackageConfig packageConfig,
        NugetAuthConfig authConfig,
        CancellationToken cancellationToken
    )
    {
        const string updater = "nuget";
        var nugetUpdaterContext = CreateContext(repositoryConfig, packageConfig.TargetFrameworks);

        var currentPackageVersions = nugetUpdaterContext.GetCurrentPackageVersions();
        var sourceRepositories = nugetUpdaterContext.GetSourceRepositories(
            authConfig.NugetFeedAuthentications,
            packageConfig.FallbackRegistries,
            nugetVersionFetcherFactory
        );
        var ignoreResolver = ignoreResolverFactory.Create(packageConfig.IgnoreEntries);
        var groupResolver = groupResolverFactory.Create(packageConfig.GroupEntries);

        // Analyze dependencies
        var dependencyAnalysisResult = await AnalyzeDependenciesAsync(
            nugetUpdaterContext,
            sourceRepositories,
            ignoreResolver,
            currentPackageVersions,
            cancellationToken
        );

        // Group packages for updates
        var groupedPackagesToUpdate = GroupPackagesForUpdate(
            dependencyAnalysisResult.DependenciesToUpdate,
            dependencyAnalysisResult.PackageFlags,
            groupResolver
        );

        // Process package updates and create pull requests
        var knownPullRequests = await ProcessPackageUpdatesAsync(
            repositoryConfig,
            gitMetadataConfig,
            groupedPackagesToUpdate,
            currentPackageVersions,
            updater,
            cancellationToken
        ).ToListAsync(cancellationToken);

        // Clean up abandoned pull requests
        await CleanupAbandonedPullRequestsAsync(
            updater,
            knownPullRequests,
            cancellationToken
        );
    }

    private async Task<DependencyAnalysisResult> AnalyzeDependenciesAsync(
        NugetUpdaterContext nugetUpdaterContext,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        CancellationToken cancellationToken
    )
    {
        // Resolve possible package versions
        var possiblePackageVersions = await ResolvePossiblePackageVersionsAsync(
            nugetUpdaterContext,
            sourceRepositories,
            ignoreResolver,
            cancellationToken
        );

        var dependencyToUpdate = possiblePackageVersions.ToDictionary(
            x => x.Key,
            x => x.Value
        );

        // Initialize data structures for dependency analysis
        var packageFlags = new Dictionary<Package, EDependencyFlag>();
        var dependenciesToCheck = new Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)>();
        var dependenciesToRevisit = new Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)>();

        // Process dependencies to update
        ProcessDependenciesToUpdate(
            ignoreResolver,
            currentPackageVersions,
            dependencyToUpdate,
            packageFlags,
            dependenciesToCheck
        );

        // Process dependencies to check
        await ProcessDependenciesToCheckAsync(
            ignoreResolver,
            currentPackageVersions,
            packageFlags,
            dependenciesToCheck,
            sourceRepositories,
            nugetUpdaterContext,
            dependenciesToRevisit,
            cancellationToken
        );

        // Process dependencies to revisit
        ProcessDependenciesToRevisit(dependenciesToRevisit, packageFlags);

        return new DependencyAnalysisResult(dependencyToUpdate, packageFlags);
    }

    private Queue<(IReadOnlyCollection<NugetUpdateCandidate<PackageSearchMetadataRegistration>> NugetUpdateCandidates, GroupEntry GroupEntry)> GroupPackagesForUpdate(
        IReadOnlyDictionary<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>> dependencyToUpdate,
        IDictionary<Package, EDependencyFlag> packageFlags,
        GroupResolver groupResolver
    )
    {
        var packagesToUpdate = FilterPackagesToUpdate(dependencyToUpdate, packageFlags)
            .Select(x => new
            {
                NugetUpdateCandidate = new NugetUpdateCandidate<PackageSearchMetadataRegistration>(
                    x.Key,
                    x.Value
                ),
                GroupEntry = groupResolver.ResolveGroup(x.Key.NugetPackage.GetPackageName()),
            })
            .GroupBy(x => x.GroupEntry, x => x.NugetUpdateCandidate);

        var groupedPackagesToUpdateQueue = new Queue<(IReadOnlyCollection<NugetUpdateCandidate<PackageSearchMetadataRegistration>> NugetUpdateCandidates, GroupEntry GroupEntry)>();

        foreach (var grouping in packagesToUpdate)
        {
            var groupEntry = grouping.Key;
            if (groupEntry == groupResolver.Empty)
            {
                foreach (var nugetUpdateCandidate in grouping)
                {
                    groupedPackagesToUpdateQueue.Enqueue((
                        [nugetUpdateCandidate],
                        new GroupEntry($"{nugetUpdateCandidate.NugetDependency.NugetPackage.GetPackageName()}/{nugetUpdateCandidate.PackageVersion.GetSerializedVersion()}", [])
                    ));
                }
            }
            else
            {
                groupedPackagesToUpdateQueue.Enqueue((grouping.ToList(), groupEntry));
            }
        }

        return groupedPackagesToUpdateQueue;
    }

    private async IAsyncEnumerable<string> ProcessPackageUpdatesAsync(
        RepositoryConfig repositoryConfig,
        GitMetadataConfig gitMetadataConfig,
        Queue<(IReadOnlyCollection<NugetUpdateCandidate<PackageSearchMetadataRegistration>> NugetUpdateCandidates, GroupEntry GroupEntry)> groupedPackagesToUpdateQueue,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        string updater,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        using var sourceVersioning = sourceVersioningFactory.CreateSourceVersioning(repositoryConfig.RepositoryPath);

        while (groupedPackagesToUpdateQueue.TryDequeue(out var groupedPackagesToUpdate))
        {
            var pullRequestId = await ProcessSinglePackageGroupAsync(
                repositoryConfig,
                gitMetadataConfig,
                groupedPackagesToUpdate.GroupEntry,
                groupedPackagesToUpdate.NugetUpdateCandidates,
                currentPackageVersions,
                sourceVersioning,
                updater,
                cancellationToken
            );

            if (pullRequestId is not null)
            {
                yield return pullRequestId;
            }
        }
    }

    private async Task<string?> ProcessSinglePackageGroupAsync(
        RepositoryConfig repositoryConfig,
        GitMetadataConfig gitMetadataConfig,
        GroupEntry groupEntry,
        IReadOnlyCollection<NugetUpdateCandidate<PackageSearchMetadataRegistration>> nugetUpdateCandidates,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        ISourceVersioning sourceVersioning,
        string updater,
        CancellationToken cancellationToken
    )
    {
        using var temporaryDirectory = new TemporaryDirectoryProvider(create: false);
        using var gitWorkspace = sourceVersioning.CreateWorkspace(
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
            updatedPackages,
            pullRequestId,
            repositoryConfig,
            gitMetadataConfig,
            groupEntry,
            nugetUpdateCandidates,
            updater,
            cancellationToken
        );
    }

    private async IAsyncEnumerable<NugetUpdateCandidate<PackageSearchMetadataRegistration>> UpdatePackageVersionsAsync(
        ISourceVersioningWorkspace gitWorkspace,
        IReadOnlyCollection<NugetUpdateCandidate<PackageSearchMetadataRegistration>> packagesToUpdate,
        Dictionary<string, PackageVersion> groupPackageVersions,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var packagesToUpdateQueue = new Queue<(NugetUpdateCandidate<PackageSearchMetadataRegistration> NugetUpdateCandidate, int Epoch)>(
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
        NugetUpdateCandidate<PackageSearchMetadataRegistration> nugetUpdateCandidate,
        Dictionary<string, PackageVersion> groupPackageVersions
    )
    {
        if (
            !nugetVersionWriter.IsCompatibleWithCurrentVersions(
                nugetUpdateCandidate.PackageVersion,
                groupPackageVersions,
                out var conflictingPackageVersion
            )
        )
        {
            logger.LogError(
                "Cannot update '{PackageName}' to version '{Version}': it depends on '{ConflictingPackageName}' version '{ConflictingPackageVersionRequired}', but the current solution uses version '{ConflictingPackageVersionCurrent}'",
                nugetUpdateCandidate.NugetDependency.NugetPackage.GetPackageName(),
                nugetUpdateCandidate.PackageVersion.GetSerializedVersion(),
                conflictingPackageVersion.Name,
                conflictingPackageVersion.Version.GetSerializedVersion(),
                groupPackageVersions[conflictingPackageVersion.Name].GetSerializedVersion()
            );
        }
        else
        {
            logger.LogError(
                "Cannot set version '{Version}' for package '{PackageName}' due to conflicting version constraints from other packages",
                nugetUpdateCandidate.PackageVersion.GetSerializedVersion(),
                nugetUpdateCandidate.NugetDependency.NugetPackage.GetPackageName()
            );
        }
    }

    private async Task<string?> HandlePullRequestAsync(
        ISourceVersioningWorkspace gitWorkspace,
        List<NugetUpdateCandidate<PackageSearchMetadataRegistration>> updatedPackages,
        string? pullRequestId,
        RepositoryConfig repositoryConfig,
        GitMetadataConfig gitMetadataConfig,
        GroupEntry groupEntry,
        IReadOnlyCollection<NugetUpdateCandidate<PackageSearchMetadataRegistration>> nugetUpdateCandidates,
        string updater,
        CancellationToken cancellationToken
    )
    {
        if (
            gitWorkspace.HasUncommitedChanges()
            && updatedPackages.GetCommitMessage() is { } commitMessage
        )
        {
            // Commit changes and create/update PR
            gitWorkspace.CommitChanges(
                message: commitMessage,
                authorName: gitMetadataConfig.CommitAuthor,
                authorEmail: gitMetadataConfig.CommitAuthorEmail
            );

            await RestoreNugetPackagesAsync(gitWorkspace, repositoryConfig.SubdirectoryPath, gitMetadataConfig, cancellationToken);
            gitWorkspace.Push();

            return await CreateOrUpdatePullRequestAsync(
                gitWorkspace,
                pullRequestId,
                repositoryConfig,
                gitMetadataConfig,
                groupEntry,
                nugetUpdateCandidates,
                commitMessage,
                updater,
                cancellationToken
            );
        }

        if (
            pullRequestId is not null
            && updatedPackages.GetCommitMessage() is { } commitMessage2
        )
        {
            // Just update PR title and description if already exists
            await repositoryClient.UpdatePullRequestAsync(
                pullRequestId: pullRequestId,
                title: groupEntry.GetTitle(nugetUpdateCandidates),
                description: commitMessage2,
                cancellationToken
            );

            return pullRequestId;
        }

        return null;
    }

    private async Task RestoreNugetPackagesAsync(
        ISourceVersioningWorkspace gitWorkspace,
        string? subdirectoryPath,
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
        GroupEntry groupEntry,
        IReadOnlyCollection<NugetUpdateCandidate<PackageSearchMetadataRegistration>> nugetUpdateCandidates,
        string commitMessage,
        string updater,
        CancellationToken cancellationToken
    )
    {
        if (pullRequestId is not null)
        {
            await repositoryClient.UpdatePullRequestAsync(
                pullRequestId: pullRequestId,
                title: groupEntry.GetTitle(nugetUpdateCandidates),
                description: commitMessage,
                cancellationToken
            );
            return pullRequestId;
        }

        return await repositoryClient.CreatePullRequestAsync(
            branchName: gitWorkspace.GetBranchName(),
            targetBranchName: repositoryConfig.SourceBranchName,
            title: groupEntry.GetTitle(nugetUpdateCandidates),
            description: commitMessage,
            gitMetadataConfig.Milestone,
            gitMetadataConfig.Reviewers,
            updater: updater,
            cancellationToken
        );
    }

    private async Task CleanupAbandonedPullRequestsAsync(
        string updater,
        IReadOnlyCollection<string> knownPullRequests,
        CancellationToken cancellationToken
    )
    {
        foreach (var pullRequest in await repositoryClient.ListActivePullRequestsAsync(updater, cancellationToken))
        {
            if (!knownPullRequests.Contains(pullRequest.PullRequestId))
            {
                await repositoryClient.AbandonPullRequestAsync(pullRequest, cancellationToken);
            }
        }
    }

    private record DependencyAnalysisResult(
        IReadOnlyDictionary<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>> DependenciesToUpdate,
        IDictionary<Package, EDependencyFlag> PackageFlags
    );

    private void ProcessDependenciesToUpdate(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        IReadOnlyDictionary<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>> dependencyToUpdate,
        IDictionary<Package, EDependencyFlag> packageFlags,
        Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck
    )
    {
        foreach (var (dependency, possiblePackageVersions) in dependencyToUpdate)
        {
            foreach (var possiblePackageVersion in possiblePackageVersions)
            {
                foreach (var compatiblePackageDependencyGroup in possiblePackageVersion.CompatiblePackageDependencyGroups)
                {
                    _ = ProcessPackageDependencyGroup(
                        ignoreResolver,
                        currentPackageVersions,
                        packageFlags,
                        dependenciesToCheck,
                        compatiblePackageDependencyGroup,
                        dependency.TargetFrameworks
                    ).Count();
                }
            }
        }
    }

    private IEnumerable<Package> ProcessPackageDependencyGroup(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        IDictionary<Package, EDependencyFlag> packageFlags,
        Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck,
        PackageDependencyGroup compatiblePackageDependencyGroup,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks
    )
    {
        foreach (var packageDependency in compatiblePackageDependencyGroup.Packages)
        {
            if (packageDependency.VersionRange.MinVersion is not { } minVersion)
            {
                continue;
            }

            var dependentPackage = new Package(packageDependency.Id, minVersion.MapToPackageVersion());

            var isDependencyIgnored = ignoredDependenciesResolver.IsDependencyIgnored(
                packageDependency,
                ignoreResolver,
                currentPackageVersions
            );

            if (isDependencyIgnored)
            {
                packageFlags[dependentPackage] = EDependencyFlag.ContainsIgnoredDependency;
            }
            else if (packageFlags.TryAdd(dependentPackage, EDependencyFlag.Unknown))
            {
                dependenciesToCheck.Enqueue((dependentPackage, targetFrameworks));
            }

            yield return dependentPackage;
        }
    }

    private async Task ProcessDependenciesToCheckAsync(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        IDictionary<Package, EDependencyFlag> packageFlags,
        Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        NugetUpdaterContext context,
        Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)> dependenciesToRevisit,
        CancellationToken cancellationToken
    )
    {
        while (dependenciesToCheck.TryDequeue(out var item))
        {
            var (package, targetFrameworks) = item;

            var nugetSources = context.GetSourcesForPackage(package.Name, logger);

            foreach (var nugetSource in nugetSources)
            {
                if (sourceRepositories.TryGetValue(nugetSource, out var sourceRepository))
                {
                    using var nugetCache = new SourceCacheContext();

                    var packageMetadataMap = new Dictionary<EPackageSource, IPackageSearchMetadata>();

                    var defaultSourcePackageMetadata = await nugetVersionFetcher.FetchPackageMetadataAsync(
                        sourceRepository.SourceRepository,
                        package,
                        nugetCache,
                        cancellationToken
                    );

                    if (defaultSourcePackageMetadata is not null)
                    {
                        packageMetadataMap.Add(EPackageSource.Default, defaultSourcePackageMetadata);
                    }

                    if (sourceRepository.FallbackSourceRepository is { } fallbackSourceRepository)
                    {
                        var fallbackSourcePackageMetadata = await nugetVersionFetcher.FetchPackageMetadataAsync(
                            fallbackSourceRepository,
                            package,
                            nugetCache,
                            cancellationToken
                        );

                        if (fallbackSourcePackageMetadata is not null)
                        {
                            packageMetadataMap.Add(EPackageSource.Fallback, fallbackSourcePackageMetadata);
                        }
                    }

                    var packageMetadata = packageMetadataMap.MapToPackageVersion();

                    if (packageMetadata is null)
                    {
                        continue;
                    }

                    ProcessPackageMetadata(
                        ignoreResolver,
                        currentPackageVersions,
                        package,
                        targetFrameworks,
                        packageMetadata,
                        packageFlags,
                        dependenciesToCheck,
                        dependenciesToRevisit
                    );
                }
            }
        }
    }

    private void ProcessPackageMetadata(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        Package package,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks,
        PackageVersion<PackageSearchMetadataRegistration> packageVersion,
        IDictionary<Package, EDependencyFlag> packageFlags,
        Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck,
        Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)> dependenciesToRevisit
    )
    {
        var packageSearchMetadataRegistration = GetPreferredPackageSearchMetadataRegistration(packageVersion.OriginalReference);
        var compatiblePackageDependencyGroups = targetFrameworksResolver.GetCompatiblePackageDependencyGroups(
            packageSearchMetadataRegistration,
            targetFrameworks
        );

        dependenciesToRevisit.Push((package, [
            .. compatiblePackageDependencyGroups.Aggregate(
                [],
                (IEnumerable<Package> acc, PackageDependencyGroup compatiblePackageDependencyGroup) => acc.Concat(ProcessPackageDependencyGroup(
                    ignoreResolver,
                    currentPackageVersions,
                    packageFlags,
                    dependenciesToCheck,
                    compatiblePackageDependencyGroup,
                    targetFrameworks
                ))
            ),
        ]));
    }

    private TOriginalReference GetPreferredPackageSearchMetadataRegistration<TOriginalReference>(
        IReadOnlyDictionary<EPackageSource, TOriginalReference> originalReference
    ) => originalReference
        .OrderBy(x => x.Key switch
        {
            EPackageSource.Default => 0,
            EPackageSource.Fallback => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(x.Key), x.Key, null),
        })
        .Select(x => x.Value)
        .First();

    private void ProcessDependenciesToRevisit(
        Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)> dependenciesToRevisit,
        IDictionary<Package, EDependencyFlag> packageFlags
    )
    {
        while (dependenciesToRevisit.TryPop(out var item))
        {
            var (package, dependencies) = item;

            var isIgnored = dependencies.Any(dependency =>
                packageFlags.TryGetValue(dependency, out var dependencyFlag)
                && dependencyFlag is EDependencyFlag.ContainsIgnoredDependency
            );

            packageFlags[package] = isIgnored
                ? EDependencyFlag.ContainsIgnoredDependency
                : EDependencyFlag.Valid;
        }
    }

    private IEnumerable<KeyValuePair<NugetDependency, PackageVersion<PackageSearchMetadataRegistration>>> FilterPackagesToUpdate(
        IReadOnlyDictionary<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>> dependencyToUpdate,
        IDictionary<Package, EDependencyFlag> packageFlags
    )
    {
        foreach (var (dependency, possiblePackageVersions) in dependencyToUpdate)
        {
            var newPossiblePackageVersions = possiblePackageVersions
                .Select(possiblePackageVersion => possiblePackageVersion with
                {
                    CompatiblePackageDependencyGroups =
                    [
                        .. possiblePackageVersion
                            .CompatiblePackageDependencyGroups
                            .Where(group => group.Packages.All(package =>
                                packageFlags.TryGetValue(
                                    new Package(package.Id, package.VersionRange.MinVersion!.MapToPackageVersion()),
                                    out var flag
                                ) && flag is EDependencyFlag.Valid
                            )),
                    ],
                })
                .Where(x => x.CompatiblePackageDependencyGroups.Count > 0)
                .ToList();

            if (newPossiblePackageVersions.Count > 0)
            {
                yield return KeyValuePair.Create(
                    dependency,
                    newPossiblePackageVersions
                        .OrderByDescending(x => x.PackageVersion)
                        .First()
                        .PackageVersion
                );
            }
        }
    }

    private enum EDependencyFlag
    {
        Unknown,
        ContainsIgnoredDependency,
        Valid,
    }

    private NugetUpdaterContext CreateContext(
        RepositoryConfig repositoryConfig,
        IReadOnlyCollection<NugetTargetFramework> defaultTargetFrameworks
    )
    {
        var nugetConfigurations = nugetFinder.GetNugetConfig(repositoryConfig)
            .SelectMany(x => nugetConfigParser.Parse(repositoryConfig.RepositoryPath, x))
            .ToList();

        var dependencies = nugetFinder.GetDirectoryPackagesPropsFiles(repositoryConfig)
            .SelectMany(x => nugetDirectoryPackagesPropsParser.Parse(repositoryConfig.RepositoryPath, x, defaultTargetFrameworks))
            .Concat(
                nugetFinder.GetAllCsprojFiles(repositoryConfig)
                    .SelectMany(x => nugetCsprojParser.Parse(repositoryConfig.RepositoryPath, x))
                    .Where(x => x.NugetPackage is NugetPackageReference { VersionRange: not null })
            )
            .ToList();

        return new NugetUpdaterContext(
            nugetConfigurations,
            dependencies
        );
    }

    private sealed record PossiblePackageVersion(
        PackageVersion<PackageSearchMetadataRegistration> PackageVersion,
        IReadOnlyCollection<PackageDependencyGroup> CompatiblePackageDependencyGroups
    );

    private async Task<IEnumerable<KeyValuePair<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>>>> ResolvePossiblePackageVersionsAsync(
        NugetUpdaterContext nugetUpdaterContext,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        IgnoreResolver ignoreResolver,
        CancellationToken cancellationToken
    )
    {
        var dependencies = nugetUpdaterContext.MapSourceToDependency(logger);
        var results = new ConcurrentBag<KeyValuePair<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>>>();

        // Limit parallelism to avoid overwhelming system resources
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2),
            CancellationToken = cancellationToken,
        };

        // Process dependencies in parallel
        await Parallel.ForEachAsync(dependencies, parallelOptions, async (dependencyPair, token) =>
        {
            var (dependency, sources) = dependencyPair;
            var dependencyName = dependency.NugetPackage.GetPackageName();
            var dependencyVersion = dependency.NugetPackage.GetVersion();

            try
            {
                var versions = await FetchDependencyVersionsAsync(
                    dependency,
                    sources,
                    sourceRepositories,
                    token
                );

                IReadOnlyCollection<PossiblePackageVersion> futureVersions = futureVersionResolver.ResolveFutureVersion(
                        dependencyName,
                        dependencyVersion,
                        versions,
                        ignoreResolver
                    )
                    .Select(x => new PossiblePackageVersion(
                        x,
                        targetFrameworksResolver.GetCompatiblePackageDependencyGroups(
                            GetPreferredPackageSearchMetadataRegistration(x.OriginalReference),
                            dependency.TargetFrameworks
                        ).ToList()
                    ))
                    .Where(x => x.CompatiblePackageDependencyGroups.Count > 0)
                    .ToList();

                if (futureVersions.Count == 0)
                {
                    logger.LogDebug("The dependency {DependencyName} with version {Version} is up to date", dependencyName, dependencyVersion);
                    return;
                }

                results.Add(KeyValuePair.Create(dependency, futureVersions));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing dependency {DependencyName} with version {Version}", dependencyName, dependencyVersion);
            }
        });

        return results.OrderBy(r => r.Key.NugetPackage.GetPackageName());
    }

    private async Task<IReadOnlyCollection<PackageVersion<PackageSearchMetadataRegistration>>> FetchDependencyVersionsAsync(
        NugetDependency dependency,
        IReadOnlyCollection<NugetSource> sources,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        CancellationToken cancellationToken
    )
    {
        var versions = new List<PackageVersion<PackageSearchMetadataRegistration>>();
        var tasks = sources.Select<NugetSource, Task<IEnumerable<PackageVersion<PackageSearchMetadataRegistration>>>>(async nugetSource =>
        {
            if (sourceRepositories.TryGetValue(nugetSource, out var sourceRepository))
            {
                using var nugetCache = new SourceCacheContext();

                var rawPackageVersions = await nugetVersionFetcher.FetchPackageVersionsAsync(
                    sourceRepository.SourceRepository,
                    dependency,
                    nugetCache,
                    cancellationToken
                );
                var packageVersions = rawPackageVersions.Select(x => (Metadata: x, PackageVersion: x.MapToPackageVersion(), PackageSource: EPackageSource.Default));

                if (sourceRepository.FallbackSourceRepository is { } fallbackSourceRepository)
                {
                    var fallbackPackageVersions = await nugetVersionFetcher.FetchPackageVersionsAsync(
                        fallbackSourceRepository,
                        dependency,
                        nugetCache,
                        cancellationToken
                    );

                    packageVersions = packageVersions.Concat(fallbackPackageVersions.Select(x => (Metadata: x, PackageVersion: x.MapToPackageVersion(), PackageSource: EPackageSource.Fallback)));
                }

                return packageVersions.GroupBy(x => x.PackageVersion)
                    .Select(x => x.Key.MapToPackageVersion(x.ToDictionary(
                        d => d.PackageSource,
                        d => d.Metadata
                    )));
            }

            return [];
        });

        await foreach (var job in Task.WhenEach(tasks).WithCancellation(cancellationToken))
        {
            versions.AddRange(await job);
        }

        return versions;
    }
}
