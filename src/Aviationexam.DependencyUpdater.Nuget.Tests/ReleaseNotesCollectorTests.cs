using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class ReleaseNotesCollectorTests
{
    private static readonly NugetSource Source = new("feed", "https://feed/v3/index.json", NugetSourceVersion.V3, []);
    private static readonly SourceRepository Repository = NuGet.Protocol.Core.Types.Repository.Factory.GetCoreV3("https://feed/v3/index.json");
    private static readonly SourceRepository FallbackRepository = NuGet.Protocol.Core.Types.Repository.Factory.GetCoreV3("https://fallback/v3/index.json");

    private static readonly CachingConfiguration CachingConfiguration = new() { MaxCacheAge = null };

    [Fact]
    public async Task CollectReleaseNotesAsync_FetchesEachPackageUpdateOnce()
    {
        // Arrange
        var fetcher = Substitute.For<INugetReleaseNotesFetcher>();
        fetcher.FetchReleaseNotesAsync(Repository, "PackageA", Arg.Any<PackageVersion?>(), Arg.Any<PackageVersion>(), Arg.Any<SourceCacheContext>(), Arg.Any<CancellationToken>())
            .Returns([new PackageReleaseNote("PackageA", "2.0.0", "Notes A")]);
        fetcher.FetchReleaseNotesAsync(Repository, "PackageB", Arg.Any<PackageVersion?>(), Arg.Any<PackageVersion>(), Arg.Any<SourceCacheContext>(), Arg.Any<CancellationToken>())
            .Returns([new PackageReleaseNote("PackageB", "2.0.0", "Notes B")]);

        var collector = new ReleaseNotesCollector(fetcher, NullLogger<ReleaseNotesCollector>.Instance);

        // Act
        var result = await collector.CollectReleaseNotesAsync(
            [
                CreateUpdateCandidate("PackageA", "1.0.0", "2.0.0", "net9.0"),
                CreateUpdateCandidate("PackageA", "1.0.0", "2.0.0", "net10.0"),
                CreateUpdateCandidate("PackageB", "1.0.0", "2.0.0"),
            ],
            new NugetUpdaterContext([Source], []),
            new Dictionary<NugetSource, NugetSourceRepository> { [Source] = new(Repository, null) },
            CachingConfiguration,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal(
            [
                new PackageReleaseNote("PackageA", "2.0.0", "Notes A"),
                new PackageReleaseNote("PackageB", "2.0.0", "Notes B"),
            ],
            result
        );
        await fetcher.ReceivedWithAnyArgs(2).FetchReleaseNotesAsync(default!, default!, default, default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CollectReleaseNotesAsync_MergesVersionsAcrossPrimaryAndFallbackRepositories()
    {
        // Arrange
        var fetcher = Substitute.For<INugetReleaseNotesFetcher>();
        fetcher.FetchReleaseNotesAsync(Repository, "PackageA", Arg.Any<PackageVersion?>(), Arg.Any<PackageVersion>(), Arg.Any<SourceCacheContext>(), Arg.Any<CancellationToken>())
            .Returns([new PackageReleaseNote("PackageA", "1.1.0", "Notes 1.1 primary")]);
        fetcher.FetchReleaseNotesAsync(FallbackRepository, "PackageA", Arg.Any<PackageVersion?>(), Arg.Any<PackageVersion>(), Arg.Any<SourceCacheContext>(), Arg.Any<CancellationToken>())
            .Returns([
                new PackageReleaseNote("PackageA", "1.1.0", "Notes 1.1 fallback"),
                new PackageReleaseNote("PackageA", "2.0.0", "Notes 2.0"),
            ]);

        var collector = new ReleaseNotesCollector(fetcher, NullLogger<ReleaseNotesCollector>.Instance);

        // Act
        var result = await collector.CollectReleaseNotesAsync(
            [CreateUpdateCandidate("PackageA", "1.0.0", "2.0.0")],
            new NugetUpdaterContext([Source], []),
            new Dictionary<NugetSource, NugetSourceRepository> { [Source] = new(Repository, FallbackRepository) },
            CachingConfiguration,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal(
            [
                new PackageReleaseNote("PackageA", "1.1.0", "Notes 1.1 primary"),
                new PackageReleaseNote("PackageA", "2.0.0", "Notes 2.0"),
            ],
            result
        );
    }

    [Fact]
    public async Task CollectReleaseNotesAsync_FetcherFailure_SkipsPackageAndKeepsOthers()
    {
        // Arrange
        var fetcher = Substitute.For<INugetReleaseNotesFetcher>();
        fetcher.FetchReleaseNotesAsync(Repository, "PackageA", Arg.Any<PackageVersion?>(), Arg.Any<PackageVersion>(), Arg.Any<SourceCacheContext>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("feed unavailable"));
        fetcher.FetchReleaseNotesAsync(Repository, "PackageB", Arg.Any<PackageVersion?>(), Arg.Any<PackageVersion>(), Arg.Any<SourceCacheContext>(), Arg.Any<CancellationToken>())
            .Returns([new PackageReleaseNote("PackageB", "2.0.0", "Notes B")]);

        var collector = new ReleaseNotesCollector(fetcher, NullLogger<ReleaseNotesCollector>.Instance);

        // Act
        var result = await collector.CollectReleaseNotesAsync(
            [
                CreateUpdateCandidate("PackageA", "1.0.0", "2.0.0"),
                CreateUpdateCandidate("PackageB", "1.0.0", "2.0.0"),
            ],
            new NugetUpdaterContext([Source], []),
            new Dictionary<NugetSource, NugetSourceRepository> { [Source] = new(Repository, null) },
            CachingConfiguration,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal([new PackageReleaseNote("PackageB", "2.0.0", "Notes B")], result);
    }

    [Fact]
    public async Task CollectReleaseNotesAsync_WithoutSourceRepository_ReturnsEmpty()
    {
        // Arrange
        var fetcher = Substitute.For<INugetReleaseNotesFetcher>();
        var collector = new ReleaseNotesCollector(fetcher, NullLogger<ReleaseNotesCollector>.Instance);

        // Act
        var result = await collector.CollectReleaseNotesAsync(
            [CreateUpdateCandidate("PackageA", "1.0.0", "2.0.0")],
            new NugetUpdaterContext([Source], []),
            new Dictionary<NugetSource, NugetSourceRepository>(),
            CachingConfiguration,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Empty(result);
        await fetcher.DidNotReceiveWithAnyArgs().FetchReleaseNotesAsync(default!, default!, default, default!, default!, TestContext.Current.CancellationToken);
    }

    private static NugetUpdateCandidate CreateUpdateCandidate(
        string packageName,
        string fromVersion,
        string toVersion,
        string? condition = null
    ) => new(
        new NugetDependency(
            new NugetFile("Test.csproj", ENugetFileType.Csproj),
            new NugetPackageReference(packageName, new VersionRange(new NuGetVersion(fromVersion)), condition),
            [new NugetTargetFramework(condition ?? "net10.0")]
        ),
        new PossiblePackageVersion(
            new PackageVersionWithDependencySets(new NuGetVersion(toVersion).MapToPackageVersion())
            {
                DependencySets = new Dictionary<EPackageSource, IReadOnlyCollection<DependencySet>>(),
            },
            []
        )
    );
}
