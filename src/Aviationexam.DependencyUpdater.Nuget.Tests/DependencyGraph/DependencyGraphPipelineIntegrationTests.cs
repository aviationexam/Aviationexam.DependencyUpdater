using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;
using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Services;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Helpers;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Parsers;
using Aviationexam.DependencyUpdater.Nuget.Services;
using Aviationexam.DependencyUpdater.Nuget.Tests.Extensions;
using Aviationexam.DependencyUpdater.TestsInfrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests.DependencyGraph;

public sealed class DependencyGraphPipelineIntegrationTests
{
    private static readonly NugetSource DefaultNugetSource = new(
        "nuget.org",
        "https://api.nuget.org/v3/index.json",
        NugetSourceVersion.V3,
        ImmutableArray<NugetPackageSourceMap>.Empty
    );

    [Fact]
    public async Task BuildColoredGraphAsync_FullExampleScenario_ProducesExpectedGraphStructure()
    {
        // Arrange
        const string repositoryPath = "/repo";
        var tfmNet90 = new NugetTargetFramework("net9.0");

        var projectAFilename = Path.Join(repositoryPath, "src", "A", "ProjectA.csproj");
        var projectBFilename = Path.Join(repositoryPath, "src", "B", "ProjectB.csproj");

        var fileSystem = CreateFileSystem(
            [projectAFilename, projectBFilename],
            new Dictionary<string, string>
            {
                [projectAFilename] =
                    """
                    <Project Sdk="Microsoft.NET.Sdk">
                      <PropertyGroup>
                        <TargetFramework>net9.0</TargetFramework>
                      </PropertyGroup>
                      <ItemGroup>
                        <PackageReference Include="D" Version="1.0.0.0" />
                        <ProjectReference Include="../B/ProjectB.csproj" />
                      </ItemGroup>
                    </Project>
                    """,
                [projectBFilename] =
                    """
                    <Project Sdk="Microsoft.NET.Sdk">
                      <PropertyGroup>
                        <TargetFramework>net9.0</TargetFramework>
                      </PropertyGroup>
                      <ItemGroup>
                        <PackageReference Include="E" Version="1.0.0.0" />
                      </ItemGroup>
                    </Project>
                    """,
            }
        );

        var versionFetcher = CreateVersionFetcherMock(new Dictionary<string, IReadOnlyCollection<IPackageSearchMetadata>>(StringComparer.OrdinalIgnoreCase)
        {
            ["D"] = [CreateMetadata("D", "1.0.0.0", ("net9.0", [("E", "1.0.0.0"), ("F", "1.0.0.0")]))],
            ["E"] = [CreateMetadata("E", "1.0.0.0")],
            ["F"] = [CreateMetadata("F", "1.0.0.0")],
        });

        var pipeline = CreatePipeline(fileSystem, versionFetcher);

        var repositoryConfig = new RepositoryConfig
        {
            RepositoryPath = repositoryPath,
        };

        var context = new NugetUpdaterContext(
            [],
            [
                CreateDependency("src/A/ProjectA.csproj", "D", "1.0.0.0", [tfmNet90]),
                CreateDependency("src/B/ProjectB.csproj", "E", "1.0.0.0", [tfmNet90]),
            ]
        );

        var sourceRepositories = CreateSourceRepositories();
        var caching = new CachingConfiguration { MaxCacheAge = null };

        // Act
        var graph = await pipeline.BuildColoredGraphAsync(
            repositoryConfig,
            context,
            sourceRepositories,
            caching,
            TestContext.Current.CancellationToken
        );

        await versionFetcher.Received().FetchPackageVersionsAsync(
            Arg.Any<SourceRepository>(),
            Arg.Is<NugetDependency>(d => d.NugetPackage.GetPackageName() == "D"),
            Arg.Any<SourceCacheContext>(),
            Arg.Any<CancellationToken>()
        );

        // Assert
        Assert.Contains(graph.Nodes.Values, n => n.PackageName == "D");
        Assert.Contains(graph.Nodes.Values, n => n.PackageName == "E");

        Assert.Contains(graph.ProjectLinks, l =>
            l.ProjectName == "ProjectB"
            && l.Node.PackageName == "E"
            && l.Nature == EDependencyLinkNature.Direct
        );

        Assert.Contains(graph.ProjectLinks, l =>
            l.ProjectName == "ProjectA"
            && l.Node.PackageName == "D"
            && l.Nature == EDependencyLinkNature.Direct
        );

        Assert.Contains(graph.ProjectLinks, l =>
            l.ProjectName == "ProjectA"
            && l.Node.PackageName == "E"
            && l.Nature == EDependencyLinkNature.Transitive
        );

        Assert.Contains(graph.ProjectLinks, l =>
            l.ProjectName == "ProjectA"
            && l.Node.PackageName == "F"
            && l.Nature == EDependencyLinkNature.Transitive
        );
    }

