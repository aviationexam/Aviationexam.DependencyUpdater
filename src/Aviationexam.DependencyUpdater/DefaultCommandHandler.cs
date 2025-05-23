using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.ConfigurationParser;
using Aviationexam.DependencyUpdater.Constants;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater;

internal static class DefaultCommandHandler
{
    public static ICommandHandler GetHandler() => CommandHandler.Create<SourceConfiguration, DependabotConfigurationLoader, IEnvVariableProvider, NugetUpdater, CancellationToken>(
        ExecuteWithBuilderAsync
    );

    private static async Task ExecuteWithBuilderAsync(
        SourceConfiguration sourceConfiguration,
        DependabotConfigurationLoader dependabotConfigurationLoader,
        IEnvVariableProvider envVariableProvider,
        NugetUpdater nugetUpdater,
        CancellationToken cancellationToken
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

                var repositoryConfig = new RepositoryConfig
                {
                    RepositoryPath = sourceConfiguration.Directory,
                    SubdirectoryPath = nugetUpdate.DirectoryValue.GetString(),
                    SourceBranchName = nugetUpdate.TargetBranch.GetString(),
                };

                var gitMetadataConfig = new GitMetadataConfig
                {
                    Milestone = nugetUpdate.Milestone.AsAny.AsString.GetString(),
                    Reviewers = [.. nugetUpdate.Reviewers.Select(x => x.GetString()!)],
                    CommitAuthor = nugetUpdate.CommitAuthor ?? GitAuthorConstants.DefaultCommitAuthor,
                    CommitAuthorEmail = nugetUpdate.CommitAuthorEmail ?? GitAuthorConstants.DefaultCommitAuthorEmail,
                    UpdateSubmodules = [.. nugetUpdate.UpdateSubmodules.Select(x => x.MapToSubmoduleEntry())],
                };

                var packageConfig = new NugetPackageConfig
                {
                    TargetFrameworks = nugetUpdate.TargetFramework.MapToNugetTargetFrameworks(),
                    IgnoreEntries = [.. nugetUpdate.Ignore.MapToIgnoreEntry()],
                    GroupEntries = [.. nugetUpdate.Groups.MapToGroupEntry()],
                    FallbackRegistries = fallbackRegistries,
                };

                var authConfig = new NugetAuthConfig
                {
                    NugetFeedAuthentications =
                    [
                        .. nugetFeedAuthentications.Where(x =>
                            registries.Contains(x.Key)
                            || fallbackRegistries.Any(r => r.Value == x.Key)
                        ),
                    ],
                };

                await nugetUpdater.ProcessUpdatesAsync(
                    repositoryConfig,
                    gitMetadataConfig,
                    packageConfig,
                    authConfig,
                    cancellationToken
                );
            }
        }
    }
}
