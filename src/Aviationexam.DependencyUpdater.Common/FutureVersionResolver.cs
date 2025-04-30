namespace Aviationexam.DependencyUpdater.Common;

public class FutureVersionResolver
{
    public PackageVersion<TOriginalVersionReference>? ResolveFutureVersion<TOriginalVersionReference>(
        string dependencyName,
        Version? version,
        IEnumerable<PackageVersion<TOriginalVersionReference>> versions,
        IReadOnlyCollection<IgnoreEntry> ignoreEntries
    )
    {
        if (version is not null)
        {
            versions = versions.Where(x => x.Version >= version);
        }

        return null;
    }
}