    [Fact]
    public async Task BuildColoredGraphAsync_MultiTfmDependencies_KeepsTfmScopedEdges()
    {
        // Arrange
        const string repositoryPath = "/repo";
        var tfmNet90 = new NugetTargetFramework("net9.0");
        var tfmNet100 = new NugetTargetFramework("net10.0");

        var projectXFilename = Path.Join(repositoryPath, "src", "X", "ProjectX.csproj");

        var fileSystem = CreateFileSystem(
            [projectXFilename],
            new Dictionary<string, string>
            {
                [projectXFilename] =
                    """
                    <Project Sdk="Microsoft.NET.Sdk">
                      <PropertyGroup>
                        <TargetFrameworks>net9.0;net10.0</TargetFrameworks>
                      </PropertyGroup>
                      <ItemGroup>
                        <PackageReference Include="X" Version="1.0.0.0" />
                      </ItemGroup>
                    </Project>
                    """,
            }
        );

        var versionFetcher = CreateVersionFetcherMock(new Dictionary<string, IReadOnlyCollection<IPackageSearchMetadata>>(StringComparer.OrdinalIgnoreCase)
        {
            ["X"] = [CreateMetadata("X", "1.0.0.0", ("net9.0", [("Y", "1.0.0.0")]), ("net10.0", [("Z", "1.0.0.0")]))],
            ["Y"] = [CreateMetadata("Y", "1.0.0.0")],
            ["Z"] = [CreateMetadata("Z", "1.0.0.0")],
        });

        var pipeline = CreatePipeline(fileSystem, versionFetcher);

        var repositoryConfig = new RepositoryConfig
        {
            RepositoryPath = repositoryPath,
        };

        var context = new NugetUpdaterContext(
            [],
            [CreateDependency("src/X/ProjectX.csproj", "X", "1.0.0.0", [tfmNet90, tfmNet100])]
        );

        // Act
        var graph = await pipeline.BuildColoredGraphAsync(
            repositoryConfig,
            context,
            CreateSourceRepositories(),
            new CachingConfiguration { MaxCacheAge = null },
            TestContext.Current.CancellationToken
        );

        await versionFetcher.Received().FetchPackageVersionsAsync(
            Arg.Any<SourceRepository>(),
            Arg.Is<NugetDependency>(d => d.NugetPackage.GetPackageName() == "X"),
            Arg.Any<SourceCacheContext>(),
            Arg.Any<CancellationToken>()
        );

        // Assert
        Assert.Contains(graph.Nodes.Values, n => n.PackageName == "X");
        Assert.Contains(graph.Nodes.Values, n => n.PackageName == "Y");
        Assert.Contains(graph.Nodes.Values, n => n.PackageName == "Z");

        Assert.Contains(graph.Edges, e =>
            e.From.PackageName == "X"
            && e.To.PackageName == "Y"
            && e.TargetFrameworks.Contains(tfmNet90)
        );

        Assert.Contains(graph.Edges, e =>
            e.From.PackageName == "X"
            && e.To.PackageName == "Z"
            && e.TargetFrameworks.Contains(tfmNet100)
        );
    }

