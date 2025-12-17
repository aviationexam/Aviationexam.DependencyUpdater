using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using Aviationexam.DependencyUpdater.Nuget.Tests.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

/// <summary>
/// Integration tests for DependencyAnalyzer.AnalyzeDependenciesAsync.
/// Tests the full dependency analysis pipeline with mocked INugetVersionFetcher.
/// Uses real instances of FutureVersionResolver, TargetFrameworksResolver, etc.
/// </summary>
public partial class DependencyAnalyzerTests
{
    private static readonly NugetSource DefaultNugetSource = new("nuget.org", "https://api.nuget.org/v3/index.json", NugetSourceVersion.V3, []);

    /// <summary>
    /// Creates a DependencyAnalyzer with real service instances and mocked version fetcher.
    /// </summary>
    private static DependencyAnalyzer CreateDependencyAnalyzer(
        INugetVersionFetcher mockVersionFetcher
    )
    {
        var logger = Substitute.For<ILogger<DependencyAnalyzer>>();
        var futureVersionResolver = new FutureVersionResolver();
        var targetFrameworksResolver = new TargetFrameworksResolver();
        var ignoredDependenciesResolver = new IgnoredDependenciesResolver();
        var ignoreResolverFactoryLogger = Substitute.For<ILogger<IgnoreResolverFactory>>();
        var ignoreResolverFactory = new IgnoreResolverFactory(ignoreResolverFactoryLogger);

        return new DependencyAnalyzer(
            mockVersionFetcher,
            futureVersionResolver,
            targetFrameworksResolver,
            ignoredDependenciesResolver,
            ignoreResolverFactory,
            logger
        );
    }

    /// <summary>
    /// Creates a mock IPackageSearchMetadata for a specific package version.
    /// </summary>
    private static IPackageSearchMetadata CreatePackageMetadata(
        SerializedPackage registration
    ) => new PackageSearchMetadataRegistration()
        .SetPackageId(registration.PackageId)
        .SetVersion(NuGetVersion.Parse(registration.Version))
        .SetDependencySetsInternal(registration.DependencySets.AsValueEnumerable().Select(d => new PackageDependencyGroup(
            new NuGetFramework(
                d.Framework,
                Version.Parse(d.Version),
                d.Platform,
                Version.Parse(d.PlatformVersion)
            ),
            d.Packages.AsValueEnumerable().Select(p => new PackageDependency(p.Id, VersionRange.Parse(p.VersionRange))).ToList()
        )).ToList());

    /// <summary>
    /// Loads package metadata from embedded JSON resource file.
    /// This provides realistic test data with real dependency information without requiring network calls.
    /// </summary>
    private static IEnumerable<IPackageSearchMetadata> LoadPackageMetadataFromResource(
        string packageName
    )
    {
        var assembly = typeof(DependencyAnalyzerTests).Assembly;
        var resourceName = $"Aviationexam.DependencyUpdater.Nuget.Tests.Assets.{packageName}.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Embedded resource not found: {resourceName}");
        }

        var allMetadata = JsonSerializer.Deserialize(
            stream,
            NugetJsonSerializerContext.Default.IEnumerableSerializedPackage
        );

        if (allMetadata == null)
        {
            throw new InvalidOperationException($"Failed to deserialize metadata from {resourceName}");
        }

        return allMetadata.AsValueEnumerable().Select(CreatePackageMetadata).ToList();
    }

    /// <summary>
    /// Fetches real package metadata from NuGet.org for a given package and filters to only include specified versions.
    /// This provides realistic test data with real dependency information.
    /// Returns PackageSearchMetadataRegistration instances which are the actual concrete type returned by NuGet.
    /// NOTE: Prefer LoadPackageMetadataFromResource() to avoid network calls during tests.
    /// </summary>
    private static async Task<IEnumerable<IPackageSearchMetadata>> FetchRealPackageMetadataAsync(
        string packageName,
        IReadOnlyCollection<string> versionsToInclude
    )
    {
        var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        var resource = await repository.GetResourceAsync<PackageMetadataResource>();

        using var cacheContext = new SourceCacheContext();
        var metadata = await resource.GetMetadataAsync(
            packageName,
            includePrerelease: true,
            includeUnlisted: false,
            cacheContext,
            NullLogger.Instance,
            TestContext.Current.CancellationToken
        );

        var result = metadata
            .AsValueEnumerable()
            .Where(m => versionsToInclude.AsValueEnumerable().Contains(m.Identity.Version.ToString()))
            .OrderBy(m => m.Identity.Version)
            .ToList();

        var json = JsonSerializer.Serialize(
            result.AsValueEnumerable().OfType<PackageSearchMetadataRegistration>().Select(x =>
            {
                return new SerializedPackage
                {
                    PackageId = x.PackageId,
                    Version = x.Version.ToString(),
                    DependencySets = x.DependencySets.AsValueEnumerable().Select(d => new SerializedDependencySet
                    {
                        Framework = d.TargetFramework.Framework,
                        Version = d.TargetFramework.Version.ToString(),
                        Platform = d.TargetFramework.Platform,
                        PlatformVersion = d.TargetFramework.PlatformVersion.ToString(),
                        Packages = d.Packages.AsValueEnumerable().Select(p => new SerializedDependency
                        {
                            Id = p.Id,
                            VersionRange = p.VersionRange.ToString(),
                        }).ToArray(),
                    }).ToArray(),
                };
            }).ToArray(),
            NugetJsonSerializerContext.Default.IEnumerableSerializedPackage
        );
        TestContext.Current.AddAttachment(packageName, json);

        return result.AsValueEnumerable().OfType<PackageSearchMetadataRegistration>().Select(x =>
        {
            return new SerializedPackage
            {
                PackageId = x.PackageId,
                Version = x.Version.ToString(),
                DependencySets = x.DependencySets.AsValueEnumerable().Select(d => new SerializedDependencySet
                {
                    Framework = d.TargetFramework.Framework,
                    Version = d.TargetFramework.Version.ToString(),
                    Platform = d.TargetFramework.Platform,
                    PlatformVersion = d.TargetFramework.PlatformVersion.ToString(),
                    Packages = d.Packages.AsValueEnumerable().Select(p => new SerializedDependency
                    {
                        Id = p.Id,
                        VersionRange = p.VersionRange.ToString(),
                    }).ToArray(),
                }).ToArray(),
            };
        }).Select(CreatePackageMetadata).ToArray();
    }
}
