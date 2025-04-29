using Microsoft.Extensions.Logging;
using NSubstitute;
using NuGet.Versioning;
using System.Collections.Generic;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class NugetUpdaterContextExtensionsTests
{
    [Fact]
    public void MapSourceToDependencyWorks_NoConfigurations()
    {
        var logger = Substitute.For<ILogger>();

        NugetDependency dependency1;

        var nugetUpdaterContext = new NugetUpdaterContext(
            [],
            [
                dependency1 = new NugetDependency(
                    new NugetFile("AProject.csproj", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Logging.Abstractions", new VersionRange(new NuGetVersion("9.0.0")))
                ),
            ]
        );

        Assert.Equal([
            KeyValuePair.Create<NugetDependency, IReadOnlyCollection<NugetSource>>(
                dependency1,
                [new NugetSource("nuget.org", "https://api.nuget.org/v3/index.json", NugetSourceVersion.V3, [])]
            ),
        ], nugetUpdaterContext.MapSourceToDependency(logger));
    }

    [Fact]
    public void MapSourceToDependencyWorks_ConfigurationWithoutSourceMapping()
    {
        var logger = Substitute.For<ILogger>();

        NugetSource nugetFeed1;
        NugetDependency dependency1;

        var nugetUpdaterContext = new NugetUpdaterContext(
            [
                nugetFeed1 = new NugetSource("my-nuget-feed", "https://api.nuget.org/v3/index.json", NugetSourceVersion.V3, []),
            ],
            [
                dependency1 = new NugetDependency(
                    new NugetFile("AProject.csproj", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Logging.Abstractions", new VersionRange(new NuGetVersion("9.0.0")))
                ),
            ]
        );

        Assert.Equal([
            KeyValuePair.Create<NugetDependency, IReadOnlyCollection<NugetSource>>(
                dependency1,
                [nugetFeed1]
            ),
        ], nugetUpdaterContext.MapSourceToDependency(logger));
    }

    [Fact]
    public void MapSourceToDependencyWorks_ConfigurationWithoutSourceMapping_MultipleSources()
    {
        var logger = Substitute.For<ILogger>();

        NugetSource nugetFeed1, nugetFeed2;
        NugetDependency dependency1;

        var nugetUpdaterContext = new NugetUpdaterContext(
            [
                nugetFeed1 = new NugetSource("my-nuget-feed", "https://api.nuget.org/v3/index.json", NugetSourceVersion.V3, []),
                nugetFeed2 = new NugetSource("another-nuget-feed", "https://api.nuget.org/v3/index.json", NugetSourceVersion.V3, []),
            ],
            [
                dependency1 = new NugetDependency(
                    new NugetFile("AProject.csproj", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Logging.Abstractions", new VersionRange(new NuGetVersion("9.0.0")))
                ),
            ]
        );

        Assert.Equal([
            KeyValuePair.Create<NugetDependency, IReadOnlyCollection<NugetSource>>(
                dependency1,
                [nugetFeed1, nugetFeed2]
            ),
        ], nugetUpdaterContext.MapSourceToDependency(logger));
    }

    [Fact]
    public void MapSourceToDependencyWorks_ConfigurationWithSourceMapping()
    {
        var logger = Substitute.For<ILogger>();

        NugetSource nugetFeed1, nugetFeed2, nugetFeed3;
        NugetDependency dependency1, dependency2, dependency3, dependency4, dependency5;

        var nugetUpdaterContext = new NugetUpdaterContext(
            [
                nugetFeed1 = new NugetSource("my-nuget-feed", "https://api.nuget.org/v3/index.json", NugetSourceVersion.V3, [
                    new NugetPackageSourceMap("Microsoft.*"),
                    new NugetPackageSourceMap("System.*"),
                ]),
                nugetFeed2 = new NugetSource("another-nuget-feed", "https://api.nuget.org/v3/index.json", NugetSourceVersion.V3, [
                    new NugetPackageSourceMap("My.Namespace.*"),
                    new NugetPackageSourceMap("Microsoft.Extensions.Logging"),
                ]),
                nugetFeed3 = new NugetSource("wild-nuget-feed", "https://api.nuget.org/v3/index.json", NugetSourceVersion.V3, [
                    new NugetPackageSourceMap("*"),
                ]),
            ],
            [
                dependency1 = new NugetDependency(
                    new NugetFile("AProject.csproj", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Logging.Abstractions", new VersionRange(new NuGetVersion("9.0.0")))
                ),
                dependency2 = new NugetDependency(
                    new NugetFile("AProject.csproj", ENugetFileType.Csproj),
                    new NugetPackageReference("System.Memory", new VersionRange(new NuGetVersion("4.6.3")))
                ),
                dependency3 = new NugetDependency(
                    new NugetFile("AProject.csproj", ENugetFileType.Csproj),
                    new NugetPackageReference("My.Namespace.Core", new VersionRange(new NuGetVersion("1.0.0")))
                ),
                dependency4 = new NugetDependency(
                    new NugetFile("AProject.csproj", ENugetFileType.Csproj),
                    new NugetPackageReference("My.NamespaceIsNot", new VersionRange(new NuGetVersion("1.0.0")))
                ),
                dependency5 = new NugetDependency(
                    new NugetFile("AProject.csproj", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Logging", new VersionRange(new NuGetVersion("9.0.0")))
                ),
            ]
        );

        Assert.Equal([
            KeyValuePair.Create<NugetDependency, IReadOnlyCollection<NugetSource>>(
                dependency1,
                [nugetFeed1]
            ),
            KeyValuePair.Create<NugetDependency, IReadOnlyCollection<NugetSource>>(
                dependency2,
                [nugetFeed1]
            ),
            KeyValuePair.Create<NugetDependency, IReadOnlyCollection<NugetSource>>(
                dependency3,
                [nugetFeed2]
            ),
            KeyValuePair.Create<NugetDependency, IReadOnlyCollection<NugetSource>>(
                dependency4,
                [nugetFeed3]
            ),
            KeyValuePair.Create<NugetDependency, IReadOnlyCollection<NugetSource>>(
                dependency5,
                [nugetFeed2]
            ),
        ], nugetUpdaterContext.MapSourceToDependency(logger));
    }
}