    [Fact]
    public async Task BuildColoredGraphAsync_MissingPackageMetadata_CreatesMetadataUnavailableNode()
    {
        // Arrange
        const string repositoryPath = "/repo";
        var tfmNet90 = new NugetTargetFramework("net9.0");

        var projectQFilename = Path.Join(repositoryPath, "src", "Q", "ProjectQ.csproj");
        var fileSystem = CreateFileSystem(
            [projectQFilename],
            new Dictionary<string, string>
            {
                [projectQFilename] =
                    """
                    <Project Sdk="Microsoft.NET.Sdk">
                      <PropertyGroup>
                        <TargetFramework>net9.0</TargetFramework>
                      </PropertyGroup>
                      <ItemGroup>
                        <PackageReference Include="Q" Version="1.0.0.0" />
                      </ItemGroup>
                    </Project>
                    """,
            }
        );

        var versionFetcher = CreateVersionFetcherMock(new Dictionary<string, IReadOnlyCollection<IPackageSearchMetadata>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Q"] = [],
        });

        var pipeline = CreatePipeline(fileSystem, versionFetcher);

        var repositoryConfig = new RepositoryConfig
        {
            RepositoryPath = repositoryPath,
        };

        var context = new NugetUpdaterContext(
            [],
            [CreateDependency("src/Q/ProjectQ.csproj", "Q", "1.0.0.0", [tfmNet90])]
        );

        // Act
        var graph = await pipeline.BuildColoredGraphAsync(
            repositoryConfig,
            context,
            CreateSourceRepositories(),
            new CachingConfiguration { MaxCacheAge = null },
            TestContext.Current.CancellationToken
        );

