using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class NugetVersionWriter(
    NugetDirectoryPackagesPropsVersionWriter directoryPackagesPropsVersionWriter,
    NugetCsprojVersionWriter csprojVersionWriter,
    IRepositoryClient repositoryClient
)
{
    public async Task<ESetVersion> TrySetVersion<T>(
        NugetUpdateCandidate<T> nugetUpdateCandidate,
        ISourceVersioningWorkspace gitWorkspace,
        IDictionary<string, PackageVersion> groupPackageVersions,
        CancellationToken cancellationToken
    )
    {
        if (!IsCompatibleWithCurrentVersions(nugetUpdateCandidate.PackageVersion, groupPackageVersions, out _))
        {
            return ESetVersion.VersionNotSet;
        }

        var workspaceDirectory = gitWorkspace.GetWorkspaceDirectory();

        var targetFullPath = nugetUpdateCandidate.NugetDependency.NugetFile.GetFullPath(workspaceDirectory);

        if (!gitWorkspace.IsPathInsideRepository(targetFullPath))
        {
            return ESetVersion.OutOfRepository;
        }

        if (!nugetUpdateCandidate.PackageVersion.OriginalReference.ContainsKey(EPackageSource.Default))
        {
            await repositoryClient.EnsurePackageVersionIsAvailableAsync(
                nugetUpdateCandidate.NugetDependency.NugetPackage.GetPackageName(),
                nugetUpdateCandidate.PackageVersion.GetSerializedVersion(),
                cancellationToken
            );
        }

        return nugetUpdateCandidate.NugetDependency.NugetFile.Type switch
        {
            ENugetFileType.DirectoryPackagesProps => await directoryPackagesPropsVersionWriter.TrySetVersionAsync(nugetUpdateCandidate, targetFullPath, groupPackageVersions, cancellationToken),
            ENugetFileType.Csproj or ENugetFileType.Targets => await csprojVersionWriter.TrySetVersionAsync(nugetUpdateCandidate, targetFullPath, groupPackageVersions, cancellationToken),
            // ENugetFileType.NugetConfig => false, // we should not update nuget.config
            _ => throw new ArgumentOutOfRangeException(nameof(nugetUpdateCandidate.NugetDependency.NugetFile.Type), nugetUpdateCandidate.NugetDependency.NugetFile.Type, null),
        };
    }

    public bool IsCompatibleWithCurrentVersions<T>(
        PackageVersion<T> packageVersion,
        IDictionary<string, PackageVersion> groupPackageVersions,
        [NotNullWhen(false)] out Package? conflictingPackageVersion
    )
    {
        if (packageVersion is PackageVersion<PackageSearchMetadataRegistration> packageSearchMetadataRegistration)
        {
            foreach (var (_, originalReference) in packageSearchMetadataRegistration.OriginalReference)
            {
                foreach (var dependencySet in originalReference.DependencySets)
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
            }

            conflictingPackageVersion = null;
            return true;
        }

        throw new ArgumentOutOfRangeException(nameof(packageVersion), packageVersion, null);
    }
}
