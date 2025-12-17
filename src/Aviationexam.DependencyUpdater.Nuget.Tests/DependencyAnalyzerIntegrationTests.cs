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
        Assert.Equal("8.0.22", ((NugetPackageVersion) microsoftAspNetCoreNet80.NugetPackage).Version.ToString());
        Assert.Single(microsoftAspNetCoreNet80.TargetFrameworks);
        Assert.Equal("net8.0", microsoftAspNetCoreNet80.TargetFrameworks.First().TargetFramework);

        // Verify net9.0 conditional packages (with lowered version 9.0.0 instead of 9.0.11)
        var net90Dependencies = dependencies
            .Where(d => d.TargetFrameworks.Count == 1 && d.TargetFrameworks.First().TargetFramework == "net9.0")
            .ToList();
        Assert.Equal(5, net90Dependencies.Count);

        var microsoftAspNetCoreNet90 = net90Dependencies.First(d => d.NugetPackage.GetPackageName() == "Microsoft.AspNetCore.WebUtilities");
        Assert.Equal("9.0.0", ((NugetPackageVersion) microsoftAspNetCoreNet90.NugetPackage).Version.ToString());
        Assert.Single(microsoftAspNetCoreNet90.TargetFrameworks);
        Assert.Equal("net9.0", microsoftAspNetCoreNet90.TargetFrameworks.First().TargetFramework);

        var systemTextJsonNet90 = net90Dependencies.First(d => d.NugetPackage.GetPackageName() == "System.Text.Json");
        Assert.Equal("9.0.0", ((NugetPackageVersion) systemTextJsonNet90.NugetPackage).Version.ToString());
        Assert.Single(systemTextJsonNet90.TargetFrameworks);
        Assert.Equal("net9.0", systemTextJsonNet90.TargetFrameworks.First().TargetFramework);

        // Verify net10.0 conditional packages remain at 10.0.1 (should NOT be affected)
        var net100Dependencies = dependencies
            .Where(d => d.TargetFrameworks.Count == 1 && d.TargetFrameworks.First().TargetFramework == "net10.0")
            .ToList();
        Assert.Equal(5, net100Dependencies.Count);

        var microsoftAspNetCoreNet100 = net100Dependencies.First(d => d.NugetPackage.GetPackageName() == "Microsoft.AspNetCore.WebUtilities");
        Assert.Equal("10.0.1", ((NugetPackageVersion) microsoftAspNetCoreNet100.NugetPackage).Version.ToString());
        Assert.Single(microsoftAspNetCoreNet100.TargetFrameworks);
        Assert.Equal("net10.0", microsoftAspNetCoreNet100.TargetFrameworks.First().TargetFramework);

        // Key assertion: Verify that each package for each framework is a separate dependency
        // This is crucial because the analyzer must treat them independently for updates
        var aspNetCoreWebUtilitiesDeps = dependencies
            .Where(d => d.NugetPackage.GetPackageName() == "Microsoft.AspNetCore.WebUtilities")
            .ToList();
        Assert.Equal(3, aspNetCoreWebUtilitiesDeps.Count); // One for each framework

        // Each should have a different version
        Assert.Contains(aspNetCoreWebUtilitiesDeps, d => ((NugetPackageVersion) d.NugetPackage).Version.ToString() == "8.0.22");
        Assert.Contains(aspNetCoreWebUtilitiesDeps, d => ((NugetPackageVersion) d.NugetPackage).Version.ToString() == "9.0.0");
        Assert.Contains(aspNetCoreWebUtilitiesDeps, d => ((NugetPackageVersion) d.NugetPackage).Version.ToString() == "10.0.1");

        // Each should have exactly one target framework
        Assert.All(aspNetCoreWebUtilitiesDeps, d => Assert.Single(d.TargetFrameworks));
    }

    /// <summary>
    /// Tests the Kiota package group scenario from Aviationexam.MoneyErp.
    /// Kiota packages are grouped together and should be updated as a cohesive unit.
    /// All Kiota packages have the same version across all target frameworks (no conditional versioning).
    /// This tests that unconditional packages (no targetFramework conditions) are parsed correctly
    /// and receive all target frameworks.
    /// </summary>
    [Fact]
    public void Parse_KiotaPackageGroup_AllPackagesHaveAllTargetFrameworks()
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
              </PropertyGroup>
              <ItemGroup>
                <PackageVersion Include="Microsoft.Kiota.Abstractions" Version="1.21.0" />
                <PackageVersion Include="Microsoft.Kiota.Http.HttpClientLibrary" Version="1.21.0" />
                <PackageVersion Include="Microsoft.Kiota.Serialization.Form" Version="1.21.0" />
                <PackageVersion Include="Microsoft.Kiota.Serialization.Json" Version="1.21.0" />
                <PackageVersion Include="Microsoft.Kiota.Serialization.Text" Version="1.21.0" />
                <PackageVersion Include="Microsoft.Kiota.Serialization.Multipart" Version="1.21.0" />
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

        // Verify we have 6 Kiota packages
        Assert.Equal(6, dependencies.Count);

        // All packages should have all target frameworks (unconditional)
        foreach (var dependency in dependencies)
        {
            Assert.Equal(3, dependency.TargetFrameworks.Count);
            Assert.Contains(dependency.TargetFrameworks, tf => tf.TargetFramework == "net8.0");
            Assert.Contains(dependency.TargetFrameworks, tf => tf.TargetFramework == "net9.0");
            Assert.Contains(dependency.TargetFrameworks, tf => tf.TargetFramework == "net10.0");
        }

        // Verify all packages have version 1.21.0
        var abstractionsPackage = dependencies.First(d => d.NugetPackage.GetPackageName() == "Microsoft.Kiota.Abstractions");
        Assert.Equal("1.21.0", ((NugetPackageVersion) abstractionsPackage.NugetPackage).Version.ToString());

        var httpClientLibraryPackage = dependencies.First(d => d.NugetPackage.GetPackageName() == "Microsoft.Kiota.Http.HttpClientLibrary");
        Assert.Equal("1.21.0", ((NugetPackageVersion) httpClientLibraryPackage.NugetPackage).Version.ToString());

        var serializationFormPackage = dependencies.First(d => d.NugetPackage.GetPackageName() == "Microsoft.Kiota.Serialization.Form");
        Assert.Equal("1.21.0", ((NugetPackageVersion) serializationFormPackage.NugetPackage).Version.ToString());

        var serializationJsonPackage = dependencies.First(d => d.NugetPackage.GetPackageName() == "Microsoft.Kiota.Serialization.Json");
        Assert.Equal("1.21.0", ((NugetPackageVersion) serializationJsonPackage.NugetPackage).Version.ToString());

        var serializationTextPackage = dependencies.First(d => d.NugetPackage.GetPackageName() == "Microsoft.Kiota.Serialization.Text");
        Assert.Equal("1.21.0", ((NugetPackageVersion) serializationTextPackage.NugetPackage).Version.ToString());

        var serializationMultipartPackage = dependencies.First(d => d.NugetPackage.GetPackageName() == "Microsoft.Kiota.Serialization.Multipart");
        Assert.Equal("1.21.0", ((NugetPackageVersion) serializationMultipartPackage.NugetPackage).Version.ToString());

        // Key assertion: All Kiota packages share the same version and should be grouped
        var kiotaPackageNames = new[]
        {
            "Microsoft.Kiota.Abstractions",
            "Microsoft.Kiota.Http.HttpClientLibrary",
            "Microsoft.Kiota.Serialization.Form",
            "Microsoft.Kiota.Serialization.Json",
            "Microsoft.Kiota.Serialization.Text",
            "Microsoft.Kiota.Serialization.Multipart"
        };

        var kiotaPackages = dependencies
            .Where(d => kiotaPackageNames.Contains(d.NugetPackage.GetPackageName()))
            .ToList();

        Assert.Equal(6, kiotaPackages.Count);
        Assert.All(kiotaPackages, d =>
        {
            Assert.Equal("1.21.0", ((NugetPackageVersion) d.NugetPackage).Version.ToString());
            Assert.Equal(3, d.TargetFrameworks.Count);
        });
    }

    /// <summary>
    /// Tests ZLinq package parsing - a simple unconditional package.
    /// ZLinq is used across all target frameworks without special versioning.
    /// </summary>
    [Fact]
    public void Parse_ZLinqPackage_HasAllTargetFrameworks()
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
              </PropertyGroup>
              <ItemGroup>
                <PackageVersion Include="ZLinq" Version="1.5.4" />
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

        Assert.Single(dependencies);

        var zlinqPackage = dependencies[0];
        Assert.Equal("ZLinq", zlinqPackage.NugetPackage.GetPackageName());
        Assert.Equal("1.5.4", ((NugetPackageVersion) zlinqPackage.NugetPackage).Version.ToString());

        // Should have all target frameworks (unconditional)
        Assert.Equal(3, zlinqPackage.TargetFrameworks.Count);
        Assert.Contains(zlinqPackage.TargetFrameworks, tf => tf.TargetFramework == "net8.0");
        Assert.Contains(zlinqPackage.TargetFrameworks, tf => tf.TargetFramework == "net9.0");
        Assert.Contains(zlinqPackage.TargetFrameworks, tf => tf.TargetFramework == "net10.0");
    }

    /// <summary>
    /// Tests ZeroQL package with preview/prerelease version parsing.
    /// ZeroQL uses preview versions (8.0.0-preview.7) and should be parsed correctly.
    /// This tests that prerelease version strings are handled properly.
    /// </summary>
    [Fact]
    public void Parse_ZeroQLPreviewVersion_ParsesCorrectly()
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
              </PropertyGroup>
              <ItemGroup>
                <PackageVersion Include="ZeroQL" Version="8.0.0-preview.7" />
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

        Assert.Single(dependencies);

        var zeroqlPackage = dependencies[0];
        Assert.Equal("ZeroQL", zeroqlPackage.NugetPackage.GetPackageName());

        // Verify preview version is parsed correctly
        var version = ((NugetPackageVersion) zeroqlPackage.NugetPackage).Version;
        Assert.Equal("8.0.0-preview.7", version.ToString());
        Assert.True(version.IsPrerelease);
        Assert.Equal("preview.7", version.Release);

        // Should have all target frameworks (unconditional)
        Assert.Equal(3, zeroqlPackage.TargetFrameworks.Count);
        Assert.Contains(zeroqlPackage.TargetFrameworks, tf => tf.TargetFramework == "net8.0");
        Assert.Contains(zeroqlPackage.TargetFrameworks, tf => tf.TargetFramework == "net9.0");
        Assert.Contains(zeroqlPackage.TargetFrameworks, tf => tf.TargetFramework == "net10.0");
    }

    /// <summary>
    /// Tests the problematic scenario from Aviationexam.MoneyErp where Meziantou.Extensions.Logging.Xunit.v3
    /// is present in ALL target frameworks but should be updated uniformly.
    /// The issue is that this package appears in conditional ItemGroups for each framework,
    /// but all have the same version (1.1.19) and should be updated together.
    /// This tests that packages appearing in multiple conditional blocks are correctly parsed
    /// as separate dependencies for each framework.
    /// </summary>
    [Fact]
    public void Parse_MeziantouLoggingXunitCrossFramework_CreatesSeparateDependenciesPerFramework()
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
              </PropertyGroup>
              <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
                <PackageVersion Include="Meziantou.Extensions.Logging.Xunit.v3" Version="1.1.19" />
              </ItemGroup>
              <ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
                <PackageVersion Include="Meziantou.Extensions.Logging.Xunit.v3" Version="1.1.19" />
              </ItemGroup>
              <ItemGroup Condition="'$(TargetFramework)' == 'net10.0'">
                <PackageVersion Include="Meziantou.Extensions.Logging.Xunit.v3" Version="1.1.19" />
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

        // Should create 3 separate dependencies - one for each framework
        Assert.Equal(3, dependencies.Count);

        // Each dependency should be for the same package with same version
        Assert.All(dependencies, d =>
        {
            Assert.Equal("Meziantou.Extensions.Logging.Xunit.v3", d.NugetPackage.GetPackageName());
            Assert.Equal("1.1.19", ((NugetPackageVersion) d.NugetPackage).Version.ToString());
        });

        // Each dependency should have exactly ONE target framework
        Assert.All(dependencies, d => Assert.Single(d.TargetFrameworks));

        // Verify we have one dependency for each framework
        var net80Dep = dependencies.FirstOrDefault(d => d.TargetFrameworks.First().TargetFramework == "net8.0");
        var net90Dep = dependencies.FirstOrDefault(d => d.TargetFrameworks.First().TargetFramework == "net9.0");
        var net100Dep = dependencies.FirstOrDefault(d => d.TargetFrameworks.First().TargetFramework == "net10.0");

        Assert.NotNull(net80Dep);
        Assert.NotNull(net90Dep);
        Assert.NotNull(net100Dep);

        // All should have the same version
        Assert.Equal("1.1.19", ((NugetPackageVersion) net80Dep.NugetPackage).Version.ToString());
        Assert.Equal("1.1.19", ((NugetPackageVersion) net90Dep.NugetPackage).Version.ToString());
        Assert.Equal("1.1.19", ((NugetPackageVersion) net100Dep.NugetPackage).Version.ToString());

        // Key insight: When the same package appears in multiple conditional blocks with the same version,
        // it creates separate NugetDependency instances. The DependencyAnalyzer should treat these
        // independently for updates, which means if one framework's version is updated, the others
        // should also be considered for update to maintain consistency.
    }

    /// <summary>
    /// Tests parsing of the current repository's Directory.Packages.props structure.
    /// This is a real-world example with no conditional targetFramework blocks,
    /// meaning all packages should receive all target frameworks.
    /// Tests a mix of Microsoft.*, System.*, and third-party packages.
    /// </summary>
    [Fact]
    public void Parse_CurrentRepositoryStructure_AllPackagesHaveAllTargetFrameworks()
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
                <PackageVersion Include="Microsoft.Extensions.Hosting" Version="10.0.1" />
                <PackageVersion Include="Microsoft.Extensions.Logging.Console" Version="10.0.1" />
                <PackageVersion Include="NuGet.ProjectModel" Version="7.0.1" />
                <PackageVersion Include="System.Text.Json" Version="10.0.1" />
                <PackageVersion Include="System.Collections.Immutable" Version="9.0.11" />
                <PackageVersion Include="ZLinq" Version="1.5.4" />
              </ItemGroup>
              <ItemGroup>
                <PackageVersion Include="xunit.v3.mtp-v2" Version="3.2.1" />
                <PackageVersion Include="NSubstitute" Version="5.3.0" />
              </ItemGroup>
            </Project>
            """;

        IReadOnlyCollection<NugetTargetFramework> targetFrameworks =
        [
            new("net9.0"),
            new("net10.0")
        ];

        var dependencies = ParseDirectoryPackagesProps(
            temporaryDirectoryProvider,
            directoryPackagesPropsContent,
            targetFrameworks
        ).ToList();

        // All 9 packages should be present
        Assert.Equal(9, dependencies.Count);

        // All packages should have both target frameworks (unconditional)
        Assert.All(dependencies, d =>
        {
            Assert.Equal(2, d.TargetFrameworks.Count);
            Assert.Contains(d.TargetFrameworks, tf => tf.TargetFramework == "net9.0");
            Assert.Contains(d.TargetFrameworks, tf => tf.TargetFramework == "net10.0");
        });

        // Verify specific packages
        var meziantou = dependencies.First(d => d.NugetPackage.GetPackageName() == "Meziantou.Analyzer");
        Assert.Equal("2.0.260", ((NugetPackageVersion) meziantou.NugetPackage).Version.ToString());

        var microsoftExtensionsHosting = dependencies.First(d => d.NugetPackage.GetPackageName() == "Microsoft.Extensions.Hosting");
        Assert.Equal("10.0.1", ((NugetPackageVersion) microsoftExtensionsHosting.NugetPackage).Version.ToString());

        var systemTextJson = dependencies.First(d => d.NugetPackage.GetPackageName() == "System.Text.Json");
        Assert.Equal("10.0.1", ((NugetPackageVersion) systemTextJson.NugetPackage).Version.ToString());

        var systemCollectionsImmutable = dependencies.First(d => d.NugetPackage.GetPackageName() == "System.Collections.Immutable");
        Assert.Equal("9.0.11", ((NugetPackageVersion) systemCollectionsImmutable.NugetPackage).Version.ToString());

        var zlinq = dependencies.First(d => d.NugetPackage.GetPackageName() == "ZLinq");
        Assert.Equal("1.5.4", ((NugetPackageVersion) zlinq.NugetPackage).Version.ToString());

        var xunit = dependencies.First(d => d.NugetPackage.GetPackageName() == "xunit.v3.mtp-v2");
        Assert.Equal("3.2.1", ((NugetPackageVersion) xunit.NugetPackage).Version.ToString());

        var nsubstitute = dependencies.First(d => d.NugetPackage.GetPackageName() == "NSubstitute");
        Assert.Equal("5.3.0", ((NugetPackageVersion) nsubstitute.NugetPackage).Version.ToString());

        // Verify ItemGroup separation doesn't affect target framework assignment
        // Packages from both ItemGroups should have the same target frameworks
        var firstGroupPackage = dependencies.First(d => d.NugetPackage.GetPackageName() == "Meziantou.Analyzer");
        var secondGroupPackage = dependencies.First(d => d.NugetPackage.GetPackageName() == "xunit.v3.mtp-v2");

        Assert.Equal(firstGroupPackage.TargetFrameworks.Count, secondGroupPackage.TargetFrameworks.Count);
    }
}