        // Assert
        var qNode = graph.FindNode("Q", ParseVersion("1.0.0.0"));
        Assert.NotNull(qNode);
        Assert.False(qNode.IsMetadataAvailable);
    }

    private static DependencyGraphPipeline CreatePipeline(
        IFileSystem fileSystem,
        INugetVersionFetcher versionFetcher
    )
    {
        var csprojParser = new NugetCsprojParser(
            fileSystem,
            Substitute.For<ILogger<NugetCsprojParser>>(),
            new ConditionalTargetFrameworkResolver(Substitute.For<ILogger<ConditionalTargetFrameworkResolver>>())
        );

        var nugetFinder = new NugetFinder(fileSystem);
        var dependencyVersionsFetcher = new DependencyVersionsFetcher(versionFetcher);
        var graphConstructor = new DependencyGraphConstructor(
            dependencyVersionsFetcher,
            Substitute.For<ILogger<DependencyGraphConstructor>>()
        );
        var colorizer = new DependencyGraphColorizer(Substitute.For<ILogger<DependencyGraphColorizer>>());

        return new DependencyGraphPipeline(
            csprojParser,
            nugetFinder,
            graphConstructor,
            colorizer,
            Substitute.For<ILogger<DependencyGraphPipeline>>()
        );
    }

    private static IFileSystem CreateFileSystem(
        IReadOnlyCollection<string> csprojFiles,
        IReadOnlyDictionary<string, string> fileContents
    )
    {
        var fileSystem = Substitute.For<IFileSystem>();

        fileSystem
            .EnumerateFiles(Arg.Any<string>(), "*.csproj", Arg.Any<EnumerationOptions>())
            .Returns(csprojFiles);

        fileSystem
            .EnumerateFiles(Arg.Any<string>(), "Directory.Packages.props", Arg.Any<EnumerationOptions>())
            .Returns([]);

        fileSystem
            .EnumerateFiles(Arg.Any<string>(), "Nuget.Config", Arg.Any<EnumerationOptions>())
            .Returns([]);

        fileSystem
            .EnumerateFiles(Arg.Any<string>(), "dotnet-tools.json", Arg.Any<EnumerationOptions>())
            .Returns([]);

        foreach (var filePath in fileContents.Keys)
        {
            fileSystem.Exists(filePath).Returns(true);
            fileSystem
                .FileOpen(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                .Returns(_ => fileContents[filePath].AsStream());
        }

        return fileSystem;
    }

    private static INugetVersionFetcher CreateVersionFetcherMock(
        IReadOnlyDictionary<string, IReadOnlyCollection<IPackageSearchMetadata>> versionsByPackage
    )
    {
        var versionFetcher = Substitute.For<INugetVersionFetcher>();

        versionFetcher
            .FetchPackageVersionsAsync(
                Arg.Any<SourceRepository>(),
                Arg.Any<NugetDependency>(),
                Arg.Any<SourceCacheContext>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(callInfo =>
            {
                var dependency = callInfo.ArgAt<NugetDependency>(1);
                var packageName = dependency.NugetPackage.GetPackageName();
                var requestedVersion = dependency.NugetPackage.GetVersion()?.MapToNuGetVersion();

                if (!versionsByPackage.TryGetValue(packageName, out var metadata))
                {
                    return Task.FromResult<IEnumerable<IPackageSearchMetadata>>([]);
                }

                if (requestedVersion is null)
                {
                    return Task.FromResult<IEnumerable<IPackageSearchMetadata>>(metadata);
                }

                return Task.FromResult<IEnumerable<IPackageSearchMetadata>>(metadata.Select(entry =>
                {
                    if (entry is not PackageSearchMetadataRegistration registration)
                    {
                        return entry;
                    }

                    return (IPackageSearchMetadata) new PackageSearchMetadataRegistration()
                        .SetPackageId(registration.PackageId)
                        .SetVersion(requestedVersion)
                        .SetDependencySetsInternal(registration.DependencySets);
                }).ToArray());
            });

        return versionFetcher;
    }

    private static IReadOnlyDictionary<NugetSource, NugetSourceRepository> CreateSourceRepositories()
        => new Dictionary<NugetSource, NugetSourceRepository>(new NugetSourceIdentityComparer())
        {
            [DefaultNugetSource] = new(Substitute.For<SourceRepository>(), null),
        };

    private sealed class NugetSourceIdentityComparer : IEqualityComparer<NugetSource>
    {
        public bool Equals(NugetSource? x, NugetSource? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return string.Equals(x.Name, y.Name, StringComparison.Ordinal)
                   && string.Equals(x.Source, y.Source, StringComparison.Ordinal)
                   && x.Version == y.Version;
        }

        public int GetHashCode(NugetSource obj)
            => HashCode.Combine(obj.Name, obj.Source, obj.Version);
    }

    private static NugetDependency CreateDependency(
        string relativePath,
        string packageName,
        string packageVersion,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks
    ) => new(
        new NugetFile(relativePath, ENugetFileType.Csproj),
        new NugetPackageVersion(packageName, NuGetVersion.Parse(packageVersion)),
        targetFrameworks
    );

    private static PackageVersion ParseVersion(string version)
        => NuGetVersion.Parse(version).MapToPackageVersion();

    private static IPackageSearchMetadata CreateMetadata(
        string packageName,
        string version,
        params (string TargetFramework, IReadOnlyCollection<(string Id, string Version)> Dependencies)[] dependencySets
    ) => new PackageSearchMetadataRegistration()
        .SetPackageId(packageName)
        .SetVersion(new NuGetVersion(Version.Parse(version)))
        .SetDependencySetsInternal(dependencySets.Select(set =>
            new PackageDependencyGroup(
                NuGetFramework.ParseFolder(set.TargetFramework),
                set.Dependencies.Select(x => new PackageDependency(x.Id, new VersionRange(new NuGetVersion(Version.Parse(x.Version))))).ToList()
            )
        ).ToList());
}
