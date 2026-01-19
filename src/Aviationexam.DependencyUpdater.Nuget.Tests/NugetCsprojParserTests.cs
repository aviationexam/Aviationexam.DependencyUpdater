using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Helpers;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Parsers;
using Aviationexam.DependencyUpdater.TestsInfrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.IO;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class NugetCsprojParserTests(
    ITestOutputHelper outputHelper
)
{
    private readonly ILoggerFactory _loggerFactory = outputHelper.ToLoggerFactory();

    [Fact]
    public void ParseWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            _loggerFactory.CreateLogger<TemporaryDirectoryProvider>()
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
            logger,
            new ConditionalTargetFrameworkResolver(_loggerFactory.CreateLogger<ConditionalTargetFrameworkResolver>())
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
            _loggerFactory.CreateLogger<TemporaryDirectoryProvider>()
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
            logger,
            new ConditionalTargetFrameworkResolver(_loggerFactory.CreateLogger<ConditionalTargetFrameworkResolver>())
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
            logger,
            new ConditionalTargetFrameworkResolver(_loggerFactory.CreateLogger<ConditionalTargetFrameworkResolver>())
        );

        var response = csprojParser.Parse(directoryPath, new NugetFile("project/Project.csproj", ENugetFileType.Csproj));

        Assert.Empty(response);
    }

    [Fact]
    public void ParseConditionalItemGroupWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            _loggerFactory.CreateLogger<TemporaryDirectoryProvider>()
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

                  <ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
                    <PackageReference Include="Microsoft.AspNetCore.WebUtilities" Version="9.0.0" />
                  </ItemGroup>

                  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
                    <PackageReference Include="Microsoft.AspNetCore.WebUtilities" Version="8.0.0" />
                  </ItemGroup>

                  <ItemGroup>
                    <PackageReference Include="Microsoft.Extensions.Hosting" />
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("project/Project.csproj"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        var csprojParser = new NugetCsprojParser(
            fileSystem,
            logger,
            new ConditionalTargetFrameworkResolver(_loggerFactory.CreateLogger<ConditionalTargetFrameworkResolver>())
        );

        var nugetFile = new NugetFile("project/Project.csproj", ENugetFileType.Csproj);
        var response = csprojParser.Parse(temporaryDirectoryProvider.TemporaryDirectory, nugetFile);

        Assert.Equal([
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.AspNetCore.WebUtilities", new NuGet.Versioning.VersionRange(new NuGet.Versioning.NuGetVersion("9.0.0")), "net9.0"), [
                new NugetTargetFramework("net9.0"),
            ]),
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.AspNetCore.WebUtilities", new NuGet.Versioning.VersionRange(new NuGet.Versioning.NuGetVersion("8.0.0")), "net8.0"), [
                new NugetTargetFramework("net8.0"),
            ]),
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.Extensions.Hosting", null), [
                new NugetTargetFramework("net8.0"),
                new NugetTargetFramework("net9.0"),
            ]),
        ], response);
    }

    [Fact]
    public void ParseConditionalPackageReferenceElementWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            _loggerFactory.CreateLogger<TemporaryDirectoryProvider>()
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
                    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" Condition="'$(TargetFramework)' == 'net9.0'" />
                    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" Condition="'$(TargetFramework)' == 'net8.0'" />
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("project/Project.csproj"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        var csprojParser = new NugetCsprojParser(
            fileSystem,
            logger,
            new ConditionalTargetFrameworkResolver(_loggerFactory.CreateLogger<ConditionalTargetFrameworkResolver>())
        );

        var nugetFile = new NugetFile("project/Project.csproj", ENugetFileType.Csproj);
        var response = csprojParser.Parse(temporaryDirectoryProvider.TemporaryDirectory, nugetFile);

        Assert.Equal([
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.Extensions.Hosting", new NuGet.Versioning.VersionRange(new NuGet.Versioning.NuGetVersion("9.0.0")), "net9.0"), [
                new NugetTargetFramework("net9.0"),
            ]),
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.Extensions.Hosting", new NuGet.Versioning.VersionRange(new NuGet.Versioning.NuGetVersion("8.0.0")), "net8.0"), [
                new NugetTargetFramework("net8.0"),
            ]),
        ], response);
    }

    [Fact]
    public void ParseMixedConditionalFormatsWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            _loggerFactory.CreateLogger<TemporaryDirectoryProvider>()
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

                  <!-- Conditional ItemGroup -->
                  <ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
                    <PackageReference Include="Microsoft.AspNetCore.WebUtilities" Version="9.0.0" />
                  </ItemGroup>

                  <!-- Conditional PackageReference element -->
                  <ItemGroup>
                    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.4" Condition="'$(TargetFramework)' == 'net9.0'" />
                    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.10" Condition="'$(TargetFramework)' == 'net8.0'" />
                  </ItemGroup>

                  <!-- Unconditional -->
                  <ItemGroup>
                    <PackageReference Include="Meziantou.Analyzer" Version="2.0.177" />
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("project/Project.csproj"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        var csprojParser = new NugetCsprojParser(
            fileSystem,
            logger,
            new ConditionalTargetFrameworkResolver(_loggerFactory.CreateLogger<ConditionalTargetFrameworkResolver>())
        );

        var nugetFile = new NugetFile("project/Project.csproj", ENugetFileType.Csproj);
        var response = csprojParser.Parse(temporaryDirectoryProvider.TemporaryDirectory, nugetFile);

        Assert.Equal([
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.AspNetCore.WebUtilities", new NuGet.Versioning.VersionRange(new NuGet.Versioning.NuGetVersion("9.0.0")), "net9.0"), [
                new NugetTargetFramework("net9.0"),
            ]),
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.Extensions.Hosting", new NuGet.Versioning.VersionRange(new NuGet.Versioning.NuGetVersion("9.0.4")), "net9.0"), [
                new NugetTargetFramework("net9.0"),
            ]),
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.Extensions.Hosting", new NuGet.Versioning.VersionRange(new NuGet.Versioning.NuGetVersion("8.0.10")), "net8.0"), [
                new NugetTargetFramework("net8.0"),
            ]),
            new NugetDependency(nugetFile, new NugetPackageReference("Meziantou.Analyzer", new NuGet.Versioning.VersionRange(new NuGet.Versioning.NuGetVersion("2.0.177"))), [
                new NugetTargetFramework("net8.0"),
                new NugetTargetFramework("net9.0"),
            ]),
        ], response);
    }

    [Fact]
    public void ParseConditionalWithUnquotedVariableWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            _loggerFactory.CreateLogger<TemporaryDirectoryProvider>()
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
                    <TargetFrameworks>netstandard2.0;net8.0</TargetFrameworks>
                  </PropertyGroup>

                  <ItemGroup Condition="$(TargetFramework) == 'netstandard2.0'">
                    <PackageReference Include="Microsoft.Bcl.AsyncInterfaces" Version="9.0.0" />
                  </ItemGroup>

                  <ItemGroup>
                    <PackageReference Include="System.Text.Json" Version="9.0.1" Condition="$(TargetFramework) == 'netstandard2.0'" />
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("project/Project.csproj"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        var csprojParser = new NugetCsprojParser(
            fileSystem,
            logger,
            new ConditionalTargetFrameworkResolver(_loggerFactory.CreateLogger<ConditionalTargetFrameworkResolver>())
        );

        var nugetFile = new NugetFile("project/Project.csproj", ENugetFileType.Csproj);
        var response = csprojParser.Parse(temporaryDirectoryProvider.TemporaryDirectory, nugetFile);

        Assert.Equal([
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.Bcl.AsyncInterfaces", new NuGet.Versioning.VersionRange(new NuGet.Versioning.NuGetVersion("9.0.0")), "netstandard2.0"),
            [
                new NugetTargetFramework("netstandard2.0"),
            ]),
            new NugetDependency(nugetFile, new NugetPackageReference("System.Text.Json", new NuGet.Versioning.VersionRange(new NuGet.Versioning.NuGetVersion("9.0.1")), "netstandard2.0"), [
                new NugetTargetFramework("netstandard2.0"),
            ]),
        ], response);
    }
}
