using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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
    private readonly string _currentRunKey = Guid.NewGuid().ToString("N");

    private readonly IDictionary<string, PackageVersion> _storedDependency = new ConcurrentDictionary<string, PackageVersion>();

    private readonly IDictionary<(string, PackageVersion), IReadOnlyCollection<NugetTargetFramework>> _storedDependencyVersionTarget =
        new ConcurrentDictionary<(string, PackageVersion), IReadOnlyCollection<NugetTargetFramework>>();

    private readonly IDictionary<string, IReadOnlyCollection<PackageVersionWithDependencySets>> _storedDependencyData =
        new ConcurrentDictionary<string, IReadOnlyCollection<PackageVersionWithDependencySets>>();

    private string? GetDirectory(
        [CallerFilePath] string? filePath = null
    ) => Path.GetDirectoryName(filePath);

    public async ValueTask DisposeAsync()
    {
        var outputDirectory = Path.Combine(
            GetDirectory()!,
            "../../Aviationexam.DependencyUpdater.Nuget.Tests/Assets",
            $"captured-{_currentRunKey}"
        );
        Directory.CreateDirectory(outputDirectory);

        var fetchDependencyVersionsFilePath = Path.Combine(outputDirectory, nameof(FetchDependencyVersionsAsync) + ".cs");
        var fetchDependencyVersionsFactoryMethodsFilePath = Path.Combine(outputDirectory, nameof(FetchDependencyVersionsAsync) + "_Methods.cs");

        await using var fetchDependencyVersionsFileStream = new FileStream(
            fetchDependencyVersionsFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None
        );
        await using var fetchDependencyVersionsStreamWriter = new StreamWriter(fetchDependencyVersionsFileStream);

        await using var fetchDependencyVersionsFactoryMethodsFileStream = new FileStream(
            fetchDependencyVersionsFactoryMethodsFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None
        );
        await using var fetchDependencyVersionsFactoryMethodsStreamWriter = new StreamWriter(fetchDependencyVersionsFactoryMethodsFileStream);

        var knownTargetFrameworks = _storedDependencyVersionTarget.AsValueEnumerable().SelectMany(x => x.Value).Distinct().ToList();

        var knownFactoryMethods = new HashSet<string>();

        var factoryClass = $"FactoryClass_{_currentRunKey}";

        await fetchDependencyVersionsFactoryMethodsStreamWriter.WriteLineAsync(
            $$"""
              private static class {{factoryClass}}
              {
              """
        );

        foreach (var ((dependencyName, version), nugetTargetFrameworks) in _storedDependencyVersionTarget.AsValueEnumerable().OrderBy(x => x.Key.Item1).ToList())
        {
            var minVersion = _storedDependency[dependencyName];
            var dependencySetsCollection = _storedDependencyData[dependencyName];

            string? dependencySetsFactoryMethod = null;
            if (nugetTargetFrameworks.Count != knownTargetFrameworks.Count)
            {
                dependencySetsFactoryMethod = $"CreateDependencySets{dependencyName.Replace('.', '_').Replace('-', '_')}";
                if (knownFactoryMethods.Add(dependencySetsFactoryMethod))
                {
                    await fetchDependencyVersionsFactoryMethodsStreamWriter.WriteLineAsync(
                        // language=cs
                        $$"""
                          public static IReadOnlyCollection<PackageVersionWithDependencySets> {{dependencySetsFactoryMethod}}() => [
                              {{DependencySetsAsCSharp(dependencySetsCollection, minVersion)}}
                          ];

                          """
                    );
                }
            }

            var dependencySets = dependencySetsFactoryMethod is null
                ?
                // language=cs
                $"""
                 (IReadOnlyCollection<PackageVersionWithDependencySets>)
                 [
                     {DependencySetsAsCSharp(dependencySetsCollection, minVersion)}
                 ]
                 """
                : $"{factoryClass}.{dependencySetsFactoryMethod}()";

            await fetchDependencyVersionsStreamWriter.WriteLineAsync(
                // language=cs
                $$"""
                  KeyValuePair.Create(
                      new NugetDependency(
                          new NugetFile("", ENugetFileType.Csproj),
                          new NugetPackageReference("{{dependencyName}}", VersionRange.Parse("{{version.GetSerializedVersion()}}")),
                          [{{nugetTargetFrameworks.AsValueEnumerable().Select(x => GetNugetTargetFramework(x.TargetFramework)).JoinToString(", ")}}]
                      ),
                      {{dependencySets}}
                  ),
                  """);
        }

        await fetchDependencyVersionsFactoryMethodsStreamWriter.WriteLineAsync("}");

        await fetchDependencyVersionsStreamWriter.FlushAsync();
        await fetchDependencyVersionsFileStream.FlushAsync();

        await fetchDependencyVersionsFactoryMethodsStreamWriter.FlushAsync();
        await fetchDependencyVersionsFactoryMethodsFileStream.FlushAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Captured package data to {FilePath}", fetchDependencyVersionsFilePath);
        }
    }

    public static string GetNugetTargetFramework(
        string targetFramework
    ) => targetFramework switch
    {
        "net48" => "Net48",
        "net8.0" => "Net80",
        "net9.0" => "Net90",
        "net10.0" => "Net100",
        _ => $"new NugetTargetFramework(\"{targetFramework}\")",
    };

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
              }
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
                     .Select(p => $"new PackageDependencyInfo(\"{p.Id}\", CreatePackageVersion(\"{p.MinVersion?.GetSerializedVersion()}\"))")
                     .JoinToString(",\n")}
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
        var packageVersion = dependency.NugetPackage.GetVersion();
        if (
            packageVersion != null
            && (
                !_storedDependency.TryGetValue(packageName, out var storedVersion)
                || storedVersion > packageVersion
            )
        )
        {
            _storedDependency[packageName] = packageVersion;
        }

        _storedDependencyData.TryAdd(packageName, result);

        if (
            packageVersion != null
            && !_storedDependencyVersionTarget.ContainsKey((packageName, packageVersion))
        )
        {
            _storedDependencyVersionTarget.Add((packageName, packageVersion), dependency.TargetFrameworks);
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

        return result;
    }
}
