using Aviationexam.DependencyUpdater.Common;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
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
    TargetFrameworksResolver targetFrameworksResolver,
    IgnoredDependenciesResolver ignoredDependenciesResolver,
    ILogger<NugetUpdater> logger
)
{
    public async Task ProcessUpdatesAsync(
        string directoryPath,
        IReadOnlyCollection<NugetFeedAuthentication> nugetFeedAuthentications,
        IReadOnlyCollection<NugetTargetFramework> defaultTargetFrameworks,
        IReadOnlyCollection<IgnoreEntry> ignoreEntries,
        IReadOnlyCollection<GroupEntry> groupEntries,
        CancellationToken cancellationToken = default
    )
    {
        var nugetUpdaterContext = CreateContext(
            directoryPath,
            defaultTargetFrameworks
        );

        var currentPackageVersions = nugetUpdaterContext.Dependencies
            .Select(x => x.NugetPackage)
            .Select(x => (PackageName: x.GetPackageName(), Version: x.GetVersion()))
            .Where(x => x.Version is not null)
            .GroupBy(x => x.PackageName)
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.Version).First().Version!);

        var sourceRepositories = nugetUpdaterContext.NugetConfigurations
            .GroupBy(x => x.Source)
            .Select(x => x.First())
            .ToDictionary(
                x => x,
                x => nugetVersionFetcherFactory.CreateSourceRepository(x, nugetFeedAuthentications)
            );

        var ignoreResolver = ignoreResolverFactory.Create(ignoreEntries);

        var dependencyToUpdate = await ResolvePossiblePackageVersions(
            nugetUpdaterContext,
            sourceRepositories,
            ignoreResolver,
            cancellationToken
        ).ToDictionaryAsync(
            x => x.dependency,
            x => x.futureVersions,
            cancellationToken
        );

        var packageFlags = new Dictionary<Package, EDependencyFlag>();
        var dependenciesToCheck = new Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)>();
        var dependenciesToRevisit = new Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)>();

        foreach (var (dependency, possiblePackageVersions) in dependencyToUpdate)
        {
            foreach (var possiblePackageVersion in possiblePackageVersions)
            {
                foreach (var compatiblePackageDependencyGroup in possiblePackageVersion.CompatiblePackageDependencyGroups)
                {
                    foreach (var packageDependency in compatiblePackageDependencyGroup.Packages)
                    {
                        var isDependencyIgnored = ignoredDependenciesResolver.IsDependencyIgnored(
                            packageDependency,
                            ignoreResolver,
                            currentPackageVersions
                        );

                        if (packageDependency.VersionRange.MinVersion is not { } minVersion)
                        {
                            continue;
                        }

                        var package = new Package(packageDependency.Id, minVersion.MapToPackageVersion());

                        if (isDependencyIgnored)
                        {
                            packageFlags[package] = EDependencyFlag.ContainsIgnoredDependency;
                        }
                        else
                        {
                            dependenciesToCheck.Enqueue((package, dependency.TargetFrameworks));
                        }
                    }
                }
            }
        }

        while (dependenciesToCheck.TryDequeue(out var item))
        {
            var (package, targetFrameworks) = item;

            if (packageFlags.TryAdd(package, EDependencyFlag.Unknown) is false)
            {
                continue;
            }

            var nugetSources = nugetUpdaterContext.GetSourcesForPackage(package.Name, logger);

            foreach (var nugetSource in nugetSources)
            {
                if (sourceRepositories.TryGetValue(nugetSource, out var sourceRepository))
                {
                    using var nugetCache = new SourceCacheContext();

                    var packageMetadata = await nugetVersionFetcher.FetchPackageMetadataAsync(
                        sourceRepository,
                        package,
                        nugetCache,
                        cancellationToken
                    );

                    if (packageMetadata is null)
                    {
                        continue;
                    }

                    if (packageMetadata is PackageSearchMetadataRegistration packageSearchMetadataRegistration)
                    {
                        var compatiblePackageDependencyGroups = targetFrameworksResolver.GetCompatiblePackageDependencyGroups(
                            packageSearchMetadataRegistration,
                            targetFrameworks
                        );

                        EDependencyFlag? dependencyFlag = null;

                        var dependencies = new List<Package>();

                        foreach (var compatiblePackageDependencyGroup in compatiblePackageDependencyGroups)
                        {
                            foreach (var packageDependency in compatiblePackageDependencyGroup.Packages)
                            {
                                var isDependencyIgnored = ignoredDependenciesResolver.IsDependencyIgnored(
                                    packageDependency,
                                    ignoreResolver,
                                    currentPackageVersions
                                );

                                if (packageDependency.VersionRange.MinVersion is not { } minVersion)
                                {
                                    continue;
                                }

                                var dependentPackage = new Package(packageDependency.Id, minVersion.MapToPackageVersion());

                                if (isDependencyIgnored)
                                {
                                    packageFlags[dependentPackage] = EDependencyFlag.ContainsIgnoredDependency;
                                    dependencyFlag = EDependencyFlag.ContainsIgnoredDependency;
                                }
                                else if (dependencyFlag is null)
                                {
                                    dependencies.Add(dependentPackage);
                                    dependenciesToCheck.Enqueue((dependentPackage, targetFrameworks));
                                }
                            }
                        }

                        if (dependencyFlag.HasValue)
                        {
                            packageFlags[package] = dependencyFlag.Value;
                        }
                        else
                        {
                            dependenciesToRevisit.Push((package, dependencies));
                        }
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(packageMetadata), packageMetadata, null);
                    }
                }
            }
        }

        while (dependenciesToRevisit.TryPop(out var item))
        {
            var (package, dependencies) = item;

            var isIgnored = false;

            foreach (var dependency in dependencies)
            {
                if (packageFlags.TryGetValue(dependency, out var dependencyFlag) && dependencyFlag == EDependencyFlag.ContainsIgnoredDependency)
                {
                    packageFlags[package] = EDependencyFlag.ContainsIgnoredDependency;
                    isIgnored = true;
                    break;
                }
            }

            if (isIgnored)
            {
                packageFlags[package] = EDependencyFlag.Valid;
            }
        }

        var packageToUpdate = new Dictionary<NugetDependency, PackageVersion<PackageSearchMetadataRegistration>>();
        foreach (var (dependency, possiblePackageVersions) in dependencyToUpdate)
        {
            var newPossiblePackageVersions = new List<PossiblePackageVersion>();
            foreach (var possiblePackageVersion in possiblePackageVersions)
            {
                var compatiblePackageDependencyGroups = new List<PackageDependencyGroup>();
                foreach (var compatiblePackageDependencyGroup in possiblePackageVersion.CompatiblePackageDependencyGroups)
                {
                    var isIgnored = false;
                    var isValid = true;
                    foreach (var packageDependency in compatiblePackageDependencyGroup.Packages)
                    {
                        if (packageDependency.VersionRange.MinVersion is not { } minVersion)
                        {
                            continue;
                        }

                        var dependentPackage = new Package(packageDependency.Id, minVersion.MapToPackageVersion());

                        if (packageFlags.TryGetValue(dependentPackage, out var flag) && flag == EDependencyFlag.ContainsIgnoredDependency)
                        {
                            isIgnored = true;
                        }
                        else
                        {
                            isValid = false;
                        }
                    }

                    if (isIgnored is false && isValid)
                    {
                        compatiblePackageDependencyGroups.Add(compatiblePackageDependencyGroup);
                    }
                }

                if (compatiblePackageDependencyGroups.Count > 0)
                {
                    newPossiblePackageVersions.Add(possiblePackageVersion with { CompatiblePackageDependencyGroups = compatiblePackageDependencyGroups });
                }
            }

            if (newPossiblePackageVersions.Count > 0)
            {
                packageToUpdate.Add(dependency, newPossiblePackageVersions.OrderBy(x => x.PackageVersion).First().PackageVersion);
            }
        }

        var a = packageToUpdate;
    }

    private enum EDependencyFlag
    {
        Unknown,
        ContainsIgnoredDependency,
        Valid,
    }

    public NugetUpdaterContext CreateContext(
        string directoryPath,
        IReadOnlyCollection<NugetTargetFramework> defaultTargetFrameworks
    )
    {
        var nugetConfigurations = nugetFinder.GetNugetConfig(directoryPath)
            .SelectMany(nugetConfigParser.Parse)
            .ToList();

        var dependencies = nugetFinder.GetDirectoryPackagesPropsFiles(directoryPath)
            .SelectMany(x => nugetDirectoryPackagesPropsParser.Parse(x, defaultTargetFrameworks))
            .Concat(
                nugetFinder.GetAllCsprojFiles(directoryPath)
                    .SelectMany(x => nugetCsprojParser.Parse(x))
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

    private async IAsyncEnumerable<(NugetDependency dependency, IReadOnlyCollection<PossiblePackageVersion> futureVersions)> ResolvePossiblePackageVersions(
        NugetUpdaterContext nugetUpdaterContext,
        IReadOnlyDictionary<NugetSource, SourceRepository> sourceRepositories,
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

            var futureVersions = futureVersionResolver.ResolveFutureVersion(
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

            yield return (dependency, futureVersions);
        }
    }

    private async Task<IReadOnlyCollection<IPackageSearchMetadata>> FetchDependencyVersionsAsync(
        NugetDependency dependency,
        IReadOnlyCollection<NugetSource> sources,
        IReadOnlyDictionary<NugetSource, SourceRepository> sourceRepositories,
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
                    sourceRepository,
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
