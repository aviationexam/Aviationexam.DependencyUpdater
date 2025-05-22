using Aviationexam.DependencyUpdater.Repository.DevOps;
using System.CommandLine;
using System.CommandLine.Binding;

namespace Aviationexam.DependencyUpdater;

public sealed class DevOpsUndocumentedConfigurationBinder(
    Option<string> nugetFeedId,
    Option<string> serviceHost,
    Option<string> accessTokenResourceId
) : BinderBase<DevOpsUndocumentedConfiguration>
{
    protected override DevOpsUndocumentedConfiguration GetBoundValue(
        BindingContext bindingContext
    ) => new()
    {
        NugetFeedId = bindingContext.ParseResult.GetValueForOption(nugetFeedId)!,
        NugetServiceHost = bindingContext.ParseResult.GetValueForOption(serviceHost)!,
        AccessTokenResourceId = bindingContext.ParseResult.GetValueForOption(accessTokenResourceId)!,
    };
}
