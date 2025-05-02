using Microsoft.Extensions.Logging;

namespace Aviationexam.DependencyUpdater.Common;

public class IgnoreResolver(
    IReadOnlyCollection<IIgnoreRule> rules,
    ILogger logger
)
{
    public bool IsIgnored(
        string dependencyName,
        Version currentVersion,
        PackageVersion proposedVersion
    )
    {
        foreach (var ignoreRule in rules)
        {
            var applyRule = ignoreRule switch
            {
                WildcardIgnoreRule wildcardIgnore => dependencyName.StartsWith(wildcardIgnore.DependencyPrefix, StringComparison.Ordinal),
                ExplicitIgnoreRule explicitIgnore => dependencyName.Equals(explicitIgnore.DependencyName, StringComparison.Ordinal),
                _ => false,
            };

            if (applyRule)
            {
                foreach (var updateType in ignoreRule.UpdateTypes)
                {
                    bool? isIgnored = updateType switch
                    {
                        "version-update:semver-major" => DoesItViolate(currentVersion, proposedVersion, EIgnoreRule.SemverMajor),
                        "version-update:semver-minor" => DoesItViolate(currentVersion, proposedVersion, EIgnoreRule.SemverMinor),
                        "version-update:semver-patch" => DoesItViolate(currentVersion, proposedVersion, EIgnoreRule.SemverPatch),
                        _ => null,
                    };

                    if (isIgnored is null)
                    {
                        logger.LogError("The {DependencyName}:{PackageVersion}, unknown ignoreRule {IgnoreRule}", dependencyName, proposedVersion, updateType);
                    }
                    else if (isIgnored is true)
                    {
                        logger.LogDebug("The {DependencyName}:{PackageVersion} is ignored", dependencyName, proposedVersion);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool DoesItViolate(
        Version currentVersion,
        PackageVersion proposedVersion,
        EIgnoreRule ignoreRule
    )
    {
        return ignoreRule switch
        {
            EIgnoreRule.SemverMajor => proposedVersion.Version.Major != currentVersion.Major,
            EIgnoreRule.SemverMinor => proposedVersion.Version.Major != currentVersion.Major
                                       || proposedVersion.Version.Minor != currentVersion.Minor,
            EIgnoreRule.SemverPatch => proposedVersion.Version.Major != currentVersion.Major
                                       || proposedVersion.Version.Minor != currentVersion.Minor
                                       || proposedVersion.Version.Build != currentVersion.Build,
            _ => throw new ArgumentOutOfRangeException(nameof(ignoreRule), ignoreRule, null),
        };
    }

    private enum EIgnoreRule
    {
        SemverMinor,
        SemverPatch,
        SemverMajor,
    }
}
