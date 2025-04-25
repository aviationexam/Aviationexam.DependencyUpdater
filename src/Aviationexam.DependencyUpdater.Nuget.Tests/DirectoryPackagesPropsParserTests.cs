using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.IO;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class DirectoryPackagesPropsParserTests
{
    [Fact]
    public void ParseWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider();

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<DirectoryPackagesPropsParser>>();

        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath("Directory.Packages.props"))
            .Returns(true);

        using var directoryPackagesPropsStream =
            // language=props
            """
                <Project>
                  <PropertyGroup>
                    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
                  </PropertyGroup>
                  <ItemGroup>
                    <PackageVersion Include="Meziantou.Analyzer" Version="2.0.195" />
                    <PackageVersion Include="Microsoft.Extensions.Hosting" Version="9.0.4" />
                    <PackageVersion Include="Microsoft.Extensions.Logging.Console" Version="9.0.4" />
                    <PackageVersion Include="NuGet.ProjectModel" Version="6.13.2" />
                    <PackageVersion Include="System.Text.Json" Version="9.0.4" />
                    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.4" />
                    <PackageVersion Include="Microsoft.Build" Version="17.13.9" />
                    <PackageVersion Include="Microsoft.Build.Locator" Version="1.9.1" />
                  </ItemGroup>
                  <ItemGroup>
                    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.13.0" />
                    <PackageVersion Include="xunit.v3" Version="2.0.1" />
                    <PackageVersion Include="NSubstitute" Version="5.3.0" />
                    <PackageVersion Include="NSubstitute.Analyzers.CSharp" Version="1.0.17" />
                  </ItemGroup>
                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("Directory.Packages.props"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(directoryPackagesPropsStream);

        var directoryPackagesPropsParser = new DirectoryPackagesPropsParser(
            fileSystem,
            logger
        );

        var nugetFile = new NugetFile(temporaryDirectoryProvider.GetPath("Directory.Packages.props"), ENugetFileType.DirectoryPackagesProps);
        var response = directoryPackagesPropsParser.Parse(nugetFile);

        Assert.Equal([
            new NugetDependency(nugetFile, new NugetPackageVersion("Meziantou.Analyzer", "2.0.195")),
            new NugetDependency(nugetFile, new NugetPackageVersion("Microsoft.Extensions.Hosting", "9.0.4")),
            new NugetDependency(nugetFile, new NugetPackageVersion("Microsoft.Extensions.Logging.Console", "9.0.4")),
            new NugetDependency(nugetFile, new NugetPackageVersion("NuGet.ProjectModel", "6.13.2")),
            new NugetDependency(nugetFile, new NugetPackageVersion("System.Text.Json", "9.0.4")),
            new NugetDependency(nugetFile, new NugetPackageVersion("Microsoft.Extensions.Logging.Abstractions", "9.0.4")),
            new NugetDependency(nugetFile, new NugetPackageVersion("Microsoft.Build", "17.13.9")),
            new NugetDependency(nugetFile, new NugetPackageVersion("Microsoft.Build.Locator", "1.9.1")),
            new NugetDependency(nugetFile, new NugetPackageVersion("Microsoft.NET.Test.Sdk", "17.13.0")),
            new NugetDependency(nugetFile, new NugetPackageVersion("xunit.v3", "2.0.1")),
            new NugetDependency(nugetFile, new NugetPackageVersion("NSubstitute", "5.3.0")),
            new NugetDependency(nugetFile, new NugetPackageVersion("NSubstitute.Analyzers.CSharp", "1.0.17")),
        ], response);
    }

    [Fact]
    public void ParseFails()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<DirectoryPackagesPropsParser>>();

        fileSystem
            .Exists($"{directoryPath}/Directory.Packages.props")
            .Returns(false);

        var directoryPackagesPropsParser = new DirectoryPackagesPropsParser(
            fileSystem,
            logger
        );

        var response = directoryPackagesPropsParser.Parse(new NugetFile($"{directoryPath}/Directory.Packages.props", ENugetFileType.Csproj));

        Assert.Empty(response);
    }
}
