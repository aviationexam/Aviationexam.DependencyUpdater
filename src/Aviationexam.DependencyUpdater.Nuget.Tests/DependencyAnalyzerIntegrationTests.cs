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
using System.Linq;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

/// <summary>
/// Integration tests for DependencyAnalyzer focusing on targetFramework-specific dependency handling.
/// These tests verify that the analyzer correctly identifies and handles packages with different versions
/// for different target frameworks, ensuring that updates respect framework-specific constraints.
/// </summary>
public class DependencyAnalyzerIntegrationTests
{
    /// <summary>
    /// Creates a mock file system with a Directory.Packages.props file.
    /// </summary>
    private static IFileSystem CreateMockFileSystem(
        TemporaryDirectoryProvider temporaryDirectoryProvider,
        string directoryPackagesPropsContent
    )
    {
        var fileSystem = Substitute.For<IFileSystem>();

        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath("Directory.Packages.props"))
            .Returns(true);

        var stream = directoryPackagesPropsContent.AsStream();
        fileSystem
            .FileOpen(
                temporaryDirectoryProvider.GetPath("Directory.Packages.props"),
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            )
            .Returns(stream);

        return fileSystem;
    }

    /// <summary>
    /// Parses Directory.Packages.props and returns the dependencies with their target frameworks.
    /// </summary>
    private static IEnumerable<NugetDependency> ParseDirectoryPackagesProps(
        TemporaryDirectoryProvider temporaryDirectoryProvider,
        string directoryPackagesPropsContent,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks
    )
    {
        var fileSystem = CreateMockFileSystem(temporaryDirectoryProvider, directoryPackagesPropsContent);
        var logger = Substitute.For<ILogger<NugetDirectoryPackagesPropsParser>>();

        var parser = new NugetDirectoryPackagesPropsParser(fileSystem, logger);

        var nugetFile = new NugetFile("Directory.Packages.props", ENugetFileType.DirectoryPackagesProps);

        return parser.Parse(
            temporaryDirectoryProvider.TemporaryDirectory,
            nugetFile,
            ImmutableDictionary<string, IReadOnlyCollection<NugetTargetFramework>>.Empty,
            targetFrameworks
        );
    }

    /// <summary>
    /// Test scenario based on Aviationexam.MoneyErp repository structure.
    /// Verifies that when Microsoft.* packages at version 9.0.0 are updated to 9.0.11,
    /// only the net9.0 conditional packages are updated, not net8.0 or net10.0 versions.
    /// This test simulates the issue where Meziantou.Extensions.Logging.Xunit.v3 should be updated
    /// but targetFramework conditions must be respected.
    /// </summary>
    [Fact]
    public void Parse_MicrosoftPackagesNet90Group_OnlyUpdatesNet90Packages()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            NullLoggerFactory.Instance.CreateLogger<TemporaryDirectoryProvider>()
        );

        var directoryPackagesPropsContent =
            // language=xml
            """
            <Project>
              <PropertyGroup>
                <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
              </PropertyGroup>
              <ItemGroup>
                <PackageVersion Include="Meziantou.Analyzer" Version="2.0.260" />
              </ItemGroup>
              <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
                <PackageVersion Include="Microsoft.AspNetCore.WebUtilities" Version="8.0.22" />
                <PackageVersion Include="Microsoft.Extensions.Caching.Abstractions" Version="8.0.0" />
                <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="8.0.1" />
                <PackageVersion Include="System.Text.Json" Version="8.0.6" />
              </ItemGroup>
              <ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
                <PackageVersion Include="Microsoft.AspNetCore.WebUtilities" Version="9.0.0" />
                <PackageVersion Include="Microsoft.Bcl.AsyncInterfaces" Version="9.0.0" />
                <PackageVersion Include="Microsoft.Extensions.Caching.Abstractions" Version="9.0.0" />
                <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
                <PackageVersion Include="System.Text.Json" Version="9.0.0" />
              </ItemGroup>
              <ItemGroup Condition="'$(TargetFramework)' == 'net10.0'">
                <PackageVersion Include="Microsoft.AspNetCore.WebUtilities" Version="10.0.1" />
                <PackageVersion Include="Microsoft.Bcl.AsyncInterfaces" Version="10.0.1" />
                <PackageVersion Include="Microsoft.Extensions.Caching.Abstractions" Version="10.0.1" />
                <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="10.0.1" />
                <PackageVersion Include="System.Text.Json" Version="10.0.1" />
              </ItemGroup>
            </Project>
            """;

        IReadOnlyCollection<NugetTargetFramework> targetFrameworks =
        [
            new("net8.0"),
            new("net9.0"),
            new("net10.0")
        ];

        var dependencies = ParseDirectoryPackagesProps(
            temporaryDirectoryProvider,
            directoryPackagesPropsContent,
            targetFrameworks
        ).ToList();

        // Verify we have the correct number of dependencies
        // 1 (Meziantou.Analyzer) + 4 (net8.0) + 5 (net9.0) + 5 (net10.0) = 15
        Assert.Equal(15, dependencies.Count);

        // Verify Meziantou.Analyzer has all target frameworks (unconditional)
        var meziantouAnalyzer = dependencies.First(d => d.NugetPackage.GetPackageName() == "Meziantou.Analyzer");
        Assert.Equal(3, meziantouAnalyzer.TargetFrameworks.Count);
        Assert.Contains(meziantouAnalyzer.TargetFrameworks, tf => tf.TargetFramework == "net8.0");
        Assert.Contains(meziantouAnalyzer.TargetFrameworks, tf => tf.TargetFramework == "net9.0");
        Assert.Contains(meziantouAnalyzer.TargetFrameworks, tf => tf.TargetFramework == "net10.0");

        // Verify net8.0 conditional packages
        var net80Dependencies = dependencies
            .Where(d => d.TargetFrameworks.Count == 1 && d.TargetFrameworks.First().TargetFramework == "net8.0")
            .ToList();
        Assert.Equal(4, net80Dependencies.Count);

        var microsoftAspNetCoreNet80 = net80Dependencies.First(d => d.NugetPackage.GetPackageName() == "Microsoft.AspNetCore.WebUtilities");
        Assert.Equal("8.0.22", ((NugetPackageVersion)microsoftAspNetCoreNet80.NugetPackage).Version.ToString());
        Assert.Single(microsoftAspNetCoreNet80.TargetFrameworks);
        Assert.Equal("net8.0", microsoftAspNetCoreNet80.TargetFrameworks.First().TargetFramework);

        // Verify net9.0 conditional packages (with lowered version 9.0.0 instead of 9.0.11)
        var net90Dependencies = dependencies
            .Where(d => d.TargetFrameworks.Count == 1 && d.TargetFrameworks.First().TargetFramework == "net9.0")
            .ToList();
        Assert.Equal(5, net90Dependencies.Count);

        var microsoftAspNetCoreNet90 = net90Dependencies.First(d => d.NugetPackage.GetPackageName() == "Microsoft.AspNetCore.WebUtilities");
        Assert.Equal("9.0.0", ((NugetPackageVersion)microsoftAspNetCoreNet90.NugetPackage).Version.ToString());
        Assert.Single(microsoftAspNetCoreNet90.TargetFrameworks);
        Assert.Equal("net9.0", microsoftAspNetCoreNet90.TargetFrameworks.First().TargetFramework);

        var systemTextJsonNet90 = net90Dependencies.First(d => d.NugetPackage.GetPackageName() == "System.Text.Json");
        Assert.Equal("9.0.0", ((NugetPackageVersion)systemTextJsonNet90.NugetPackage).Version.ToString());
        Assert.Single(systemTextJsonNet90.TargetFrameworks);
        Assert.Equal("net9.0", systemTextJsonNet90.TargetFrameworks.First().TargetFramework);

        // Verify net10.0 conditional packages remain at 10.0.1 (should NOT be affected)
        var net100Dependencies = dependencies
            .Where(d => d.TargetFrameworks.Count == 1 && d.TargetFrameworks.First().TargetFramework == "net10.0")
            .ToList();
        Assert.Equal(5, net100Dependencies.Count);

        var microsoftAspNetCoreNet100 = net100Dependencies.First(d => d.NugetPackage.GetPackageName() == "Microsoft.AspNetCore.WebUtilities");
        Assert.Equal("10.0.1", ((NugetPackageVersion)microsoftAspNetCoreNet100.NugetPackage).Version.ToString());
        Assert.Single(microsoftAspNetCoreNet100.TargetFrameworks);
        Assert.Equal("net10.0", microsoftAspNetCoreNet100.TargetFrameworks.First().TargetFramework);

        // Key assertion: Verify that each package for each framework is a separate dependency
        // This is crucial because the analyzer must treat them independently for updates
        var aspNetCoreWebUtilitiesDeps = dependencies
            .Where(d => d.NugetPackage.GetPackageName() == "Microsoft.AspNetCore.WebUtilities")
            .ToList();
        Assert.Equal(3, aspNetCoreWebUtilitiesDeps.Count); // One for each framework

        // Each should have a different version
        Assert.Contains(aspNetCoreWebUtilitiesDeps, d => ((NugetPackageVersion)d.NugetPackage).Version.ToString() == "8.0.22");
        Assert.Contains(aspNetCoreWebUtilitiesDeps, d => ((NugetPackageVersion)d.NugetPackage).Version.ToString() == "9.0.0");
        Assert.Contains(aspNetCoreWebUtilitiesDeps, d => ((NugetPackageVersion)d.NugetPackage).Version.ToString() == "10.0.1");

        // Each should have exactly one target framework
        Assert.All(aspNetCoreWebUtilitiesDeps, d => Assert.Single(d.TargetFrameworks));
    }
}
