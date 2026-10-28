using Aviationexam.DependencyUpdater.Common;
using NuGet.Protocol.Core.Types;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public interface INugetReleaseNotesFetcher
{
    Task<IReadOnlyCollection<PackageReleaseNote>> FetchReleaseNotesAsync(
        SourceRepository repository,
        string packageName,
        PackageVersion? fromVersion,
        PackageVersion toVersion,
        SourceCacheContext nugetCache,
        CancellationToken cancellationToken
    );
}
