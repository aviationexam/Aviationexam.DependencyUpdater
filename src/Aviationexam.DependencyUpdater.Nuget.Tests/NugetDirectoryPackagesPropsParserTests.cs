using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Parsers;
using Aviationexam.DependencyUpdater.TestsInfrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Xunit;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class NugetDirectoryPackagesPropsParserTests
{
    [Fact]
    public void ParseWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            NullLoggerFactory.Instance.CreateLogger<TemporaryDirectoryProvider>()
        );

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetDirectoryPackagesPropsParser>>();

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

        var directoryPackagesPropsParser = new NugetDirectoryPackagesPropsParser(
            fileSystem,
            logger
        );

        IReadOnlyCollection<NugetTargetFramework> targetFrameworks = [new("net9.0")];

        var nugetFile = new NugetFile("Directory.Packages.props", ENugetFileType.DirectoryPackagesProps);
        var response = directoryPackagesPropsParser.Parse(
            temporaryDirectoryProvider.TemporaryDirectory,
            nugetFile,
            ImmutableDictionary<string, IReadOnlyCollection<NugetTargetFramework>>.Empty,
            targetFrameworks
        );

        Assert.Equal([
            new NugetDependency(nugetFile, new NugetPackageVersion("Meziantou.Analyzer", "2.0.195"), targetFrameworks),
            new NugetDependency(nugetFile, new NugetPackageVersion("Microsoft.Extensions.Hosting", "9.0.4"), targetFrameworks),
            new NugetDependency(nugetFile, new NugetPackageVersion("Microsoft.Extensions.Logging.Console", "9.0.4"), targetFrameworks),
            new NugetDependency(nugetFile, new NugetPackageVersion("NuGet.ProjectModel", "6.13.2"), targetFrameworks),
            new NugetDependency(nugetFile, new NugetPackageVersion("System.Text.Json", "9.0.4"), targetFrameworks),
            new NugetDependency(nugetFile, new NugetPackageVersion("Microsoft.Extensions.Logging.Abstractions", "9.0.4"), targetFrameworks),
            new NugetDependency(nugetFile, new NugetPackageVersion("Microsoft.Build", "17.13.9"), targetFrameworks),
            new NugetDependency(nugetFile, new NugetPackageVersion("Microsoft.Build.Locator", "1.9.1"), targetFrameworks),
            new NugetDependency(nugetFile, new NugetPackageVersion("Microsoft.NET.Test.Sdk", "17.13.0"), targetFrameworks),
            new NugetDependency(nugetFile, new NugetPackageVersion("xunit.v3", "2.0.1"), targetFrameworks),
            new NugetDependency(nugetFile, new NugetPackageVersion("NSubstitute", "5.3.0"), targetFrameworks),
            new NugetDependency(nugetFile, new NugetPackageVersion("NSubstitute.Analyzers.CSharp", "1.0.17"), targetFrameworks),
        ], response);
    }

    [Fact]
    public void ParseFails()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetDirectoryPackagesPropsParser>>();

        fileSystem
            .Exists($"{directoryPath}/Directory.Packages.props")
            .Returns(false);

        var directoryPackagesPropsParser = new NugetDirectoryPackagesPropsParser(
            fileSystem,
            logger
        );

        var response = directoryPackagesPropsParser.Parse(
            directoryPath,
            new NugetFile("Directory.Packages.props", ENugetFileType.Csproj),
            ImmutableDictionary<string, IReadOnlyCollection<NugetTargetFramework>>.Empty,
            []
        );

        Assert.Empty(response);
    }

    [Fact]
    public void ParseConditionalItemGroupWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            NullLoggerFactory.Instance.CreateLogger<TemporaryDirectoryProvider>()
        );

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetDirectoryPackagesPropsParser>>();

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
                  </ItemGroup>
                  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
                    <PackageVersion Include="Microsoft.AspNetCore.WebUtilities" Version="8.0.22" />
                    <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="8.0.1" />
                  </ItemGroup>
                  <ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
                    <PackageVersion Include="Microsoft.AspNetCore.WebUtilities" Version="9.0.11" />
                    <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="9.0.11" />
                  </ItemGroup>
                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("Directory.Packages.props"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(directoryPackagesPropsStream);

        var directoryPackagesPropsParser = new NugetDirectoryPackagesPropsParser(
            fileSystem,
            logger
        );

        IReadOnlyCollection<NugetTargetFramework> targetFrameworks = [new("net8.0"), new("net9.0")];

        var nugetFile = new NugetFile("Directory.Packages.props", ENugetFileType.DirectoryPackagesProps);
        var response = directoryPackagesPropsParser.Parse(
            temporaryDirectoryProvider.TemporaryDirectory,
            nugetFile,
            ImmutableDictionary<string, IReadOnlyCollection<NugetTargetFramework>>.Empty,
            targetFrameworks
        );

        var actualList = response.AsValueEnumerable().ToList();

        Assert.Equal(5, actualList.Count);

        // First package - no condition, should have all target frameworks
        Assert.Equal("Meziantou.Analyzer", actualList[0].NugetPackage.GetPackageName());
        Assert.Equal("2.0.195", ((NugetPackageVersion) actualList[0].NugetPackage).Version.ToString());
        Assert.Equal(2, actualList[0].TargetFrameworks.Count);

        // Conditional packages for net8.0
        Assert.Equal("Microsoft.AspNetCore.WebUtilities", actualList[1].NugetPackage.GetPackageName());
        Assert.Equal("8.0.22", ((NugetPackageVersion) actualList[1].NugetPackage).Version.ToString());
        Assert.Single(actualList[1].TargetFrameworks);
        Assert.Equal("net8.0", actualList[1].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);

        Assert.Equal("Microsoft.Extensions.DependencyInjection", actualList[2].NugetPackage.GetPackageName());
        Assert.Equal("8.0.1", ((NugetPackageVersion) actualList[2].NugetPackage).Version.ToString());
        Assert.Single(actualList[2].TargetFrameworks);
        Assert.Equal("net8.0", actualList[2].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);

        // Conditional packages for net9.0
        Assert.Equal("Microsoft.AspNetCore.WebUtilities", actualList[3].NugetPackage.GetPackageName());
        Assert.Equal("9.0.11", ((NugetPackageVersion) actualList[3].NugetPackage).Version.ToString());
        Assert.Single(actualList[3].TargetFrameworks);
        Assert.Equal("net9.0", actualList[3].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);

        Assert.Equal("Microsoft.Extensions.DependencyInjection", actualList[4].NugetPackage.GetPackageName());
        Assert.Equal("9.0.11", ((NugetPackageVersion) actualList[4].NugetPackage).Version.ToString());
        Assert.Single(actualList[4].TargetFrameworks);
        Assert.Equal("net9.0", actualList[4].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);
    }

    [Fact]
    public void ParseConditionalPackageVersionElementWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            NullLoggerFactory.Instance.CreateLogger<TemporaryDirectoryProvider>()
        );

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetDirectoryPackagesPropsParser>>();

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
                    <PackageVersion Include="Microsoft.Extensions.Hosting" Version="8.0.0" Condition="'$(TargetFramework)' == 'net8.0'" />
                    <PackageVersion Include="Microsoft.Extensions.Hosting" Version="9.0.4" Condition="'$(TargetFramework)' == 'net9.0'" />
                  </ItemGroup>
                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("Directory.Packages.props"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(directoryPackagesPropsStream);

        var directoryPackagesPropsParser = new NugetDirectoryPackagesPropsParser(
            fileSystem,
            logger
        );

        IReadOnlyCollection<NugetTargetFramework> targetFrameworks = [new("net8.0"), new("net9.0")];

        var nugetFile = new NugetFile("Directory.Packages.props", ENugetFileType.DirectoryPackagesProps);
        var response = directoryPackagesPropsParser.Parse(
            temporaryDirectoryProvider.TemporaryDirectory,
            nugetFile,
            ImmutableDictionary<string, IReadOnlyCollection<NugetTargetFramework>>.Empty,
            targetFrameworks
        );

        var actualList = response.AsValueEnumerable().ToList();

        Assert.Equal(3, actualList.Count);

        // First package - no condition, should have all target frameworks
        Assert.Equal("Meziantou.Analyzer", actualList[0].NugetPackage.GetPackageName());
        Assert.Equal("2.0.195", ((NugetPackageVersion) actualList[0].NugetPackage).Version.ToString());
        Assert.Equal(2, actualList[0].TargetFrameworks.Count);

        // Conditional package for net8.0
        Assert.Equal("Microsoft.Extensions.Hosting", actualList[1].NugetPackage.GetPackageName());
        Assert.Equal("8.0.0", ((NugetPackageVersion) actualList[1].NugetPackage).Version.ToString());
        Assert.Single(actualList[1].TargetFrameworks);
        Assert.Equal("net8.0", actualList[1].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);

        // Conditional package for net9.0
        Assert.Equal("Microsoft.Extensions.Hosting", actualList[2].NugetPackage.GetPackageName());
        Assert.Equal("9.0.4", ((NugetPackageVersion) actualList[2].NugetPackage).Version.ToString());
        Assert.Single(actualList[2].TargetFrameworks);
        Assert.Equal("net9.0", actualList[2].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);
    }

    [Fact]
    public void ParseMixedConditionalFormatsWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            NullLoggerFactory.Instance.CreateLogger<TemporaryDirectoryProvider>()
        );

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetDirectoryPackagesPropsParser>>();

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
                    <PackageVersion Include="Meziantou.Analyzer" Version="2.0.257" />
                  </ItemGroup>
                  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
                    <PackageVersion Include="Microsoft.AspNetCore.WebUtilities" Version="8.0.22" />
                    <PackageVersion Include="Microsoft.Extensions.Caching.Abstractions" Version="8.0.0" />
                    <PackageVersion Include="System.Text.Json" Version="8.0.6" />
                  </ItemGroup>
                  <ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
                    <PackageVersion Include="Microsoft.AspNetCore.WebUtilities" Version="9.0.11" />
                    <PackageVersion Include="Microsoft.Extensions.Caching.Abstractions" Version="9.0.11" />
                    <PackageVersion Include="System.Text.Json" Version="9.0.11" />
                  </ItemGroup>
                  <ItemGroup Condition="'$(TargetFramework)' == 'net10.0'">
                    <PackageVersion Include="Microsoft.AspNetCore.WebUtilities" Version="10.0.1" />
                    <PackageVersion Include="System.Text.Json" Version="10.0.1" />
                  </ItemGroup>
                  <ItemGroup>
                    <PackageVersion Include="ZLinq" Version="1.5.4" />
                  </ItemGroup>
                </Project>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("Directory.Packages.props"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(directoryPackagesPropsStream);

        var directoryPackagesPropsParser = new NugetDirectoryPackagesPropsParser(
            fileSystem,
            logger
        );

        IReadOnlyCollection<NugetTargetFramework> targetFrameworks = [new("net8.0"), new("net9.0"), new("net10.0")];

        var nugetFile = new NugetFile("Directory.Packages.props", ENugetFileType.DirectoryPackagesProps);
        var response = directoryPackagesPropsParser.Parse(
            temporaryDirectoryProvider.TemporaryDirectory,
            nugetFile,
            ImmutableDictionary<string, IReadOnlyCollection<NugetTargetFramework>>.Empty,
            targetFrameworks
        );

        var actualList = response.AsValueEnumerable().ToList();

        Assert.Equal(10, actualList.Count);

        // First package - no condition, should have all target frameworks
        Assert.Equal("Meziantou.Analyzer", actualList[0].NugetPackage.GetPackageName());
        Assert.Equal("2.0.257", ((NugetPackageVersion) actualList[0].NugetPackage).Version.ToString());
        Assert.Equal(3, actualList[0].TargetFrameworks.Count);

        // net8.0 conditional packages
        Assert.Equal("Microsoft.AspNetCore.WebUtilities", actualList[1].NugetPackage.GetPackageName());
        Assert.Equal("8.0.22", ((NugetPackageVersion) actualList[1].NugetPackage).Version.ToString());
        Assert.Single(actualList[1].TargetFrameworks);
        Assert.Equal("net8.0", actualList[1].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);

        Assert.Equal("Microsoft.Extensions.Caching.Abstractions", actualList[2].NugetPackage.GetPackageName());
        Assert.Equal("8.0.0", ((NugetPackageVersion) actualList[2].NugetPackage).Version.ToString());
        Assert.Single(actualList[2].TargetFrameworks);
        Assert.Equal("net8.0", actualList[2].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);

        Assert.Equal("System.Text.Json", actualList[3].NugetPackage.GetPackageName());
        Assert.Equal("8.0.6", ((NugetPackageVersion) actualList[3].NugetPackage).Version.ToString());
        Assert.Single(actualList[3].TargetFrameworks);
        Assert.Equal("net8.0", actualList[3].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);

        // net9.0 conditional packages
        Assert.Equal("Microsoft.AspNetCore.WebUtilities", actualList[4].NugetPackage.GetPackageName());
        Assert.Equal("9.0.11", ((NugetPackageVersion) actualList[4].NugetPackage).Version.ToString());
        Assert.Single(actualList[4].TargetFrameworks);
        Assert.Equal("net9.0", actualList[4].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);

        Assert.Equal("Microsoft.Extensions.Caching.Abstractions", actualList[5].NugetPackage.GetPackageName());
        Assert.Equal("9.0.11", ((NugetPackageVersion) actualList[5].NugetPackage).Version.ToString());
        Assert.Single(actualList[5].TargetFrameworks);
        Assert.Equal("net9.0", actualList[5].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);

        Assert.Equal("System.Text.Json", actualList[6].NugetPackage.GetPackageName());
        Assert.Equal("9.0.11", ((NugetPackageVersion) actualList[6].NugetPackage).Version.ToString());
        Assert.Single(actualList[6].TargetFrameworks);
        Assert.Equal("net9.0", actualList[6].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);

        // net10.0 conditional packages
        Assert.Equal("Microsoft.AspNetCore.WebUtilities", actualList[7].NugetPackage.GetPackageName());
        Assert.Equal("10.0.1", ((NugetPackageVersion) actualList[7].NugetPackage).Version.ToString());
        Assert.Single(actualList[7].TargetFrameworks);
        Assert.Equal("net10.0", actualList[7].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);

        Assert.Equal("System.Text.Json", actualList[8].NugetPackage.GetPackageName());
        Assert.Equal("10.0.1", ((NugetPackageVersion) actualList[8].NugetPackage).Version.ToString());
        Assert.Single(actualList[8].TargetFrameworks);
        Assert.Equal("net10.0", actualList[8].TargetFrameworks.AsValueEnumerable().Single().TargetFramework);

        // Last package - no condition, should have all target frameworks
        Assert.Equal("ZLinq", actualList[9].NugetPackage.GetPackageName());
        Assert.Equal("1.5.4", ((NugetPackageVersion) actualList[9].NugetPackage).Version.ToString());
        Assert.Equal(3, actualList[9].TargetFrameworks.Count);
    }
}
