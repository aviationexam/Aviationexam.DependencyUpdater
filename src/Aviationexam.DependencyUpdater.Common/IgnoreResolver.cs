namespace Aviationexam.DependencyUpdater.Common;

public class IgnoreResolver(
    IReadOnlyCollection<IIgnoreRule> Rules
)
{
    public bool IsIgnored(
        string dependencyName,
        PackageVersion packageVersion
    )
    {
        foreach (var ignoreRule in Rules)
        {
            var applyRule = ignoreRule switch
            {
                WildcardIgnoreRule wildcardIgnore => dependencyName.StartsWith(wildcardIgnore.DependencyPrefix, StringComparison.Ordinal),
                ExplicitIgnoreRule explicitIgnore => dependencyName.Equals(explicitIgnore.DependencyName, StringComparison.Ordinal),
                _ => false,
            };
        }

        return false;
    }
}
