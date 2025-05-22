using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.ConfigurationParser;
using Aviationexam.DependencyUpdater.Constants;
using Aviationexam.DependencyUpdater.DefaultImplementations;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget;
using Aviationexam.DependencyUpdater.Repository.DevOps;
using Aviationexam.DependencyUpdater.Vcs.Git;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater;

internal sealed class DefaultCommandHandler
{
    public static IHost CreateHostBuilder(string[] args, string directory)
    {
        HostApplicationBuilderSettings settings = new()
        {
            Args = args,
            Configuration = new ConfigurationManager(),
            ContentRootPath = directory,
        };

        var builder = Host.CreateEmptyApplicationBuilder(settings);

        builder.Configuration
            .AddEnvironmentVariables("DEPENDENCY_UPDATER_")
            .AddCommandLine(args);

        builder.Services.AddLogging(x => x.AddConsole());
        builder.Services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
        builder.Services.AddCommon(builder.Configuration);
        builder.Services.AddConfigurationParser();
        builder.Services.AddNuget();
        builder.Services.AddVcsGit();
        builder.Services.AddRepositoryDevOps(builder.Configuration);
        builder.Services.AddDefaultImplementations();

        return builder.Build();
    }

    public static async Task ExecuteWithBuilderAsync(IServiceProvider serviceProvider)
    {
        var sourceConfiguration = serviceProvider.GetRequiredService<IOptions<SourceConfiguration>>().Value;
        var dependabotConfigurationLoader = serviceProvider.GetRequiredService<DependabotConfigurationLoader>();
        var envVariableProvider = serviceProvider.GetRequiredService<IEnvVariableProvider>();
        var nugetUpdater = serviceProvider.GetRequiredService<NugetUpdater>();

        var dependabotConfigurations = dependabotConfigurationLoader.LoadConfiguration(sourceConfiguration.Directory);

        foreach (var dependabotConfiguration in dependabotConfigurations)
        {
            var nugetUpdates = dependabotConfiguration.ExtractEcosystemUpdates("nuget");

            if (nugetUpdates.Count == 0)
            {
                continue;
            }

            var nugetFeedAuthentications = dependabotConfiguration.ExtractFeeds(
                "nuget-feed",
                (key, value) => value.MapToNugetFeedAuthentication(key, envVariableProvider)
            );

            foreach (var nugetUpdate in nugetUpdates)
            {
                var registries = nugetUpdate.Registries.Select(x => x.AsString.GetString()!).ToList();
                var fallbackRegistries = nugetUpdate.FallbackRegistries;

                await nugetUpdater.ProcessUpdatesAsync(
                    repositoryPath: sourceConfiguration.Directory,
                    subdirectoryPath: nugetUpdate.DirectoryValue.GetString(),
                    sourceBranchName: nugetUpdate.TargetBranch.GetString(),
                    milestone: nugetUpdate.Milestone.AsAny.AsString.GetString(),
                    reviewers: [.. nugetUpdate.Reviewers.Select(x => x.GetString()!)],
                    nugetUpdate.CommitAuthor ?? GitAuthorConstants.DefaultCommitAuthor,
                    nugetUpdate.CommitAuthorEmail ?? GitAuthorConstants.DefaultCommitAuthorEmail,
                    [
                        .. nugetFeedAuthentications.Where(x =>
                            registries.Contains(x.Key)
                            || fallbackRegistries.Any(r => r.Value == x.Key)
                        ),
                    ],
                    fallbackRegistries,
                    nugetUpdate.TargetFramework.MapToNugetTargetFrameworks(),
                    [.. nugetUpdate.Ignore.MapToIgnoreEntry()],
                    [.. nugetUpdate.Groups.MapToGroupEntry()]
                );
            }
        }
    }
}
