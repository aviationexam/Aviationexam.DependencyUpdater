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
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

/// <summary>
/// Temporary logging decorator for IDependencyVersionsFetcher that captures real execution data.
/// Writes JSON files to disk for later use in tests.
/// </summary>
public sealed class LoggingDependencyVersionsFetcher(
    [FromKeyedServices(IDependencyVersionsFetcher.Real)]
    IDependencyVersionsFetcher inner,
    ILogger<LoggingDependencyVersionsFetcher> logger
) : IDependencyVersionsFetcher, IAsyncDisposable
{
    private static readonly string OutputDirectory = Path.Combine(
        "/opt/asp.net/Aviationexam.DependencyUpdater/src/Aviationexam.DependencyUpdater.Nuget.Tests/Assets",
        $"captured-{Guid.NewGuid():N}"
    );

    private readonly IDictionary<string, PackageVersion> _storedDependency = new Dictionary<string, PackageVersion>();

    private readonly IDictionary<string, (PackageVersion, IReadOnlyCollection<NugetTargetFramework>, IReadOnlyCollection<PackageVersionWithDependencySets>)> _storedDependencyData =
        new Dictionary<string, (PackageVersion, IReadOnlyCollection<NugetTargetFramework>, IReadOnlyCollection<PackageVersionWithDependencySets>)>();

    static LoggingDependencyVersionsFetcher()
    {
        Directory.CreateDirectory(OutputDirectory);
    }

    public async ValueTask DisposeAsync()
    {
        var filePath = Path.Combine(OutputDirectory, nameof(FetchDependencyVersionsAsync));

        await using var fileStream = new FileStream(
            filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None
        );
        await using var streamWriter = new StreamWriter(fileStream);
        foreach (var (dependencyName, (version, nugetTargetFrameworks, dependencySetsCollection)) in _storedDependencyData)
        {
            await streamWriter.WriteAsync(
                // language=cs
                $$"""
                  KeyValuePair.Create(
                      new NugetDependency(
                          new NugetFile("", ENugetFileType.Csproj),
                          new NugetPackageReference("{{dependencyName}}", VersionRange.Parse("{{version.GetSerializedVersion()}}")),
                          [{{nugetTargetFrameworks.AsValueEnumerable().Select(x => $"new NugetTargetFramework(\"{x.TargetFramework}\")").JoinToString(", ")}}]
                      ),
                      (IReadOnlyCollection<PackageVersionWithDependencySets>)
                      [
                          {{DependencySetsAsCSharp(dependencySetsCollection, version)}}
                      ]
                  ),
                  """);
        }

        await streamWriter.FlushAsync();
        await fileStream.FlushAsync();

        logger.LogInformation("Captured package data to {FilePath}", filePath);
    }

    private static string DependencySetsAsCSharp(
        IReadOnlyCollection<PackageVersionWithDependencySets> dependencySets,
        PackageVersion packageVersion
    ) => dependencySets.AsValueEnumerable()
        .Where(x => x > packageVersion)
        .Select(x =>
            // language=cs
            $$"""
              new PackageVersionWithDependencySets(CreatePackageVersion("{{x.GetSerializedVersion()}}"))
              {
                  DependencySets = CreateDependencySets(
                      {{DependencySetsAsCSharp(x.DependencySets.Values.AsValueEnumerable().First())}}
                  ),
              },
              """
        )
        .JoinToString(",\n");

    private static string DependencySetsAsCSharp(
        IReadOnlyCollection<DependencySet> dependencySets
    ) => dependencySets.AsValueEnumerable()
        .Select(x =>
            // language=cs
            $"""
             new DependencySet("{x.TargetFramework}", [
                 {x.Packages
                     .AsValueEnumerable()
                     .Select(p => $"new PackageDependencyInfo(\"{p.Id}\", CreatePackageVersion(\"{p.OriginalVersionString}\"))")
                     .JoinToString(",\n")
                 }
             ])
             """
        )
        .JoinToString(",\n");

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

        var packageName = dependency.NugetPackage.GetPackageName();
        if (
            dependency.NugetPackage.GetVersion() is { } version
            && (
                !_storedDependency.TryGetValue(packageName, out var storedVersion)
                || storedVersion > version
            )
        )
        {
            _storedDependency[packageName] = version;
            _storedDependencyData[packageName] = (version, dependency.TargetFrameworks, result);
        }

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
