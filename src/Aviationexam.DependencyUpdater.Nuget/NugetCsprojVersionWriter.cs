using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class NugetCsprojVersionWriter
{
    public Task<ESetVersion> TrySetVersion<T>(
        NugetUpdateCandidate<T> nugetUpdateCandidate,
        string fullPath,
        IDictionary<string, PackageVersion> groupPackageVersions,
        CancellationToken cancellationToken
    ) => Task.FromResult(ESetVersion.VersionNotSet);
}
