using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using System;

namespace Aviationexam.DependencyUpdater.Nuget;

public static class NugetFeedAuthenticationFactory
{
    public static NugetFeedAuthentication CreateNugetFeedAuthentication(
        IEnvVariableProvider envVariableProvider,
        string key,
        string url,
        string? username,
        string? password,
        string? token,
        string? nugetFeedVersion
    )
    {
        username = username is null ? null : envVariableProvider.PopulateEnvironmentVariables(username);
        password = password is null ? null : envVariableProvider.PopulateEnvironmentVariables(password);
        token = token is null ? null : envVariableProvider.PopulateEnvironmentVariables(token);

        if (!Enum.TryParse<NugetSourceVersion>(nugetFeedVersion, ignoreCase: true, out var nugetSourceVersion))
        {
            nugetSourceVersion = NugetSourceVersion.Unknown;
        }

        if (
            string.IsNullOrEmpty(username)
            && string.IsNullOrEmpty(password)
            && !string.IsNullOrEmpty(token)
            && token.Split(':', 2, StringSplitOptions.TrimEntries) is { Length: 2 } explodedToken
        )
        {
            username = explodedToken[0];
            password = explodedToken[1];
        }

        return new NugetFeedAuthentication(
            key,
            url,
            username,
            password,
            nugetSourceVersion
        );
    }
}
