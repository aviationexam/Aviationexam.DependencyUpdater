using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class NugetVersionFetcherFactory(
    Repository.RepositoryFactory repositoryFactory
)
{
    public SourceRepository CreateSourceRepository(
        NugetSource nugetSource,
        IReadOnlyCollection<NugetFeedAuthentication> nugetFeedAuthentications
    )
    {
        var nugetFeedAuthentication = nugetFeedAuthentications.SingleOrDefault(x => x.FeedUrl == nugetSource.Source);

        var packageSource = new PackageSource(nugetSource.Source)
        {
            Credentials = nugetFeedAuthentication is null
                ? null
                : new PackageSourceCredential(
                    nugetSource.Source,
                    "<username>",
                    "<personal_access_token>",
                    isPasswordClearText: true,
                    validAuthenticationTypesText: null
                ),
        };

        return nugetSource.Version switch
        {
            NugetSourceVersion.V3 => repositoryFactory.GetCoreV3(packageSource),
            NugetSourceVersion.V2 => repositoryFactory.GetCoreV2(packageSource),
            _ => throw new ArgumentOutOfRangeException(nameof(nugetSource.Version), nugetSource.Version, null),
        };
    }
}
