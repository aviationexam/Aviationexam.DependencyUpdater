using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

/// <summary>
/// Represents current package versions indexed by package name, condition, and target framework group.
/// Structure: PackageName -> Condition -> TargetFrameworkGroup -> PackageVersion
/// </summary>
public sealed class CurrentPackageVersions
{
    private readonly Dictionary<string, IDictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>> _versions;

    public CurrentPackageVersions()
    {
        _versions = [];
    }

    public CurrentPackageVersions(
        IReadOnlyDictionary<string, IDictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>> versions
    )
    {
        _versions = new Dictionary<string, IDictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>>(versions);
    }

    /// <summary>
    /// Exposes the internal dictionary for direct access by existing code.
    /// </summary>
    public IReadOnlyDictionary<string, IDictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>> Inner => _versions;

    /// <summary>
    /// Exposes a mutable view of the internal dictionary for modification by existing code.
    /// </summary>
    public IDictionary<string, IDictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>> InnerMutable => _versions;

    /// <summary>
    /// Creates a shallow mutable copy for modification during package updates.
    /// </summary>
    public CurrentPackageVersions CreateMutableCopy()
    {
        var copy = new CurrentPackageVersions();

        foreach (var (packageName, conditions) in _versions)
        {
            copy._versions[packageName] = new Dictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>(
                conditions.AsValueEnumerable().ToDictionary(
                    x => x.Key,
                    x => x.Value
                )
            );
        }

        return copy;
    }

