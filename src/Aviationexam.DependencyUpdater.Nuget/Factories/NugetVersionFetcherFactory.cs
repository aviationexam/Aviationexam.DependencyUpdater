using Aviationexam.DependencyUpdater.Nuget.Configurations;
using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Factories;

public sealed class NugetVersionFetcherFactory(
    Repository.RepositoryFactory repositoryFactory
)
{
    public NugetSourceRepository CreateSourceRepositories(
        NugetSource nugetSource,
        IReadOnlyDictionary<string, string> fallbackRegistries,
        IReadOnlyCollection<NugetFeedAuthentication> nugetFeedAuthentications
    )
    {
        var nugetFeedAuthentication = nugetFeedAuthentications.AsValueEnumerable().SingleOrDefault(x => x.FeedUrl == nugetSource.Source);

        var sourceRepository = CreateSourceRepository(nugetSource, nugetFeedAuthentication);

        var fallbackSourceRepository =
            nugetFeedAuthentication is { Key: { } registryKey }
            && fallbackRegistries.TryGetValue(registryKey, out var fallbackRegistryKey)
            && nugetFeedAuthentications.AsValueEnumerable().SingleOrDefault(x => x.Key == fallbackRegistryKey) is { } nugetFallbackFeedAuthentication
                ? CreateSourceRepository(new NugetSource(
                    fallbackRegistryKey,
                    nugetFallbackFeedAuthentication.FeedUrl,
                    nugetFallbackFeedAuthentication.Version,
                    []
                ), nugetFallbackFeedAuthentication)
                : null;

        return new NugetSourceRepository(
            sourceRepository,
            fallbackSourceRepository
        );
    }

    private SourceRepository CreateSourceRepository(
        NugetSource nugetSource,
        NugetFeedAuthentication? nugetFeedAuthentication
    )
    {
        var packageSource = new PackageSource(nugetSource.Source)
        {
            Credentials = nugetFeedAuthentication is { Username: not null, Password: not null }
                ? new PackageSourceCredential(
                    nugetSource.Source,
                    nugetFeedAuthentication.Username,
                    nugetFeedAuthentication.Password,
                    isPasswordClearText: true,
                    validAuthenticationTypesText: null
                )
                : null,
        };

        return nugetSource.Version switch
        {
            NugetSourceVersion.V3 => repositoryFactory.GetCoreV3(packageSource),
            NugetSourceVersion.V2 => repositoryFactory.GetCoreV2(packageSource),
            _ => throw new ArgumentOutOfRangeException(nameof(nugetSource.Version), nugetSource.Version, null),
        };
    }
}
