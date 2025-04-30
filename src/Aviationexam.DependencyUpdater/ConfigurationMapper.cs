using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.ConfigurationParser;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater;

public static class ConfigurationMapper
{
    public static NugetFeedAuthentication MapToNugetFeedAuthentication(
        this DependabotConfiguration.Registry.Entity registry,
        IEnvVariableProvider envVariableProvider
    ) => NugetFeedAuthenticationFactory.CreateNugetFeedAuthentication(
        envVariableProvider,
        registry.Url.GetString()!,
        registry.Username.GetString(),
        registry.Password.GetString(),
        registry.Token.GetString()
    );

    public static IReadOnlyCollection<NugetTargetFramework> MapToNugetTargetFrameworks(
        this TargetFrameworkEntity? targetFrameworkEntity
    ) => targetFrameworkEntity is { } targetFramework ? [new NugetTargetFramework(targetFramework.TargetFramework)] : [];

    public static IReadOnlyCollection<IgnoreEntry> MapToIgnoreEntry(
        this DependabotConfiguration.Update.IgnoreArray ignoreArray
    )
    {
        return [];
    }

    public static IReadOnlyCollection<GroupEntry> MapToGroupEntry(
        this DependabotConfiguration.Update.GroupsEntity groupsEntity
    )
    {
        return [];
    }
}