    /// <summary>
    /// Tries to get the version for a specific package, condition, and compatible target framework.
    /// </summary>
    public bool TryGetVersion(
        string packageName,
        NugetPackageCondition condition,
        NugetTargetFramework targetFramework,
        [MaybeNullWhen(false)] out PackageVersion version
    )
    {
        version = null;

        if (!_versions.TryGetValue(packageName, out var conditions))
        {
            return false;
        }

        if (!conditions.TryGetValue(condition, out var frameworkVersions))
        {
            return false;
        }

        foreach (var (frameworkGroup, packageVersion) in frameworkVersions)
        {
            if (frameworkGroup.CanBeUsedWith(targetFramework.TargetFramework, out _))
            {
                version = packageVersion;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Tries to get the version for a specific package and condition, returning the first compatible framework version found.
    /// </summary>
    public bool TryGetVersionForAnyFramework(
        string packageName,
        NugetPackageCondition condition,
        [MaybeNullWhen(false)] out PackageVersion version,
        [MaybeNullWhen(false)] out NugetTargetFrameworkGroup frameworkGroup
    )
    {
        version = null;
        frameworkGroup = null;

        if (!_versions.TryGetValue(packageName, out var conditions))
        {
            return false;
        }

        if (!conditions.TryGetValue(condition, out var frameworkVersions))
        {
            return false;
        }

        foreach (var (group, packageVersion) in frameworkVersions)
        {
            version = packageVersion;
            frameworkGroup = group;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the version for a package across all conditions for a compatible target framework.
    /// Used by writers that don't operate on specific conditions.
    /// </summary>
    public bool TryGetVersionAcrossConditions(
        string packageName,
        NugetTargetFramework targetFramework,
        [MaybeNullWhen(false)] out PackageVersion version
    )
    {
        version = null;

        if (!_versions.TryGetValue(packageName, out var conditions))
        {
            return false;
        }

        foreach (var (_, frameworkVersions) in conditions)
        {
            foreach (var (frameworkGroup, packageVersion) in frameworkVersions)
            {
                if (frameworkGroup.CanBeUsedWith(targetFramework.TargetFramework, out _))
                {
                    version = packageVersion;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Sets the version for a specific package, condition, and target framework group.
    /// </summary>
    public void SetVersion(
        string packageName,
        NugetPackageCondition condition,
        NugetTargetFrameworkGroup targetFrameworkGroup,
        PackageVersion version
    )
    {
        if (!_versions.TryGetValue(packageName, out var conditions))
        {
            conditions = new Dictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>();
            _versions[packageName] = conditions;
        }

        if (!conditions.TryGetValue(condition, out var frameworkVersions))
        {
            frameworkVersions = new Dictionary<NugetTargetFrameworkGroup, PackageVersion>();
            conditions[condition] = frameworkVersions;
        }

        frameworkVersions[targetFrameworkGroup] = version;
    }

    /// <summary>
    /// Updates version for a package across all its conditions for specific target frameworks.
    /// Used by writers that update versions without condition context.
    /// </summary>
    public void UpdateVersionForTargetFrameworks(
        string packageName,
        IEnumerable<NugetTargetFramework> targetFrameworks,
        PackageVersion newVersion
    )
    {
        if (!_versions.TryGetValue(packageName, out var conditions))
        {
            return;
        }

        foreach (var (_, frameworkVersions) in conditions)
        {
            foreach (var targetFramework in targetFrameworks)
            {
                foreach (var (frameworkGroup, _) in frameworkVersions)
                {
                    if (frameworkGroup.CanBeUsedWith(targetFramework.TargetFramework, out _))
                    {
                        frameworkVersions[frameworkGroup] = newVersion;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks if a package with the given name exists.
    /// </summary>
    public bool ContainsPackage(string packageName) => _versions.ContainsKey(packageName);

    /// <summary>
    /// Checks if the package is at the expected version for the specified condition and target framework.
    /// </summary>
    public bool IsAtVersion(
        string packageName,
        NugetPackageCondition condition,
        NugetTargetFramework targetFramework,
        PackageVersion expectedVersion
    ) => TryGetVersion(packageName, condition, targetFramework, out var currentVersion)
         && currentVersion == expectedVersion;

    /// <summary>
    /// Gets all conditions for a specific package.
    /// </summary>
    public bool TryGetConditions(
        string packageName,
        [MaybeNullWhen(false)] out IDictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>> conditions
    ) => _versions.TryGetValue(packageName, out conditions);

    /// <summary>
    /// Checks version compatibility for a dependency across all conditions.
    /// Returns false if any condition has a version conflict.
    /// </summary>
    public bool IsVersionCompatibleAcrossConditions(
        string packageName,
        PackageVersion requiredMinVersion,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks,
        [MaybeNullWhen(true)] out PackageVersion? conflictingVersion
    )
    {
        conflictingVersion = null;

        if (!_versions.TryGetValue(packageName, out var conditions))
        {
            return true;
        }

        foreach (var (_, frameworkVersions) in conditions)
        {
            foreach (var targetFramework in targetFrameworks)
            {
                foreach (var (frameworkGroup, currentVersion) in frameworkVersions)
                {
                    if (frameworkGroup.CanBeUsedWith(targetFramework.TargetFramework, out _))
                    {
                        if (requiredMinVersion > currentVersion)
                        {
                            conflictingVersion = currentVersion;
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Merges another collection of versions into this one, grouping by package name and condition.
    /// </summary>
    public void Merge(
        IEnumerable<KeyValuePair<string, IDictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>>> other
    )
    {
        foreach (var (packageName, conditions) in other)
        {
            if (!_versions.TryGetValue(packageName, out var existingConditions))
            {
                existingConditions = new Dictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>();
                _versions[packageName] = existingConditions;
            }

            foreach (var (condition, frameworkVersions) in conditions)
            {
                if (!existingConditions.TryGetValue(condition, out var existingFrameworkVersions))
                {
                    existingFrameworkVersions = new Dictionary<NugetTargetFrameworkGroup, PackageVersion>();
                    existingConditions[condition] = existingFrameworkVersions;
                }

                foreach (var (frameworkGroup, version) in frameworkVersions)
                {
                    existingFrameworkVersions[frameworkGroup] = version;
                }
            }
        }
    }

    /// <summary>
    /// Adds a single package version entry.
    /// </summary>
    public void Add(
        string packageName,
        NugetPackageCondition condition,
        NugetTargetFrameworkGroup targetFrameworkGroup,
        PackageVersion version
    )
    {
        SetVersion(packageName, condition, targetFrameworkGroup, version);
    }

    /// <summary>
    /// Creates from a concurrent bag of key-value pairs, grouping and deduplicating as needed.
    /// </summary>
    public static CurrentPackageVersions FromConcurrentBag(
        IEnumerable<KeyValuePair<string, IDictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>>> items
    )
    {
        var result = new CurrentPackageVersions();
        result.Merge(items);
        return result;
    }
}
