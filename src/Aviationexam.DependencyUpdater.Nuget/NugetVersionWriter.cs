using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class NugetVersionWriter
{
    public bool TrySetVersion<T>(
        NugetUpdateCandidate<T> nugetUpdateCandidate,
        ISourceVersioningWorkspace gitWorkspace,
        IDictionary<string, PackageVersion> groupPackageVersions
    )
    {
        var workspaceDirectory = gitWorkspace.GetWorkspaceDirectory();

        var targetFullPath = nugetUpdateCandidate.NugetDependency.NugetFile.GetFullPath(workspaceDirectory);

        return true;
    }
}
