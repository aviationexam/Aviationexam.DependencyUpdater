using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.ConfigurationParser;
using Aviationexam.DependencyUpdater.Constants;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater;

internal static class DefaultCommandHandler
{
    public static ICommandHandler GetHandler() => CommandHandler.Create<SourceConfiguration, DependabotConfigurationLoader, IEnvVariableProvider, NugetUpdater>(
        ExecuteWithBuilderAsync
    );

    private static async Task ExecuteWithBuilderAsync(
        SourceConfiguration sourceConfiguration,
        DependabotConfigurationLoader dependabotConfigurationLoader,
        IEnvVariableProvider envVariableProvider,
        NugetUpdater nugetUpdater
    )
    {
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
