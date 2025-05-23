using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Common;

public class IgnoreResolverFactory(
    ILogger<IgnoreResolverFactory> logger
)
{
    public IgnoreResolver Create(IReadOnlyCollection<IgnoreEntry> ignoreEntries)
    {
        var ignoreRules = ignoreEntries
            .Where(x => x.DependencyName is not null)
            .Select<IgnoreEntry, IIgnoreRule>(x =>
            {
                if (x.DependencyName!.EndsWith('*'))
                {
                    return new WildcardIgnoreRule(
                        x.DependencyName.TrimEnd('*'),
                        x.UpdateTypes
                    );
                }

                return new ExplicitIgnoreRule(
                    x.DependencyName,
                    x.UpdateTypes
                );
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
            [.. ignoreRules],
            logger
        );
    }
}
