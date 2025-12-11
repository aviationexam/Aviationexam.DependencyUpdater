using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater.Commands;

public sealed class GitHubCommand() : Command(
    nameof(EPlatformSelection.GitHub), "Updates dependencies in GitHub repositories."
)
{
    public IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ParseResult parseResult
    ) => serviceCollection;
}
