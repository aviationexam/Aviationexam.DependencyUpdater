using Aviationexam.DependencyUpdater.Interfaces.ReleaseNotes;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

/// <summary>
/// Maps project releases (GitHub releases, tags) to package versions the same way dependabot does:
/// releases named after the package ("Package.Name v1.2.3", "Package.Name/1.2.3") take precedence,
/// otherwise the release name or tag is read as a plain version ("v1.2.3", "release-1.2.3").
/// </summary>
public static partial class ProjectReleaseVersionMatcher
{
    [GeneratedRegex(@"^[^0-9]*", RegexOptions.None, matchTimeoutMilliseconds: 100)]
    private static partial Regex NamePrefixRegexFactory();

    private static readonly Regex NamePrefix = NamePrefixRegexFactory();

    [GeneratedRegex(@"(?:^|[\s/_-])(?<dependency>[A-Za-z0-9_.-]+)[\s/_-]+v?(?<version>[0-9][A-Za-z0-9.+-]*)$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex PackageScopedNameRegexFactory();

    private static readonly Regex PackageScopedName = PackageScopedNameRegexFactory();

    public static IReadOnlyDictionary<NuGetVersion, ProjectRelease> MatchReleases(
        IEnumerable<ProjectRelease> releases,
        string packageName
    )
    {
        var repoScoped = new Dictionary<NuGetVersion, ProjectRelease>();
        var packageScoped = new Dictionary<NuGetVersion, ProjectRelease>();
        var otherPackageScoped = new Dictionary<NuGetVersion, ProjectRelease>();

        foreach (var release in releases)
        {
            if (GetPackageScopedVersion(release, packageName) is { } packageScopedVersion)
            {
                packageScoped.TryAdd(packageScopedVersion, release);
            }
            else if (GetRepoScopedVersion(release) is { } version)
            {
                if (HasPackageScopedVersionForOtherPackage(release, packageName))
                {
                    otherPackageScoped.TryAdd(version, release);
                }
                else
                {
                    repoScoped.TryAdd(version, release);
                }
            }
        }

        // Releases of a monorepo scoped to other packages are used only when this package has no releases of its own
        foreach (var (version, release) in packageScoped.Count > 0 ? packageScoped : otherPackageScoped)
        {
            repoScoped[version] = release;
        }

        return repoScoped;
    }

    private static NuGetVersion? GetPackageScopedVersion(
        ProjectRelease release,
        string packageName
    )
    {
        foreach (var candidate in new[] { release.Name, release.TagName })
        {
            if (candidate is null)
            {
                continue;
            }

            var match = PackageScopedName.Match(candidate.Trim());
            if (
                match.Success
                && match.Groups["dependency"].Value.Equals(packageName, StringComparison.OrdinalIgnoreCase)
                && NuGetVersion.TryParse(match.Groups["version"].Value, out var version)
            )
            {
                return version;
            }
        }

        return null;
    }

    private static bool HasPackageScopedVersionForOtherPackage(
        ProjectRelease release,
        string packageName
    )
    {
        foreach (var candidate in new[] { release.Name, release.TagName })
        {
            if (candidate is null)
            {
                continue;
            }

            var match = PackageScopedName.Match(candidate.Trim());
            if (
                match.Success
                && match.Groups["dependency"].Value.Contains('.')
                && !match.Groups["dependency"].Value.Equals(packageName, StringComparison.OrdinalIgnoreCase)
                && NuGetVersion.TryParse(match.Groups["version"].Value, out _)
            )
            {
                return true;
            }
        }

        return false;
    }

    private static NuGetVersion? GetRepoScopedVersion(
        ProjectRelease release
    )
    {
        foreach (var candidate in new[] { release.Name, release.TagName })
        {
            if (candidate is null)
            {
                continue;
            }

            if (NuGetVersion.TryParse(NamePrefix.Replace(candidate.Trim(), string.Empty), out var version))
            {
                return version;
            }
        }

        return null;
    }
}
