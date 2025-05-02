namespace Aviationexam.DependencyUpdater.Common;

public class FutureVersionResolver
{
    public IEnumerable<PackageVersion<TOriginalVersionReference>> ResolveFutureVersion<TOriginalVersionReference>(
        string dependencyName,
        PackageVersion? version,
        IEnumerable<PackageVersion<TOriginalVersionReference>> versions,
        IgnoreResolver ignoreResolver
    )
    {
        if (version is not null)
        {
            versions = versions
                .Where(x => x.IsPrerelease == version.IsPrerelease || x.IsPrerelease is false)
                .Where(x => x > version)
                .Where(x => !ignoreResolver.IsIgnored(
                    dependencyName,
                    version,
                    x
                ));
        }

        return versions.OrderByDescending(x => x.Version);
    }
}
