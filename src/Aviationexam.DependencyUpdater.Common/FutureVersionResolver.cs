namespace Aviationexam.DependencyUpdater.Common;

public class FutureVersionResolver
{
    public PackageVersion<TOriginalVersionReference>? ResolveFutureVersion<TOriginalVersionReference>(
        string dependencyName,
        PackageVersion? version,
        IEnumerable<PackageVersion<TOriginalVersionReference>> versions,
        IgnoreResolver ignoreResolver
    )
    {
        if (version is not null)
        {
            versions = versions
                .Where(x => x.Version >= version)
                .Where(x => !ignoreResolver.IsIgnored(
                    dependencyName,
                    version,
                    x
                ));
        }

        return null;
    }
}
