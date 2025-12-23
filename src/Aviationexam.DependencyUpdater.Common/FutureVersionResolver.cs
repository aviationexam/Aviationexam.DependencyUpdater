using System.Collections.Generic;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Common;

public class FutureVersionResolver
{
    public IEnumerable<PackageVersionWithDependencySets> ResolveFutureVersion(
        string dependencyName,
        PackageVersion? version,
        IEnumerable<PackageVersionWithDependencySets> versions,
        IgnoreResolver ignoreResolver
    )
    {
        if (version is not null)
        {
            return versions
                .AsValueEnumerable()
                .Where(x => x.IsPrerelease == version.IsPrerelease || x.IsPrerelease is false)
                .Where(x => x > version)
                .Where(x => !ignoreResolver.IsIgnored(
                    dependencyName,
                    version,
                    x
                ))
                .OrderDescending()
                .ToList();
        }

        return versions
            .AsValueEnumerable()
            .OrderDescending()
            .ToList();
    }
}
