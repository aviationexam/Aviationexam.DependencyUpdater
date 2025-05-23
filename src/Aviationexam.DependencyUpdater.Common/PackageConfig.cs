using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Common;

/// <summary>
/// Package configuration for NuGet update operations.
/// </summary>
public class PackageConfig
{
    /// <summary>
    /// The packages or patterns to ignore during updates.
    /// </summary>
    public required IReadOnlyCollection<IgnoreEntry> IgnoreEntries { get; init; }

    /// <summary>
    /// The package groups to update.
    /// </summary>
    public required IReadOnlyCollection<GroupEntry> GroupEntries { get; init; }

    /// <summary>
    /// The fallback registries to use when a package is not found in the primary registry.
    /// </summary>
    public required IReadOnlyDictionary<string, string> FallbackRegistries { get; init; }
}
