using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget;

public static class NugetDependencyExtensions
{
    public static bool TrySetVersion<T>(
        this NugetUpdateCandidate<T> nugetUpdateCandidate,
        ISourceVersioningWorkspace gitWorkspace,
        IDictionary<string, PackageVersion> groupPackageVersions
    )
    {
        var workspaceDirectory = gitWorkspace.GetWorkspaceDirectory();

        var targetFullPath = nugetUpdateCandidate.NugetDependency.NugetFile.GetFullPath(workspaceDirectory);

        return true;
    }
}
