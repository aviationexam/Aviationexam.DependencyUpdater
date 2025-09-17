using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.ConfigurationParser;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Configurations;
using Aviationexam.DependencyUpdater.Nuget.Factories;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using ZLinq;

namespace Aviationexam.DependencyUpdater;

public static class ConfigurationMapper
{
    public static NugetFeedAuthentication MapToNugetFeedAuthentication(
        this DependabotConfiguration.Registry.Entity registry,
        string key,
        IEnvVariableProvider envVariableProvider
    ) => NugetFeedAuthenticationFactory.CreateNugetFeedAuthentication(
        envVariableProvider,
        key,
        registry.Url.GetString()!,
        registry.Username.GetString(),
        registry.Password.GetString(),
        registry.Token.GetString(),
        registry.NugetFeedVersion.GetString()
    );

    public static IReadOnlyCollection<NugetTargetFramework> MapToNugetTargetFrameworks(
        this TargetFrameworkEntity? targetFrameworkEntity
    ) => targetFrameworkEntity is { } targetFramework ? [new NugetTargetFramework(targetFramework.TargetFramework)] : [];

    public static IEnumerable<IgnoreEntry> MapToIgnoreEntry(
        this DependabotConfiguration.Update.IgnoreArray ignoreArray
    )
    {
        foreach (var ignoreEntity in ignoreArray)
        {
            yield return new IgnoreEntry(
                ignoreEntity.DependencyName.GetString(),
                [.. ignoreEntity.UpdateTypesValue.Select(x => x.GetString()!)]
            );
        }
    }

    public static IEnumerable<GroupEntry> MapToGroupEntry(
        this DependabotConfiguration.Update.GroupsEntity groupsEntity
    )
    {
        foreach (var groupEntity in groupsEntity)
        {
            yield return new GroupEntry(
                groupEntity.Key.GetString(),
                [.. groupEntity.Value.Patterns.Select(x => x.GetString()!)]
            );
        }
    }

    public static SubmoduleEntry MapToSubmoduleEntry(
        this SubmoduleEntity submoduleEntity
    ) => new(
        submoduleEntity.Path,
        submoduleEntity.Branch
    );
}
