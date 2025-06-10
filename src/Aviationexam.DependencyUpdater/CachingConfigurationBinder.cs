using Aviationexam.DependencyUpdater.Common;
using System;
using System.CommandLine;
using System.CommandLine.Binding;

namespace Aviationexam.DependencyUpdater;

public sealed class CachingConfigurationBinder(
    TimeProvider timeProvider,
    Option<bool> resetCache
) : BinderBase<CachingConfiguration>
{
    protected override CachingConfiguration GetBoundValue(
        BindingContext bindingContext
    ) => new()
    {
        MaxCacheAge = bindingContext.ParseResult.GetValueForOption(resetCache)
            ? timeProvider.GetUtcNow()
            : null,
    };
}
