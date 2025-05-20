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
    NugetCsprojVersionWriter csprojVersionWriter
)
{
    public Task<ESetVersion> TrySetVersion<T>(
        NugetUpdateCandidate<T> nugetUpdateCandidate,
        ISourceVersioningWorkspace gitWorkspace,
        IDictionary<string, PackageVersion> groupPackageVersions,
        CancellationToken cancellationToken
    )
    {
        if (!IsCompatibleWithCurrentVersions(nugetUpdateCandidate.PackageVersion, groupPackageVersions, out _))
        {
            return Task.FromResult(ESetVersion.VersionNotSet);
        }

        var workspaceDirectory = gitWorkspace.GetWorkspaceDirectory();

        var targetFullPath = nugetUpdateCandidate.NugetDependency.NugetFile.GetFullPath(workspaceDirectory);

        if (!gitWorkspace.IsPathInsideRepository(targetFullPath))
        {
            return Task.FromResult(ESetVersion.OutOfRepository);
        }

        return nugetUpdateCandidate.NugetDependency.NugetFile.Type switch
        {
            ENugetFileType.DirectoryPackagesProps => directoryPackagesPropsVersionWriter.TrySetVersion(nugetUpdateCandidate, targetFullPath, groupPackageVersions, cancellationToken),
            ENugetFileType.Csproj or ENugetFileType.Targets => csprojVersionWriter.TrySetVersion(nugetUpdateCandidate, targetFullPath, groupPackageVersions, cancellationToken),
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
            foreach (var dependencySet in packageSearchMetadataRegistration.OriginalReference.DependencySets)
            {
                foreach (var dependencyPackage in dependencySet.Packages)
                {
                    if (groupPackageVersions.TryGetValue(dependencyPackage.Id, out var dependencyCurrentVersion))
                    {
                        if (
                            dependencyPackage.VersionRange.MinVersion is { } dependencyPackageMinVersion
                            && dependencyPackageMinVersion.MapToPackageVersion(dependencyCurrentVersion.PackageSource) is { } dependencyPackageVersion
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

        throw new ArgumentOutOfRangeException(nameof(packageVersion), packageVersion, null);
    }
}
