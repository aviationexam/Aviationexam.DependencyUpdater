using Aviationexam.DependencyUpdater.Interfaces;

namespace Aviationexam.DependencyUpdater.Nuget;

public static class NugetFeedAuthenticationFactory
{
    public static NugetFeedAuthentication CreateNugetFeedAuthentication(
        IEnvVariableProvider envVariableProvider,
        string url,
        string? username,
        string? password,
        string? token
    )
    {
        // TODO envVariableProvider, token
        return new NugetFeedAuthentication(url, username, password);
    }
}
