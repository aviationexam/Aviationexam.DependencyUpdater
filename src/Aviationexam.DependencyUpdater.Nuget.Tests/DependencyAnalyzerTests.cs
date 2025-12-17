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
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

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
    protected static DependencyAnalyzer CreateDependencyAnalyzer(
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
    protected static IPackageSearchMetadata CreatePackageMetadata(
        string packageName,
        string version,
        IEnumerable<PackageDependencyGroup>? dependencyGroups = null
    ) => new PackageSearchMetadataRegistration()
        .SetPackageId(packageName)
        .SetVersion(NuGetVersion.Parse(version))
        .SetDependencySetsInternal(dependencyGroups ?? []);

    /// <summary>
    /// Creates a PackageDependencyGroup for a specific target framework.
    /// </summary>
    protected static PackageDependencyGroup CreateDependencyGroup(
        string targetFramework,
        params IReadOnlyCollection<(string Name, string VersionRange)> dependencies
    )
    {
        var tfm = NuGetFramework.Parse(targetFramework);
        var packages = dependencies.Select(d =>
            new PackageDependency(
                d.Name,
                VersionRange.Parse(d.VersionRange)
            )
        ).ToList();

        return new PackageDependencyGroup(tfm, packages);
    }

    /// <summary>
    /// Helper to verify that a dependency analysis result contains expected updates.
    /// </summary>
    protected static void AssertContainsUpdate(
        DependencyAnalysisResult result,
        string packageName,
        string currentVersion,
        string[] expectedVersions,
        string[] targetFrameworks
    )
    {
        var updates = result.DependenciesToUpdate
            .Where(kvp => kvp.Key.NugetPackage.GetPackageName() == packageName)
            .ToList();

        Assert.NotEmpty(updates);

        foreach (var update in updates)
        {
            var dependency = update.Key;
            var versions = update.Value;

            // Verify current version
            Assert.Equal(currentVersion, ((NugetPackageVersion) dependency.NugetPackage).Version.ToString());

            // Verify target frameworks match
            var depFrameworks = dependency.TargetFrameworks.Select(tf => tf.TargetFramework).ToArray();
            Assert.Subset(new HashSet<string>(targetFrameworks), new HashSet<string>(depFrameworks));

            // Verify available versions
            var availableVersions = versions.Select(v => v.PackageVersion.Version.ToString()).ToArray();
            foreach (var expectedVersion in expectedVersions)
            {
                Assert.Contains(expectedVersion, availableVersions);
            }
        }
    }

    /// <summary>
    /// Loads package metadata from embedded JSON resource file.
    /// This provides realistic test data with real dependency information without requiring network calls.
    /// </summary>
    protected static IEnumerable<IPackageSearchMetadata> LoadPackageMetadataFromResource(
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
            NugetJsonSerializerContext.Default.IEnumerablePackageSearchMetadataRegistration
        );

        if (allMetadata == null)
        {
            throw new InvalidOperationException($"Failed to deserialize metadata from {resourceName}");
        }

        return allMetadata;
    }

    /// <summary>
    /// Fetches real package metadata from NuGet.org for a given package and filters to only include specified versions.
    /// This provides realistic test data with real dependency information.
    /// Returns PackageSearchMetadataRegistration instances which are the actual concrete type returned by NuGet.
    /// NOTE: Prefer LoadPackageMetadataFromResource() to avoid network calls during tests.
    /// </summary>
    protected static async Task<IEnumerable<IPackageSearchMetadata>> FetchRealPackageMetadataAsync(
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
            .Where(m => versionsToInclude.Contains(m.Identity.Version.ToString()))
            .OrderBy(m => m.Identity.Version)
            .ToList();

        TestContext.Current.AddAttachment(packageName, JsonSerializer.Serialize(
            result,
            NugetJsonSerializerContext.Default.IEnumerableIPackageSearchMetadata
        ));

        return result;
    }
}
