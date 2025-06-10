using System;

namespace Aviationexam.DependencyUpdater.Common;

public sealed class CachingConfiguration
{
    public required DateTimeOffset? MaxCacheAge { get; init; }
}
