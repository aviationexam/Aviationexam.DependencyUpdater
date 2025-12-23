using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Interfaces.Repository;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Writers;

public sealed class NugetVersionWriter(
    NugetDirectoryPackagesPropsVersionWriter directoryPackagesPropsVersionWriter,
    NugetCsprojVersionWriter csprojVersionWriter,
    DotnetToolsVersionWriter dotnetToolsVersionWriter,
    Optional<IPackageFeedClient> optionalPackageFeedClient,
    TargetFrameworksResolver targetFrameworksResolver
)
{
    public async Task<ESetVersion> TrySetVersion(
        NugetUpdateCandidate nugetUpdateCandidate,
        ISourceVersioningWorkspace gitWorkspace,
        IDictionary<string, IDictionary<string, PackageVersion>> groupPackageVersions,
        CancellationToken cancellationToken
    )
    {
        if (
            !IsCompatibleWithCurrentVersions(
                nugetUpdateCandidate.PossiblePackageVersion,
                nugetUpdateCandidate.NugetDependency.TargetFrameworks,
                groupPackageVersions,
                out _
            )
        )
        {
            return ESetVersion.VersionNotSet;
        }

        var workspaceDirectory = gitWorkspace.GetWorkspaceDirectory();

        var targetFullPath = nugetUpdateCandidate.NugetDependency.NugetFile.GetFullPath(workspaceDirectory);

        if (!gitWorkspace.IsPathInsideRepository(targetFullPath))
        {
            return ESetVersion.OutOfRepository;
        }

        if (optionalPackageFeedClient.Value is { } packageFeedClient)
        {
            await packageFeedClient.EnsurePackageVersionIsAvailableAsync(
                nugetUpdateCandidate.NugetDependency.NugetPackage.GetPackageName(),
                nugetUpdateCandidate.PossiblePackageVersion.PackageVersion.GetSerializedVersion(),
                cancellationToken
            );
        }

        return nugetUpdateCandidate.NugetDependency.NugetFile.Type switch
        {
            ENugetFileType.DirectoryPackagesProps => await directoryPackagesPropsVersionWriter.TrySetVersionAsync(nugetUpdateCandidate, targetFullPath, groupPackageVersions, cancellationToken),
            ENugetFileType.Csproj or ENugetFileType.Targets => await csprojVersionWriter.TrySetVersionAsync(nugetUpdateCandidate, targetFullPath, groupPackageVersions, cancellationToken),
            ENugetFileType.DotnetTools => await dotnetToolsVersionWriter.TrySetVersionAsync(nugetUpdateCandidate, targetFullPath, groupPackageVersions, cancellationToken),
            // ENugetFileType.NugetConfig => false, // we should not update nuget.config
            _ => throw new ArgumentOutOfRangeException(nameof(nugetUpdateCandidate.NugetDependency.NugetFile.Type), nugetUpdateCandidate.NugetDependency.NugetFile.Type, null),
        };
    }

    public bool IsCompatibleWithCurrentVersions(
        PossiblePackageVersion possiblePackageVersion,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks,
        IDictionary<string, IDictionary<string, PackageVersion>> groupPackageVersions,
        [NotNullWhen(false)] out Package? conflictingPackageVersion
    )
    {
        foreach (var dependencySet in possiblePackageVersion.CompatibleDependencySets)
        {
            foreach (var dependencyPackage in dependencySet.Packages)
            {
                if (groupPackageVersions.TryGetValue(dependencyPackage.Id, out var frameworkVersions))
                {
                    // Get all compatible target frameworks for version checking
                    var compatibleFrameworks = targetFrameworksResolver.GetCompatibleTargetFrameworks(
                        targetFrameworks,
                        [.. frameworkVersions.Keys.AsValueEnumerable().Intersect(targetFrameworks.AsValueEnumerable().Select(x => x.TargetFramework))]
                    );

                    foreach (var tfm in compatibleFrameworks)
                    {
                        if (frameworkVersions.TryGetValue(tfm, out var dependencyCurrentVersion))
                        {
                            if (
                                dependencyPackage.MinVersion is { } dependencyPackageVersion
                                && dependencyPackageVersion > dependencyCurrentVersion
                            )
                            {
                                conflictingPackageVersion = new Package(
                                    dependencyPackage.Id,
                                    dependencyPackageVersion
                                );
                                return false;
                            }
                        }
                    }
                }
            }
        }

        conflictingPackageVersion = null;
        return true;
    }
}
