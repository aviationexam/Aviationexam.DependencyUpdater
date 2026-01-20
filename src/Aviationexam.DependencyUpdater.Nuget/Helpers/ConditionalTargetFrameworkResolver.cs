using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Helpers;

public partial class ConditionalTargetFrameworkResolver(
    ILogger<ConditionalTargetFrameworkResolver> logger
)
{
    private const string TargetFrameworkAllowListKeyword = "TargetFramework";

    [GeneratedRegex(@"\'?\$\(TargetFramework\)\'?\s*==\s*\'(?<tfm>[^\']+)\'", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex TargetFrameworkConditionRegex();

    public string? Resolve(IReadOnlyList<string> conditions, string? packageName)
    {
        var filteredConditions = FilterConditionsByAllowList(conditions, packageName);

        if (filteredConditions.Count == 0)
        {
            return null;
        }

        var resolvedConditions = new List<(string Condition, string TargetFramework)>();
        foreach (var condition in filteredConditions)
        {
            if (TryExtractTargetFramework(condition, out var targetFramework))
            {
                resolvedConditions.Add((condition, targetFramework));
            }
            else if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    "Unable to parse condition '{Condition}' for package {PackageName}",
                    condition,
                    packageName
                );
            }
        }

        if (resolvedConditions.Count == 0)
        {
            return null;
        }

        if (resolvedConditions.Count > 1)
        {
            var conditionDescriptions = resolvedConditions
                .AsValueEnumerable()
                .Select(c => $"'{c.Condition}' -> '{c.TargetFramework}'")
                .ToArray();

            throw new InvalidOperationException(
                $"Multiple target framework conditions found for package '{packageName}': " +
                $"[{string.Join(", ", conditionDescriptions)}]. " +
                "Expected at most one condition after filtering."
            );
        }

        var (resolvedCondition, conditionalTargetFramework) = resolvedConditions[0];

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Found conditional target framework {TargetFramework} for package {PackageName} from condition '{Condition}'",
                conditionalTargetFramework,
                packageName,
                resolvedCondition
            );
        }

        return conditionalTargetFramework;
    }

    private List<string> FilterConditionsByAllowList(IReadOnlyList<string> conditions, string? packageName)
    {
        var filtered = new List<string>();

        foreach (var condition in conditions)
        {
            if (condition.Contains(TargetFrameworkAllowListKeyword, StringComparison.OrdinalIgnoreCase))
            {
                filtered.Add(condition);
            }
            else if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace(
                    "Condition '{Condition}' for package {PackageName} filtered out (does not contain '{AllowListKeyword}')",
                    condition,
                    packageName,
                    TargetFrameworkAllowListKeyword
                );
            }
        }

        return filtered;
    }

    public static bool TryExtractTargetFramework(string? condition, [NotNullWhen(true)] out string? targetFramework)
    {
        targetFramework = null;

        if (string.IsNullOrWhiteSpace(condition))
        {
            return false;
        }

        var match = TargetFrameworkConditionRegex().Match(condition);
        if (match.Success)
        {
            targetFramework = match.Groups["tfm"].Value;
            return true;
        }

        return false;
    }
}
