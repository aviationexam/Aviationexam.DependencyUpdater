using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using NSubstitute;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public partial class DependencyAnalyzerTests
{
    [Fact]
    public async Task AnalyzeDependencies_MicrosoftPackagesNet90_OnlyAnalyzesNet90Packages()
    {
        // Arrange
        var nugetFile = new NugetFile("Directory.Packages.props", ENugetFileType.DirectoryPackagesProps);

        var dependencies = new List<NugetDependency>
        {
            // Unconditional package (no updates available in this test)
            new(
                nugetFile,
                new NugetPackageVersion("Meziantou.Analyzer", "2.0.260"),
                [new NugetTargetFramework("net8.0"), new NugetTargetFramework("net9.0"), new NugetTargetFramework("net10.0")]
            ),
            // net8.0 packages (current, already at latest 8.x)
            new(
                nugetFile,
                new NugetPackageVersion("Microsoft.AspNetCore.WebUtilities", "8.0.22"),
                [new NugetTargetFramework("net8.0")]
            ),
            new(
                nugetFile,
                new NugetPackageVersion("System.Text.Json", "8.0.22"),
                [new NugetTargetFramework("net8.0")]
            ),
            // net9.0 packages (at 9.0.0, updates available to 9.0.11)
            new(
                nugetFile,
                new NugetPackageVersion("Microsoft.AspNetCore.WebUtilities", "9.0.0"),
                [new NugetTargetFramework("net9.0")]
            ),
            new(
                nugetFile,
                new NugetPackageVersion("Microsoft.Extensions.DependencyInjection", "9.0.0"),
                [new NugetTargetFramework("net9.0")]
            ),
            new(
                nugetFile,
                new NugetPackageVersion("System.Text.Json", "9.0.0"),
                [new NugetTargetFramework("net9.0")]
            ),
            // net10.0 packages (current, already at latest 10.x)
            new(
                nugetFile,
                new NugetPackageVersion("Microsoft.AspNetCore.WebUtilities", "10.0.1"),
                [new NugetTargetFramework("net10.0")]
            ),
            new(
                nugetFile,
                new NugetPackageVersion("System.Text.Json", "10.0.1"),
                [new NugetTargetFramework("net10.0")]
            ),
        };

        var context = new NugetUpdaterContext([DefaultNugetSource], dependencies);
        var mockSourceRepository = Substitute.For<SourceRepository>();
        var sourceRepositories = new Dictionary<NugetSource, NugetSourceRepository>
        {
            [DefaultNugetSource] = new(mockSourceRepository, null),
        };

        // Fetch real package metadata from NuGet.org and filter to specific versions
        // Only include versions for the major versions we're testing (8.x, 9.x, 10.x separately)
        var aspNetCoreMetadata = await FetchRealPackageMetadataAsync(
            "Microsoft.AspNetCore.WebUtilities",
            ["8.0.0", "8.0.22", "9.0.0", "9.0.1", "9.0.10", "9.0.11", "10.0.0", "10.0.1"]
        );

        var dependencyInjectionMetadata = await FetchRealPackageMetadataAsync(
            "Microsoft.Extensions.DependencyInjection",
            ["8.0.0", "8.0.22", "9.0.0", "9.0.1", "9.0.11"]
        );

        // System.Text.Json - include all major versions
        // The ignore rules will prevent cross-major-version updates
        var textJsonMetadata = await FetchRealPackageMetadataAsync(
            "System.Text.Json",
            ["8.0.0", "8.0.22", "9.0.0", "9.0.5", "9.0.11", "10.0.0", "10.0.1"]
        );

        var mezianiauAnalyzerMetadata = await FetchRealPackageMetadataAsync(
            "Meziantou.Analyzer",
            ["2.0.260"]
        );

        // Mock INugetVersionFetcher
        // Key insight: FetchPackageVersionsAsync returns ALL available versions from the NuGet feed
        // for a given package name, regardless of target framework. The filtering by target framework
        // happens later in the analysis pipeline (TargetFrameworksResolver, FutureVersionResolver).
        var mockVersionFetcher = Substitute.For<INugetVersionFetcher>();

        // Setup mocks - return real metadata for packages we care about
        mockVersionFetcher
            .FetchPackageVersionsAsync(
                Arg.Any<SourceRepository>(),
                Arg.Is<NugetDependency>(d => d.NugetPackage.GetPackageName() == "Microsoft.AspNetCore.WebUtilities"),
                Arg.Any<SourceCacheContext>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(aspNetCoreMetadata));

        mockVersionFetcher
            .FetchPackageVersionsAsync(
                Arg.Any<SourceRepository>(),
                Arg.Is<NugetDependency>(d => d.NugetPackage.GetPackageName() == "Microsoft.Extensions.DependencyInjection"),
                Arg.Any<SourceCacheContext>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(dependencyInjectionMetadata));

        mockVersionFetcher
            .FetchPackageVersionsAsync(
                Arg.Any<SourceRepository>(),
                Arg.Is<NugetDependency>(d => d.NugetPackage.GetPackageName() == "System.Text.Json"),
                Arg.Any<SourceCacheContext>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(textJsonMetadata));

        mockVersionFetcher
            .FetchPackageVersionsAsync(
                Arg.Any<SourceRepository>(),
                Arg.Is<NugetDependency>(d => d.NugetPackage.GetPackageName() == "Meziantou.Analyzer"),
                Arg.Any<SourceCacheContext>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(mezianiauAnalyzerMetadata));

        // Mock FetchPackageMetadataAsync to return null for transitive dependencies
        // This skips transitive dependency validation in this test
        mockVersionFetcher
            .FetchPackageMetadataAsync(
                Arg.Any<SourceRepository>(),
                Arg.Any<Package>(),
                Arg.Any<SourceCacheContext>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IPackageSearchMetadata?>(null));

        var analyzer = CreateDependencyAnalyzer(mockVersionFetcher);
        var currentPackageVersions = context.GetCurrentPackageVersions();
        var cachingConfiguration = new CachingConfiguration { MaxCacheAge = null };

        // Act
        var result = await analyzer.AnalyzeDependenciesAsync(
            context,
            sourceRepositories,
            [
                new IgnoreEntry("Microsoft.*", ["version-update:semver-major"]),
                new IgnoreEntry("System.*", ["version-update:semver-major"]),
            ],
            currentPackageVersions,
            cachingConfiguration,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.DependenciesToUpdate);

        // Verify only net9.0 packages have updates
        var net90Updates = result.DependenciesToUpdate
            .Where(kvp => kvp.Key.TargetFrameworks.Any(tf => tf.TargetFramework is "net9.0"))
            .ToList();

        Assert.Equal(3, net90Updates.Count); // 3 net9.0 packages should have updates
        Assert.Equal(3, result.DependenciesToUpdate.Count); // Only net9.0 packages should have updates

        // Verify specific packages - with ignore rules for semver-major, only minor/patch updates within same major version
        var aspNetCoreUpdate = Assert.Single(
            net90Updates,
            u => u.Key.NugetPackage.GetPackageName() is "Microsoft.AspNetCore.WebUtilities"
        );
        // Only 9.x updates (9.0.11, 9.0.10, 9.0.1), no 10.x due to semver-major ignore
        Assert.Equal(
            [new Version(9, 0, 11, 0), new Version(9, 0, 10, 0), new Version(9, 0, 1, 0)],
            aspNetCoreUpdate.Value.Select(x => x.PackageVersion.Version)
        );

        var dependencyInjectionUpdate = Assert.Single(
            net90Updates,
            u => u.Key.NugetPackage.GetPackageName() is "Microsoft.Extensions.DependencyInjection"
        );
        // Only 9.x updates (9.0.11, 9.0.1), no 8.x or 10.x due to semver-major ignore
        Assert.Equal(
            [new Version(9, 0, 11, 0), new Version(9, 0, 1, 0)],
            dependencyInjectionUpdate.Value.Select(x => x.PackageVersion.Version)
        );

        var textJsonUpdate = Assert.Single(
            net90Updates,
            u => u.Key.NugetPackage.GetPackageName() is "System.Text.Json"
        );
        // Only 9.x updates (9.0.11, 9.0.5), no 10.x due to semver-major ignore
        Assert.Equal(
            [new Version(9, 0, 11, 0), new Version(9, 0, 5, 0)],
            textJsonUpdate.Value.Select(x => x.PackageVersion.Version)
        );

        // Verify net8.0 and net10.0 packages have NO updates (they're already at latest for their framework)
        var net80Updates = result.DependenciesToUpdate
            .Where(kvp => kvp.Key.TargetFrameworks.Any(tf => tf.TargetFramework is "net8.0"))
            .ToList();

        var net100Updates = result.DependenciesToUpdate
            .Where(kvp => kvp.Key.TargetFrameworks.Any(tf => tf.TargetFramework is "net10.0"))
            .ToList();

        Assert.Empty(net80Updates);
        Assert.Empty(net100Updates);

        // Verify Meziantou.Analyzer has no updates
        var mezianiauUpdates = result.DependenciesToUpdate
            .Where(kvp => kvp.Key.NugetPackage.GetPackageName() == "Meziantou.Analyzer")
            .ToList();
        Assert.Empty(mezianiauUpdates);
    }
}
