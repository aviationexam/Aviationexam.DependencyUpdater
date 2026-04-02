using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;
using Aviationexam.DependencyUpdater.Nuget.Helpers;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Parsers;
using Aviationexam.DependencyUpdater.TestsInfrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.IO;
using System.Linq;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests.DependencyGraph;

public sealed class ProjectReferenceParsingTests(
    ITestOutputHelper outputHelper
)
{
    private readonly ILoggerFactory _loggerFactory = outputHelper.ToLoggerFactory();

    [Fact]
    public void ParseProjectReferences_BasicProjectReference_ExtractsCorrectly()
    {
        // Arrange
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            _loggerFactory.CreateLogger<TemporaryDirectoryProvider>()
        );

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetCsprojParser>>();

        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath("project/A.csproj"))
            .Returns(true);

        using var projectFileStream =
            // language=csproj
            """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net9.0</TargetFramework>
                  </PropertyGroup>

                  <ItemGroup>
                    <ProjectReference Include="../B/B.csproj" />
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("project/A.csproj"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        var csprojParser = new NugetCsprojParser(
            fileSystem,
            logger,
            new ConditionalTargetFrameworkResolver(
                _loggerFactory.CreateLogger<ConditionalTargetFrameworkResolver>()
            )
        );

        var nugetFile = new NugetFile("project/A.csproj", ENugetFileType.Csproj);

        // Act
        var response = csprojParser.ParseProjectReferences(temporaryDirectoryProvider.TemporaryDirectory, nugetFile).ToList();

        // Assert
        Assert.Collection(response,
            r =>
            {
                Assert.Equal("B", r.ProjectName);
                Assert.Equal("../B/B.csproj", r.RelativePath);
                Assert.Null(r.Condition);
                Assert.Equal([new NugetTargetFramework("net9.0")], r.TargetFrameworks);
            }
        );
    }

    [Fact]
    public void ParseProjectReferences_MultipleProjectReferences_ExtractsAll()
    {
        // Arrange
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            _loggerFactory.CreateLogger<TemporaryDirectoryProvider>()
        );

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetCsprojParser>>();

        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath("project/A.csproj"))
            .Returns(true);

        using var projectFileStream =
            // language=csproj
            """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net9.0</TargetFramework>
                  </PropertyGroup>

                  <ItemGroup>
                    <ProjectReference Include="../B/B.csproj" />
                    <ProjectReference Include="../C/C.csproj" />
                    <ProjectReference Include="../D/D.csproj" />
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("project/A.csproj"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        var csprojParser = new NugetCsprojParser(
            fileSystem,
            logger,
            new ConditionalTargetFrameworkResolver(
                _loggerFactory.CreateLogger<ConditionalTargetFrameworkResolver>()
            )
        );

        var nugetFile = new NugetFile("project/A.csproj", ENugetFileType.Csproj);

        // Act
        var response = csprojParser.ParseProjectReferences(temporaryDirectoryProvider.TemporaryDirectory, nugetFile).ToList();

        // Assert
        Assert.Collection(response,
            r =>
            {
                Assert.Equal("B", r.ProjectName);
                Assert.Equal("../B/B.csproj", r.RelativePath);
                Assert.Null(r.Condition);
                Assert.Equal([new NugetTargetFramework("net9.0")], r.TargetFrameworks);
            },
            r =>
            {
                Assert.Equal("C", r.ProjectName);
                Assert.Equal("../C/C.csproj", r.RelativePath);
                Assert.Null(r.Condition);
                Assert.Equal([new NugetTargetFramework("net9.0")], r.TargetFrameworks);
            },
            r =>
            {
                Assert.Equal("D", r.ProjectName);
                Assert.Equal("../D/D.csproj", r.RelativePath);
                Assert.Null(r.Condition);
                Assert.Equal([new NugetTargetFramework("net9.0")], r.TargetFrameworks);
            }
        );
    }

    [Fact]
    public void ParseProjectReferences_ConditionalProjectReference_ScopedToTfm()
    {
        // Arrange
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            _loggerFactory.CreateLogger<TemporaryDirectoryProvider>()
        );

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetCsprojParser>>();

        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath("project/A.csproj"))
            .Returns(true);

        using var projectFileStream =
            // language=csproj
            """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
                  </PropertyGroup>

                  <ItemGroup>
                    <ProjectReference Condition="'$(TargetFramework)' == 'net9.0'" Include="../C/C.csproj" />
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("project/A.csproj"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        var csprojParser = new NugetCsprojParser(
            fileSystem,
            logger,
            new ConditionalTargetFrameworkResolver(
                _loggerFactory.CreateLogger<ConditionalTargetFrameworkResolver>()
            )
        );

        var nugetFile = new NugetFile("project/A.csproj", ENugetFileType.Csproj);

        // Act
        var response = csprojParser.ParseProjectReferences(temporaryDirectoryProvider.TemporaryDirectory, nugetFile).ToList();

        // Assert
        Assert.Collection(response,
            r =>
            {
                Assert.Equal("C", r.ProjectName);
                Assert.Equal("../C/C.csproj", r.RelativePath);
                Assert.Equal("net9.0", r.Condition);
                Assert.Equal([new NugetTargetFramework("net9.0")], r.TargetFrameworks);
            }
        );
    }

    [Fact]
    public void ParseProjectReferences_NoProjectReferences_ReturnsEmpty()
    {
        // Arrange
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            _loggerFactory.CreateLogger<TemporaryDirectoryProvider>()
        );

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetCsprojParser>>();

        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath("project/A.csproj"))
            .Returns(true);

        using var projectFileStream =
            // language=csproj
            """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net9.0</TargetFramework>
                  </PropertyGroup>

                  <ItemGroup>
                    <PackageReference Include="Microsoft.Extensions.Hosting" />
                    <PackageReference Include="Microsoft.Extensions.Logging.Console" />
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("project/A.csproj"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        var csprojParser = new NugetCsprojParser(
            fileSystem,
            logger,
            new ConditionalTargetFrameworkResolver(
                _loggerFactory.CreateLogger<ConditionalTargetFrameworkResolver>()
            )
        );

        var nugetFile = new NugetFile("project/A.csproj", ENugetFileType.Csproj);

        // Act
        var response = csprojParser.ParseProjectReferences(temporaryDirectoryProvider.TemporaryDirectory, nugetFile).ToList();

        // Assert
        Assert.Empty(response);
    }

    [Fact]
    public void ParseProjectReferences_MixedContent_ExtractsOnlyProjectReferences()
    {
        // Arrange
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            _loggerFactory.CreateLogger<TemporaryDirectoryProvider>()
        );

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetCsprojParser>>();

        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath("project/A.csproj"))
            .Returns(true);

        using var projectFileStream =
            // language=csproj
            """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net9.0</TargetFramework>
                  </PropertyGroup>

                  <ItemGroup>
                    <PackageReference Include="Microsoft.Extensions.Hosting" />
                    <PackageReference Include="Microsoft.Extensions.Logging.Console" />
                  </ItemGroup>

                  <ItemGroup>
                    <ProjectReference Include="../B/B.csproj" />
                    <ProjectReference Include="../C/C.csproj" />
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("project/A.csproj"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        var csprojParser = new NugetCsprojParser(
            fileSystem,
            logger,
            new ConditionalTargetFrameworkResolver(
                _loggerFactory.CreateLogger<ConditionalTargetFrameworkResolver>()
            )
        );

        var nugetFile = new NugetFile("project/A.csproj", ENugetFileType.Csproj);

        // Act
        var response = csprojParser.ParseProjectReferences(temporaryDirectoryProvider.TemporaryDirectory, nugetFile).ToList();

        // Assert
        Assert.Collection(response,
            r =>
            {
                Assert.Equal("B", r.ProjectName);
                Assert.Equal("../B/B.csproj", r.RelativePath);
                Assert.Null(r.Condition);
                Assert.Equal([new NugetTargetFramework("net9.0")], r.TargetFrameworks);
            },
            r =>
            {
                Assert.Equal("C", r.ProjectName);
                Assert.Equal("../C/C.csproj", r.RelativePath);
                Assert.Null(r.Condition);
                Assert.Equal([new NugetTargetFramework("net9.0")], r.TargetFrameworks);
            }
        );
    }

    [Fact]
    public void ParseProjectReferences_WithChildMetadata_StillExtracted()
    {
        // Arrange
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            _loggerFactory.CreateLogger<TemporaryDirectoryProvider>()
        );

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetCsprojParser>>();

        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath("project/A.csproj"))
            .Returns(true);

        using var projectFileStream =
            // language=csproj
            """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net9.0</TargetFramework>
                  </PropertyGroup>

                  <ItemGroup>
                    <ProjectReference Include="../D/D.csproj">
                      <PrivateAssets>all</PrivateAssets>
                    </ProjectReference>
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("project/A.csproj"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        var csprojParser = new NugetCsprojParser(
            fileSystem,
            logger,
            new ConditionalTargetFrameworkResolver(
                _loggerFactory.CreateLogger<ConditionalTargetFrameworkResolver>()
            )
        );

        var nugetFile = new NugetFile("project/A.csproj", ENugetFileType.Csproj);

        // Act
        var response = csprojParser.ParseProjectReferences(temporaryDirectoryProvider.TemporaryDirectory, nugetFile).ToList();

        // Assert
        Assert.Collection(response,
            r =>
            {
                Assert.Equal("D", r.ProjectName);
                Assert.Equal("../D/D.csproj", r.RelativePath);
                Assert.Null(r.Condition);
                Assert.Equal([new NugetTargetFramework("net9.0")], r.TargetFrameworks);
            }
        );
    }
}
