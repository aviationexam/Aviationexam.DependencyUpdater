using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
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
        string repositoryPath,
        string? subdirectoryPath,
        string? sourceBranchName,
        string? milestone,
        IReadOnlyCollection<string> reviewers,
        string commitAuthor,
        string commitAuthorEmail,
        IReadOnlyCollection<NugetFeedAuthentication> nugetFeedAuthentications,
        IReadOnlyDictionary<string, string> fallbackRegistries,
        IReadOnlyCollection<NugetTargetFramework> defaultTargetFrameworks,
        IReadOnlyCollection<IgnoreEntry> ignoreEntries,
        IReadOnlyCollection<GroupEntry> groupEntries,
        CancellationToken cancellationToken = default
    )
    {
        var nugetUpdaterContext = CreateContext(repositoryPath, subdirectoryPath, defaultTargetFrameworks);

        var currentPackageVersions = nugetUpdaterContext.GetCurrentPackageVersions();
        var sourceRepositories = nugetUpdaterContext.GetSourceRepositories(
            nugetFeedAuthentications,
            fallbackRegistries,
            nugetVersionFetcherFactory
        );
        var ignoreResolver = ignoreResolverFactory.Create(ignoreEntries);
        var groupResolver = groupResolverFactory.Create(groupEntries);

        var dependencyToUpdate = await ResolvePossiblePackageVersionsAsync(
            nugetUpdaterContext,
            sourceRepositories,
            ignoreResolver,
            cancellationToken
        ).ToDictionaryAsync(
            x => x.Key,
            x => x.Value,
            cancellationToken
        );

        var packageFlags = new Dictionary<Package, EDependencyFlag>();
        var dependenciesToCheck = new Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)>();
        var dependenciesToRevisit = new Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)>();

        ProcessDependenciesToUpdate(
            ignoreResolver,
            currentPackageVersions,
            dependencyToUpdate,
            packageFlags,
            dependenciesToCheck
        );

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

        ProcessDependenciesToRevisit(dependenciesToRevisit, packageFlags);

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

        using var sourceVersioning = sourceVersioningFactory.CreateSourceVersioning(repositoryPath);

        var knownPullRequests = new List<string>();
        while (groupedPackagesToUpdateQueue.TryDequeue(out var groupedPackagesToUpdate))
        {
#pragma warning disable CA2000
            using var temporaryDirectory = new TemporaryDirectoryProvider(create: false);
#pragma warning restore CA2000
            using var gitWorkspace = sourceVersioning.CreateWorkspace(
                temporaryDirectory.TemporaryDirectory,
                sourceBranchName: sourceBranchName,
                branchName: groupedPackagesToUpdate.GroupEntry.GetBranchName(),
                worktreeName: groupedPackagesToUpdate.GroupEntry.GroupName.Replace('/', '-')
            );

            gitWorkspace.TryPullRebase(
                sourceBranchName: sourceBranchName,
                authorName: commitAuthor,
                authorEmail: commitAuthorEmail
            );

            var groupPackageVersions = currentPackageVersions.ToDictionary();

            var updatedPackages = new List<NugetUpdateCandidate<PackageSearchMetadataRegistration>>();
            var packagesToUpdateQueue = new Queue<(NugetUpdateCandidate<PackageSearchMetadataRegistration> NugetUpdateCandidate, int Epoch)>(
                groupedPackagesToUpdate.NugetUpdateCandidates.Select(x => (x, 0))
            );
            var epoch = 1;
            while (packagesToUpdateQueue.TryDequeue(out var packageToUpdate))
            {
                if (packageToUpdate.Epoch == epoch)
                {
                    if (
                        !nugetVersionWriter.IsCompatibleWithCurrentVersions(
                            packageToUpdate.NugetUpdateCandidate.PackageVersion,
                            groupPackageVersions,
                            out var conflictingPackageVersion
                        )
                    )
                    {
                        logger.LogError(
                            "Cannot update '{PackageName}' to version '{Version}': it depends on '{ConflictingPackageName}' version '{ConflictingPackageVersionRequired}', but the current solution uses version '{ConflictingPackageVersionCurrent}'",
                            packageToUpdate.NugetUpdateCandidate.NugetDependency.NugetPackage.GetPackageName(),
                            packageToUpdate.NugetUpdateCandidate.PackageVersion.GetSerializedVersion(),
                            conflictingPackageVersion.Name,
                            conflictingPackageVersion.Version.GetSerializedVersion(),
                            groupPackageVersions[conflictingPackageVersion.Name].GetSerializedVersion()
                        );
                    }
                    else
                    {
                        logger.LogError(
                            "Cannot set version '{Version}' for package '{PackageName}' due to conflicting version constraints from other packages",
                            packageToUpdate.NugetUpdateCandidate.PackageVersion.GetSerializedVersion(),
                            packageToUpdate.NugetUpdateCandidate.NugetDependency.NugetPackage.GetPackageName()
                        );
                    }

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
                        updatedPackages.Add(packageToUpdate.NugetUpdateCandidate);
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

            var pullRequestId = await repositoryClient.GetPullRequestForBranchAsync(
                branchName: gitWorkspace.GetBranchName(),
                cancellationToken
            );

            if (pullRequestId is not null)
            {
                knownPullRequests.Add(pullRequestId);
            }

            if (
                gitWorkspace.HasUncommitedChanges()
                && updatedPackages.GetCommitMessage() is { } commitMessage
            )
            {
                gitWorkspace.CommitChanges(
                    message: commitMessage,
                    authorName: commitAuthor,
                    authorEmail: commitAuthorEmail
                );

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
                        authorName: commitAuthor,
                        authorEmail: commitAuthorEmail
                    );
                }

                gitWorkspace.Push();

                if (pullRequestId is not null)
                {
                    await repositoryClient.UpdatePullRequestAsync(
                        pullRequestId: pullRequestId,
                        title: groupedPackagesToUpdate.GroupEntry.GetTitle(groupedPackagesToUpdate.NugetUpdateCandidates),
                        description: commitMessage,
                        cancellationToken
                    );
                }
                else
                {
                    pullRequestId = await repositoryClient.CreatePullRequestAsync(
                        branchName: gitWorkspace.GetBranchName(),
                        targetBranchName: sourceBranchName,
                        title: groupedPackagesToUpdate.GroupEntry.GetTitle(groupedPackagesToUpdate.NugetUpdateCandidates),
                        description: commitMessage,
                        milestone,
                        reviewers,
                        cancellationToken
                    );

                    knownPullRequests.Add(pullRequestId);
                }
            }
            else if (
                pullRequestId is not null
                && updatedPackages.GetCommitMessage() is { } commitMessage2
            )
            {
                await repositoryClient.UpdatePullRequestAsync(
                    pullRequestId: pullRequestId,
                    title: groupedPackagesToUpdate.GroupEntry.GetTitle(groupedPackagesToUpdate.NugetUpdateCandidates),
                    description: commitMessage2,
                    cancellationToken
                );
            }
        }
    }

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

                    var packageMetadata = await nugetVersionFetcher.FetchPackageMetadataAsync(
                        sourceRepository.SourceRepository,
                        package,
                        nugetCache,
                        cancellationToken
                    );

                    if (
                        packageMetadata is null
                        && sourceRepository.FallbackSourceRepository is { } fallbackSourceRepository)
                    {
                        packageMetadata = await nugetVersionFetcher.FetchPackageMetadataAsync(
                            fallbackSourceRepository,
                            package,
                            nugetCache,
                            cancellationToken
                        );
                    }

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
        IPackageSearchMetadata packageMetadata,
        IDictionary<Package, EDependencyFlag> packageFlags,
        Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck,
        Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)> dependenciesToRevisit
    )
    {
        if (packageMetadata is PackageSearchMetadataRegistration packageSearchMetadataRegistration)
        {
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
        else
        {
            throw new ArgumentOutOfRangeException(nameof(packageMetadata), packageMetadata, null);
        }
    }

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
                yield return KeyValuePair.Create(dependency, newPossiblePackageVersions
                    .OrderBy(x => x.PackageVersion)
                    .First()
                    .PackageVersion);
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
        string repositoryPath,
        string? subdirectoryPath,
        IReadOnlyCollection<NugetTargetFramework> defaultTargetFrameworks
    )
    {
        var directoryPath = Path.Join(repositoryPath, subdirectoryPath);

        var nugetConfigurations = nugetFinder.GetNugetConfig(repositoryPath, directoryPath)
            .SelectMany(x => nugetConfigParser.Parse(repositoryPath, x))
            .ToList();

        var dependencies = nugetFinder.GetDirectoryPackagesPropsFiles(repositoryPath, directoryPath)
            .SelectMany(x => nugetDirectoryPackagesPropsParser.Parse(repositoryPath, x, defaultTargetFrameworks))
            .Concat(
                nugetFinder.GetAllCsprojFiles(repositoryPath, directoryPath)
                    .SelectMany(x => nugetCsprojParser.Parse(repositoryPath, x))
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

    private async IAsyncEnumerable<KeyValuePair<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>>> ResolvePossiblePackageVersionsAsync(
        NugetUpdaterContext nugetUpdaterContext,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        IgnoreResolver ignoreResolver,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var dependencies = nugetUpdaterContext.MapSourceToDependency(logger);
        foreach (var (dependency, sources) in dependencies)
        {
            var versions = await FetchDependencyVersionsAsync(
                dependency,
                sources,
                sourceRepositories,
                cancellationToken
            );

            var dependencyName = dependency.NugetPackage.GetPackageName();
            var dependencyVersion = dependency.NugetPackage.GetVersion();

            IReadOnlyCollection<PossiblePackageVersion> futureVersions = futureVersionResolver.ResolveFutureVersion(
                    dependencyName,
                    dependencyVersion,
                    versions.Select(NugetMapper.MapToPackageVersion),
                    ignoreResolver
                )
                .Select(x => new PossiblePackageVersion(
                    x,
                    targetFrameworksResolver.GetCompatiblePackageDependencyGroups(
                        x.OriginalReference,
                        dependency.TargetFrameworks
                    ).ToList()
                ))
                .Where(x => x.CompatiblePackageDependencyGroups.Count > 0)
                .ToList();

            if (futureVersions.Count == 0)
            {
                logger.LogDebug("The dependency {DependencyName} with version {Version} is up to date", dependencyName, dependencyVersion);
                continue;
            }

            yield return KeyValuePair.Create(dependency, futureVersions);
        }
    }

    private async Task<IReadOnlyCollection<IPackageSearchMetadata>> FetchDependencyVersionsAsync(
        NugetDependency dependency,
        IReadOnlyCollection<NugetSource> sources,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        CancellationToken cancellationToken
    )
    {
        var versions = new List<IPackageSearchMetadata>();
        var tasks = sources.Select(async nugetSource =>
        {
            if (sourceRepositories.TryGetValue(nugetSource, out var sourceRepository))
            {
                using var nugetCache = new SourceCacheContext();

                var packageVersions = await nugetVersionFetcher.FetchPackageVersionsAsync(
                    sourceRepository.SourceRepository,
                    dependency,
                    nugetCache,
                    cancellationToken
                );

                return packageVersions.ToList();
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
