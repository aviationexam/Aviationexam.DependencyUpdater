using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Parsers;
using Aviationexam.DependencyUpdater.TestsInfrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.IO;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class NugetCsprojParserTests
{
    [Fact]
    public void ParseWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            NullLoggerFactory.Instance.CreateLogger<TemporaryDirectoryProvider>()
            );

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetCsprojParser>>();

        Directory.CreateDirectory(temporaryDirectoryProvider.GetPath("project"));
        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath("project/Project.csproj"))
            .Returns(true);

        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath("WarningConfiguration.targets"))
            .Returns(true);

        using var projectFileStream =
            // language=csproj
            """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net9.0</TargetFramework>
                  </PropertyGroup>

                  <Import Project="..\WarningConfiguration.targets" />

                  <ItemGroup>
                    <PackageReference Include="Microsoft.Extensions.Hosting" />
                    <PackageReference Include="Microsoft.Extensions.Logging.Console" />
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("project/Project.csproj"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        using var warningConfigurationStream =
            // language=targets
            """
                <Project>

                  <ItemGroup>
                    <PackageReference Include="Meziantou.Analyzer">
                      <PrivateAssets>all</PrivateAssets>
                      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
                    </PackageReference>
                  </ItemGroup>

                </Project>
                """.AsStream(temporaryDirectoryProvider.GetPath("WarningConfiguration.targets"));

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("WarningConfiguration.targets"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(warningConfigurationStream);

        var csprojParser = new NugetCsprojParser(
            fileSystem,
            logger
        );

        var nugetFile = new NugetFile("project/Project.csproj", ENugetFileType.Csproj);
        var warningConfigurationFile = new NugetFile("WarningConfiguration.targets", ENugetFileType.Targets);
        var response = csprojParser.Parse(temporaryDirectoryProvider.TemporaryDirectory, nugetFile);

        Assert.Equal([
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.Extensions.Hosting", null), [
                new NugetTargetFramework("net9.0"),
            ]),
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.Extensions.Logging.Console", null), [
                new NugetTargetFramework("net9.0"),
            ]),
            new NugetDependency(warningConfigurationFile, new NugetPackageReference("Meziantou.Analyzer", null), [
                new NugetTargetFramework("net9.0"),
            ]),
        ], response);
    }

    [Fact]
    public void ParseWorks_MultipleTargetFrameworks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            NullLoggerFactory.Instance.CreateLogger<TemporaryDirectoryProvider>()
            );

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetCsprojParser>>();

        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath("project/Project.csproj"))
            .Returns(true);

        using var projectFileStream =
            // language=csproj
            """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
                  </PropertyGroup>

                  <ItemGroup>
                    <PackageReference Include="Microsoft.Extensions.Hosting" />
                    <PackageReference Include="Microsoft.Extensions.Logging.Console" />
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("project/Project.csproj"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        var csprojParser = new NugetCsprojParser(
            fileSystem,
            logger
        );

        var nugetFile = new NugetFile("project/Project.csproj", ENugetFileType.Csproj);
        var response = csprojParser.Parse(temporaryDirectoryProvider.TemporaryDirectory, nugetFile);

        Assert.Equal([
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.Extensions.Hosting", null), [
                new NugetTargetFramework("net8.0"),
                new NugetTargetFramework("net9.0"),
            ]),
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.Extensions.Logging.Console", null), [
                new NugetTargetFramework("net8.0"),
                new NugetTargetFramework("net9.0"),
            ]),
        ], response);
    }

    [Fact]
    public void ParseFails()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetCsprojParser>>();

        fileSystem
            .Exists($"{directoryPath}/project/Project.csproj")
            .Returns(false);

        var csprojParser = new NugetCsprojParser(
            fileSystem,
            logger
        );

        var response = csprojParser.Parse(directoryPath, new NugetFile("project/Project.csproj", ENugetFileType.Csproj));

        Assert.Empty(response);
    }
}
