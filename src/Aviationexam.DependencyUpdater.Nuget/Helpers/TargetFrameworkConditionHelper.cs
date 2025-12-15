using System.Text.RegularExpressions;

namespace Aviationexam.DependencyUpdater.Nuget.Helpers;

public static partial class TargetFrameworkConditionHelper
{
    // Matches Condition="'$(TargetFramework)' == 'net9.0'" or Condition="'$(TargetFramework)'=='net9.0'" (with or without spaces)
    [GeneratedRegex(@"\'\$\(TargetFramework\)\'\s*==\s*\'(?<tfm>[^\']+)\'", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex TargetFrameworkConditionRegex();

    public static string? TryExtractTargetFramework(string? condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return null;
        }

        var match = TargetFrameworkConditionRegex().Match(condition);
        return match.Success ? match.Groups["tfm"].Value : null;
    }
}
