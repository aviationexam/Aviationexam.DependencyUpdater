namespace Aviationexam.DependencyUpdater.Common;

public class FutureVersionResolver
{
    public PackageVersion<TOriginalVersionReference>? ResolveFutureVersion<TOriginalVersionReference>(
        string dependencyName,
        Version? version,
        IEnumerable<PackageVersion<TOriginalVersionReference>> versions,
        IgnoreResolver ignoreResolver
    )
    {
        if (version is not null)
        {
            versions = versions.Where(x => x.Version >= version);
        }

        versions = versions.Where(x => !ignoreResolver.IsIgnored(dependencyName, x));

        return null;
    }
}
