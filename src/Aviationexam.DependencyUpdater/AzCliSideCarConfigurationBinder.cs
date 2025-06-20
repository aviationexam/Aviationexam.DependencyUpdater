using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Repository.DevOps;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater;

public sealed class AzCliSideCarConfigurationBinder(
    Option<string> address,
    Option<string> token
) : IBinder<Optional<AzCliSideCarConfiguration>>
{
    public Optional<AzCliSideCarConfiguration> CreateValue(
        ParseResult parseResult
    ) => parseResult.GetValue(address) is { } addressValue
         && parseResult.GetValue(token) is { } tokenValue
        ? new Optional<AzCliSideCarConfiguration>(new AzCliSideCarConfiguration
        {
            Address = addressValue,
            Token = tokenValue,
        })
        : new Optional<AzCliSideCarConfiguration>(Value: null);
}
