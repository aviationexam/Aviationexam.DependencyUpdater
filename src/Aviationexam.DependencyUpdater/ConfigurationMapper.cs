using Aviationexam.DependencyUpdater.ConfigurationParser;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget;

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
}
