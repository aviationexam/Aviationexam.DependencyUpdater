using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class NugetUpdaterContext(
    IReadOnlyCollection<NugetSource> nugetConfigurations,
    IReadOnlyCollection<NugetDependency> dependencies,
    NugetVersionFetcherFactory nugetVersionFetcherFactory
)
{
    public IReadOnlyCollection<NugetSource> NugetConfigurations => nugetConfigurations;
    public IReadOnlyCollection<NugetDependency> Dependencies => dependencies;

    public IEnumerable<KeyValuePair<NugetDependency, IReadOnlyCollection<NugetSource>>> MapSourceToDependency(
        ILogger logger
    ) => AsRecord().MapSourceToDependency(logger);

    public IEnumerable<NugetSource> GetSourcesForPackage(
        string packageName, ILogger logger
    ) => AsRecord().GetSourcesForPackage(packageName, logger);

    public IReadOnlyDictionary<string, PackageVersion> GetCurrentPackageVersions() =>
        AsRecord().GetCurrentPackageVersions();

    public IReadOnlyDictionary<NugetSource, NugetSourceRepository> GetSourceRepositories(
        IReadOnlyCollection<NugetFeedAuthentication> nugetFeedAuthentications,
        IReadOnlyDictionary<string, string> fallbackRegistries
    ) => AsRecord().GetSourceRepositories(nugetFeedAuthentications, fallbackRegistries, nugetVersionFetcherFactory);

    private Nuget.NugetUpdaterContext AsRecord() => new(NugetConfigurations, Dependencies);
}
