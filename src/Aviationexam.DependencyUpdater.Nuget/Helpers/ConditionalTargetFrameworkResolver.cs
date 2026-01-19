using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Aviationexam.DependencyUpdater.Nuget.Helpers;

public partial class ConditionalTargetFrameworkResolver(
    ILogger logger
)
{
    [GeneratedRegex(@"\'?\$\(TargetFramework\)\'?\s*==\s*\'(?<tfm>[^\']+)\'", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex TargetFrameworkConditionRegex();

    public string? Resolve(string? condition, string? packageName)
    {
        if (TryExtractTargetFramework(condition, out var conditionalTargetFramework))
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    "Found conditional target framework {TargetFramework} for package {PackageName}",
                    conditionalTargetFramework,
                    packageName
                );
            }

            return conditionalTargetFramework;
        }

        if (!string.IsNullOrWhiteSpace(condition) && logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning(
                "Unable to parse condition '{Condition}' for package {PackageName}",
                condition,
                packageName
            );
        }

        return null;
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
