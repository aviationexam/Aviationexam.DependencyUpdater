using Aviationexam.DependencyUpdater.Repository.DevOps;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater;

public sealed class DevOpsConfigurationBinder(
    Option<string> organization,
    Option<string> project,
    Option<string> repository,
    Option<string> pat,
    Option<string> accountId
) : IBinder<DevOpsConfiguration>
{
    public DevOpsConfiguration CreateValue(
        ParseResult parseResult
    ) => new()
    {
        Organization = parseResult.GetRequiredValue(organization),
        Project = parseResult.GetRequiredValue(project),
        Repository = parseResult.GetRequiredValue(repository),
        PersonalAccessToken = parseResult.GetRequiredValue(pat),
        AccountId = parseResult.GetRequiredValue(accountId),
    };
}
