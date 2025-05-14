using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class NugetVersionWriter(
    NugetDirectoryPackagesPropsVersionWriter directoryPackagesPropsVersionWriter,
    NugetCsprojVersionWriter csprojVersionWriter
)
{
    public bool TrySetVersion<T>(
        NugetUpdateCandidate<T> nugetUpdateCandidate,
        ISourceVersioningWorkspace gitWorkspace,
        IDictionary<string, PackageVersion> groupPackageVersions
    )
    {
        if (!IsCompatibleWithCurrentVersions(nugetUpdateCandidate.PackageVersion, groupPackageVersions, out _))
        {
            return false;
        }

        var workspaceDirectory = gitWorkspace.GetWorkspaceDirectory();

        var targetFullPath = nugetUpdateCandidate.NugetDependency.NugetFile.GetFullPath(workspaceDirectory);

        return nugetUpdateCandidate.NugetDependency.NugetFile.Type switch
        {
            ENugetFileType.DirectoryPackagesProps => directoryPackagesPropsVersionWriter.TrySetVersion(nugetUpdateCandidate, targetFullPath, groupPackageVersions),
            ENugetFileType.Csproj or ENugetFileType.Targets => csprojVersionWriter.TrySetVersion(nugetUpdateCandidate, targetFullPath, groupPackageVersions),
            // ENugetFileType.NugetConfig => false,
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
                            && dependencyPackageMinVersion.MapToPackageVersion() is { } dependencyPackageVersion
                            && dependencyPackageVersion < dependencyCurrentVersion
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
