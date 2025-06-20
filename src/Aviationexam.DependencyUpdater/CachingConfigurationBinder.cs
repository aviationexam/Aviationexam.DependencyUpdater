using Aviationexam.DependencyUpdater.Common;
using System;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater;

public sealed class CachingConfigurationBinder(
    TimeProvider timeProvider,
    Option<bool> resetCache
) : IBinder<CachingConfiguration>
{
    public CachingConfiguration CreateValue(
        ParseResult parseResult
    ) => new()
    {
        MaxCacheAge = parseResult.GetValue(resetCache)
            ? timeProvider.GetUtcNow()
            : null,
    };
}
