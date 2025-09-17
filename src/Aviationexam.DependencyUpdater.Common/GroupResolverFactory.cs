using System;
using System.Collections.Generic;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Common;

public class GroupResolverFactory
{
    public GroupResolver Create(IReadOnlyCollection<GroupEntry> groupEntries)
    {
        var groupRules = groupEntries
            .AsValueEnumerable()
            .SelectMany(x => x.Patterns.AsValueEnumerable().Select(IGroupRule (p) =>
            {
                if (p.EndsWith('*'))
                {
                    return new WildcardGroupRule(
                        p.TrimEnd('*'),
                        x
                    );
                }

                return new ExplicitGroupRule(
                    p,
                    x
                );
            }))
            .OrderBy(x => x switch
            {
                ExplicitGroupRule => 0,
                WildcardGroupRule => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(x), x, null),
            })
            .ThenByDescending(x => x switch
            {
                ExplicitGroupRule explicitIgnore => explicitIgnore.DependencyName.Length,
                WildcardGroupRule wildcardIgnore => wildcardIgnore.DependencyPrefix.Length,
                _ => throw new ArgumentOutOfRangeException(nameof(x), x, null),
            });

        return new GroupResolver(
            [.. groupRules]
        );
    }
}
