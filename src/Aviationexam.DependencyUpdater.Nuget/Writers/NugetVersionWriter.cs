using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Repository.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.Writers;

public sealed class NugetVersionWriter(
    NugetDirectoryPackagesPropsVersionWriter directoryPackagesPropsVersionWriter,
    NugetCsprojVersionWriter csprojVersionWriter,
    DotnetToolsVersionWriter dotnetToolsVersionWriter,
    Optional<IPackageFeedClient> packageFeedClient
)
{
    public async Task<ESetVersion> TrySetVersion(
        NugetUpdateCandidate nugetUpdateCandidate,
        ISourceVersioningWorkspace gitWorkspace,
        IDictionary<string, PackageVersion> groupPackageVersions,
        CancellationToken cancellationToken
    )
    {
        if (!IsCompatibleWithCurrentVersions(nugetUpdateCandidate.PossiblePackageVersion, groupPackageVersions, out _))
        {
            return ESetVersion.VersionNotSet;
        }

        var workspaceDirectory = gitWorkspace.GetWorkspaceDirectory();

        var targetFullPath = nugetUpdateCandidate.NugetDependency.NugetFile.GetFullPath(workspaceDirectory);

        if (!gitWorkspace.IsPathInsideRepository(targetFullPath))
        {
            return ESetVersion.OutOfRepository;
        }

        if (packageFeedClient.Value is not null)
        {
            await packageFeedClient.Value.EnsurePackageVersionIsAvailableAsync(
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
        IDictionary<string, PackageVersion> groupPackageVersions,
        [NotNullWhen(false)] out Package? conflictingPackageVersion
    )
    {
        foreach (var dependencySet in possiblePackageVersion.CompatiblePackageDependencyGroups)
        {
            foreach (var dependencyPackage in dependencySet.Packages)
            {
                if (groupPackageVersions.TryGetValue(dependencyPackage.Id, out var dependencyCurrentVersion))
                {
                    if (
                        dependencyPackage.VersionRange.MinVersion is { } dependencyPackageMinVersion
                        && dependencyPackageMinVersion.MapToPackageVersion() is { } dependencyPackageVersion
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

        conflictingPackageVersion = null;
        return true;
    }
}
