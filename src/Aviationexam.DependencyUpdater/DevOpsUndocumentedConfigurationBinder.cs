using Aviationexam.DependencyUpdater.Repository.DevOps;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater;

public sealed class DevOpsUndocumentedConfigurationBinder(
    Option<string> nugetFeedProject,
    Option<string> nugetFeedId,
    Option<string> serviceHost,
    Option<string> accessTokenResourceId
) : IBinder<DevOpsUndocumentedConfiguration>
{
    public DevOpsUndocumentedConfiguration CreateValue(
        ParseResult parseResult
    ) => new()
    {
        NugetFeedProject = parseResult.GetRequiredValue(nugetFeedProject),
        NugetFeedId = parseResult.GetRequiredValue(nugetFeedId),
        NugetServiceHost = parseResult.GetRequiredValue(serviceHost),
        AccessTokenResourceId = parseResult.GetRequiredValue(accessTokenResourceId),
    };
}
