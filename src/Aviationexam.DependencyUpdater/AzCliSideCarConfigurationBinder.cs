using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Repository.DevOps;
using System.CommandLine;
using System.CommandLine.Binding;

namespace Aviationexam.DependencyUpdater;

public sealed class AzCliSideCarConfigurationBinder(
    Option<string> address,
    Option<string> token
) : BinderBase<Optional<AzCliSideCarConfiguration>>
{
    protected override Optional<AzCliSideCarConfiguration> GetBoundValue(
        BindingContext bindingContext
    ) => bindingContext.ParseResult.GetValueForOption(address) is { } addressValue
         && bindingContext.ParseResult.GetValueForOption(token) is { } tokenValue
        ? new Optional<AzCliSideCarConfiguration>(new AzCliSideCarConfiguration
        {
            Address = addressValue,
            Token = tokenValue,
        })
        : new Optional<AzCliSideCarConfiguration>(Value: null);
}
