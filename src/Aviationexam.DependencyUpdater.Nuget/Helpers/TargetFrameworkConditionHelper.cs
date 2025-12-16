using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Aviationexam.DependencyUpdater.Nuget.Helpers;

public static partial class TargetFrameworkConditionHelper
{
    // Matches Condition="'$(TargetFramework)' == 'net9.0'" or Condition="'$(TargetFramework)'=='net9.0'" (with or without spaces)
    [GeneratedRegex(@"\'\$\(TargetFramework\)\'\s*==\s*\'(?<tfm>[^\']+)\'", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex TargetFrameworkConditionRegex();

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
