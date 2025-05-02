namespace Aviationexam.DependencyUpdater.Common;

public class IgnoreResolverFactory
{
    public IgnoreResolver Create(IReadOnlyCollection<IgnoreEntry> ignoreEntries)
    {
        var ignoreRules = ignoreEntries
            .Where(x => x.DependencyName is not null)
            .Select<IgnoreEntry, IIgnoreRule>(x =>
            {
                if (x.DependencyName!.EndsWith('*'))
                {
                    return new WildcardIgnoreRule(x.DependencyName.TrimEnd('*'));
                }

                return new ExplicitIgnoreRule(x.DependencyName);
            })
            .OrderBy(x => x switch
            {
                ExplicitIgnoreRule => 0,
                WildcardIgnoreRule => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(x), x, null),
            })
            .ThenByDescending(x => x switch
            {
                ExplicitIgnoreRule explicitIgnore => explicitIgnore.DependencyName.Length,
                WildcardIgnoreRule wildcardIgnore => wildcardIgnore.DependencyPrefix.Length,
                _ => throw new ArgumentOutOfRangeException(nameof(x), x, null),
            });

        return new IgnoreResolver(
            [.. ignoreRules]
        );
    }
}
