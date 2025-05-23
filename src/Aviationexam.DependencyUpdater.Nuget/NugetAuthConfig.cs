using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget;

/// <summary>
/// Authentication configuration for NuGet update operations.
/// </summary>
public sealed class NugetAuthConfig
{
    /// <summary>
    /// The NuGet feed authentications to use.
    /// </summary>
    public required IReadOnlyCollection<NugetFeedAuthentication> NugetFeedAuthentications { get; init; }
}
