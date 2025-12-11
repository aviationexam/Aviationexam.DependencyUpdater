using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater.Commands;

public sealed class GitHubCommand : Command
{
    public GitHubCommand()
        : base(nameof(EPlatformSelection.GitHub), "Updates dependencies in GitHub repositories.")
    {
        // GitHub-specific options will be added here
    }

    public void ConfigureServices(
        IServiceCollection serviceCollection,
        ParseResult parseResult,
        Option<string> directory,
        Option<string> gitUsername,
        Option<string> gitPassword,
        Option<bool> resetCache
    )
    {
        serviceCollection
            .BindCommonConfiguration(parseResult, directory, gitUsername, gitPassword, resetCache);

        // GitHub-specific configuration will be added here
        throw new NotImplementedException("GitHub support is not yet implemented");
    }
}
