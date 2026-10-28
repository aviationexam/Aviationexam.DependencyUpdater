using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Interfaces.ReleaseNotes;

public interface IProjectReleasesProvider
{
    bool CanProvide(
        Uri repositoryUrl
    );

    Task<IReadOnlyCollection<ProjectRelease>> GetReleasesAsync(
        Uri repositoryUrl,
        CancellationToken cancellationToken
    );
}
