using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

/// <summary>
/// Temporary logging decorator for IDependencyVersionsFetcher that captures real execution data.
/// Writes JSON files to disk for later use in tests.
/// </summary>
public sealed class LoggingDependencyVersionsFetcher(
    [FromKeyedServices(IDependencyVersionsFetcher.Real)]
    IDependencyVersionsFetcher inner,
    ILogger<LoggingDependencyVersionsFetcher> logger
) : IDependencyVersionsFetcher
{
    private static readonly string OutputDirectory = Path.Combine(
        "/opt/asp.net/Aviationexam.DependencyUpdater/src/Aviationexam.DependencyUpdater.Nuget.Tests/Assets",
        $"captured-{Guid.NewGuid():N}"
    );

    static LoggingDependencyVersionsFetcher()
    {
        Directory.CreateDirectory(OutputDirectory);
    }

    public async Task<IReadOnlyCollection<PackageVersionWithDependencySets>> FetchDependencyVersionsAsync(
        NugetDependency dependency,
        IReadOnlyCollection<NugetSource> sources,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        var result = await inner.FetchDependencyVersionsAsync(
            dependency,
            sources,
            sourceRepositories,
            cachingConfiguration,
            cancellationToken
        );

        await LogResultAsync($"FetchDependencyVersions.{dependency.NugetPackage.GetPackageName()}.json", result);

        return result;
    }

    public async Task<PackageVersionWithDependencySets?> FetchPackageMetadataAsync(
        Package package,
        IReadOnlyCollection<NugetSource> sources,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        var result = await inner.FetchPackageMetadataAsync(
            package,
            sources,
            sourceRepositories,
            cachingConfiguration,
            cancellationToken
        );

        if (result is not null)
        {
            await LogResultAsync($"FetchPackageMetadata.{package.Name}={package.Version.GetSerializedVersion()}.json", [result]);
        }
        else
        {
            await LogResultAsync($"FetchPackageMetadata.{package.Name}={package.Version.GetSerializedVersion()}.json");
        }

        return result;
    }

    private async Task LogResultAsync(
        string baseFileName,
        params IEnumerable<PackageVersionWithDependencySets> results
    )
    {
        try
        {
            var filePath = Path.Combine(OutputDirectory, baseFileName);

            var resultsList = results.ToList();
            var json = JsonSerializer.Serialize(resultsList, NugetJsonSerializerContext.Default.IReadOnlyCollectionPackageVersionWithDependencySets);

            await File.WriteAllTextAsync(filePath, json);

            logger.LogInformation("Captured package data to {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to log package data to {FileName}", baseFileName);
        }
    }
}
