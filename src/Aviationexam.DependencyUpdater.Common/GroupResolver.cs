namespace Aviationexam.DependencyUpdater.Common;

public class GroupResolver(
    IReadOnlyCollection<IGroupRule> groupRules
)
{
    public GroupEntry? ResolveGroup(string dependencyName)
    {
        foreach (var groupRule in groupRules)
        {
            var applyGroup = groupRule switch
            {
                WildcardGroupRule wildcardIgnore => dependencyName.StartsWith(wildcardIgnore.DependencyPrefix, StringComparison.Ordinal),
                ExplicitGroupRule explicitIgnore => dependencyName.Equals(explicitIgnore.DependencyName, StringComparison.Ordinal),
                _ => false,
            };

            if (applyGroup)
            {
                return groupRule.GroupEntry;
            }
        }

        return null;
    }
}

