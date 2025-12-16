using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.ConfigurationParser;
using Aviationexam.DependencyUpdater.Constants;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget;
using Aviationexam.DependencyUpdater.Nuget.Configurations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;

namespace Aviationexam.DependencyUpdater;

internal sealed class DefaultCommandHandler(
    SourceConfiguration sourceConfiguration,
    GitCredentialsConfiguration gitCredentialsConfiguration,
    DependabotConfigurationLoader dependabotConfigurationLoader,
    IEnvVariableProvider envVariableProvider,
    NugetUpdater nugetUpdater,
    CachingConfiguration cachingConfiguration
) : ICommandHandler
{
    public async Task<int> ExecuteAsync(
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
                var registries = nugetUpdate.Registries is { ValueKind: JsonValueKind.Object } r
                    ? r.AsValueEnumerable().Select(x => x.AsString.GetString()!).ToList()
                    : [];
                var fallbackRegistries = nugetUpdate.FallbackRegistries;

                var repositoryConfig = new RepositoryConfig
                {
                    RepositoryPath = sourceConfiguration.Directory,
                    SubdirectoryPath = nugetUpdate.DirectoryValue.GetString(),
                    SourceBranchName = nugetUpdate.TargetBranch.GetString(),
                };

                var gitMetadataConfig = new GitMetadataConfig
                {
                    Milestone = nugetUpdate.Milestone.ValueKind is JsonValueKind.String or JsonValueKind.Number ? nugetUpdate.Milestone.AsAny.AsString.GetString() : null,
                    Reviewers = nugetUpdate.Reviewers ?? [],
                    Labels = [.. nugetUpdate.Labels.AsValueEnumerable().Select(x => x.GetString()!)],
                    CommitAuthor = nugetUpdate.CommitAuthor ?? GitAuthorConstants.DefaultCommitAuthor,
                    CommitAuthorEmail = nugetUpdate.CommitAuthorEmail ?? GitAuthorConstants.DefaultCommitAuthorEmail,
                    UpdateSubmodules = [.. nugetUpdate.UpdateSubmodules.AsValueEnumerable().Select(x => x.MapToSubmoduleEntry())],
                };

                var packageConfig = new NugetPackageConfig
                {
                    TargetFrameworks = nugetUpdate.TargetFramework.MapToNugetTargetFrameworks(),
                    IgnoreEntries = [.. nugetUpdate.Ignore.MapToIgnoreEntry()],
                    GroupEntries = [.. nugetUpdate.Groups.MapToGroupEntry()],
                    FallbackRegistries = fallbackRegistries,
                    ExecuteRestore = nugetUpdate.ExecuteRestore,
                    RestoreDirectory = nugetUpdate.RestoreDirectory ?? nugetUpdate.DirectoryValue.GetString(),
                };

                var authConfig = new NugetAuthConfig
                {
                    NugetFeedAuthentications =
                    [
                        .. nugetFeedAuthentications.AsValueEnumerable().Where(x =>
                            registries.Contains(x.Key)
                            || fallbackRegistries.AsValueEnumerable().Any(r => r.Value == x.Key)
                        ),
                    ],
                };

                await nugetUpdater.ProcessUpdatesAsync(
                    repositoryConfig,
                    gitCredentialsConfiguration,
                    gitMetadataConfig,
                    packageConfig,
                    authConfig,
                    cachingConfiguration,
                    cancellationToken
                );
            }
        }

        return 0;
    }

    public static Func<ParseResult, CancellationToken, Task<int>> GetHandler(
        Action<IServiceCollection, ParseResult> addConfigurations
    ) => (parseResult, cancellationToken) => parseResult.ExecuteCommandHandlerAsync<DefaultCommandHandler>(
        HostBuilderFactory.Create,
        addConfigurations,
        cancellationToken
    );
}
