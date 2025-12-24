using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

/// <summary>
/// Tests for DependencyUpdateProcessor.
/// This demonstrates how the refactored code is now testable without needing the full DependencyAnalyzer context.
/// </summary>
public sealed class DependencyUpdateProcessorTests
{
    private readonly IgnoredDependenciesResolver _ignoredDependenciesResolver;
    private readonly DependencyUpdateProcessor _processor;

    public DependencyUpdateProcessorTests()
    {
        _ignoredDependenciesResolver = new IgnoredDependenciesResolver();
        _processor = new DependencyUpdateProcessor(_ignoredDependenciesResolver);
    }

    [Fact]
    public void ProcessDependencySet_WithNullMinVersion_SkipsPackage()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var ignoreResolver = new IgnoreResolver([], logger);
        var currentVersions = new Dictionary<string, IDictionary<string, PackageVersion>>();
        var packageFlags = new Dictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>>();
        var dependenciesToCheck = new Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)>();
        var targetFramework = new NugetTargetFramework("net8.0");

        var packageDependency = new PackageDependencyInfo(
            Id: "TestPackage",
            MinVersion: null // No minimum version
        );

        var dependencySet = new DependencySet(
            TargetFramework: "net8.0",
            Packages: [packageDependency]
        );

        // Act
        var result = _processor.ProcessDependencySet(
            ignoreResolver,
            currentVersions,
            packageFlags,
            dependenciesToCheck,
            dependencySet,
            [targetFramework]
        );

        // Assert
        Assert.Empty(result);
        Assert.Empty(packageFlags);
        Assert.Empty(dependenciesToCheck);
    }

    [Fact]
    public void ProcessDependencySet_WithSinglePackage_PopulatesPackageFlags()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var ignoreResolver = new IgnoreResolver([], logger);
        var currentVersions = new Dictionary<string, IDictionary<string, PackageVersion>>();
        var packageFlags = new Dictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>>();
        var dependenciesToCheck = new Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)>();
        var targetFramework = new NugetTargetFramework("net8.0");

        var packageVersion = new PackageVersion(
            Version: new Version(1, 0, 0),
            IsPrerelease: false,
            ReleaseLabels: [],
            ReleaseLabelsComparer: NugetReleaseLabelComparer.Instance
        );

        var packageDependency = new PackageDependencyInfo(
            Id: "TestPackage",
            MinVersion: packageVersion
        );

        var dependencySet = new DependencySet(
            TargetFramework: "net8.0",
            Packages: [packageDependency]
        );

        // Act
        var result = _processor.ProcessDependencySet(
            ignoreResolver,
            currentVersions,
            packageFlags,
            dependenciesToCheck,
            dependencySet,
            [targetFramework]
        );

        // Assert
        var expectedPackage = Assert.Single(result);
        Assert.Equal(new Package("TestPackage", packageVersion), expectedPackage);

        // Verify package flags were set
        var flags = Assert.Contains(expectedPackage, packageFlags);
        Assert.Contains(targetFramework, flags.Keys);
        Assert.Equal(EDependencyFlag.Unknown, flags[targetFramework]);

        // Verify dependency was queued for checking
        Assert.Single(dependenciesToCheck);
        var (queuedPackage, queuedFrameworks) = dependenciesToCheck.Dequeue();
        Assert.Equal(expectedPackage, queuedPackage);
        Assert.Contains(targetFramework, queuedFrameworks);
    }

    [Fact]
    public void ProcessDependencySet_WithAlreadyInstalledVersion_MarksAsValid()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var ignoreResolver = new IgnoreResolver([], logger);
        var targetFramework = new NugetTargetFramework("net8.0");
        var packageVersion = new PackageVersion(
            Version: new Version(1, 0, 0),
            IsPrerelease: false,
            ReleaseLabels: [],
            ReleaseLabelsComparer: NugetReleaseLabelComparer.Instance
        );

        // Package is already installed at this version
        var currentVersions = new Dictionary<string, IDictionary<string, PackageVersion>>
        {
            ["TestPackage"] = new Dictionary<string, PackageVersion>
            {
                ["net8.0"] = packageVersion
            }
        };

        var packageFlags = new Dictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>>();
        var dependenciesToCheck = new Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)>();

        var packageDependency = new PackageDependencyInfo(
            Id: "TestPackage",
            MinVersion: packageVersion
        );

        var dependencySet = new DependencySet(
            TargetFramework: "net8.0",
            Packages: [packageDependency]
        );

        // Act
        var result = _processor.ProcessDependencySet(
            ignoreResolver,
            currentVersions,
            packageFlags,
            dependenciesToCheck,
            dependencySet,
            [targetFramework]
        );

        // Assert
        var expectedPackage = Assert.Single(result);
        Assert.Equal(new Package("TestPackage", packageVersion), expectedPackage);

        // Should be marked as valid since it's already at the correct version
        var flags = Assert.Contains(expectedPackage, packageFlags);
        Assert.Equal(EDependencyFlag.Valid, flags[targetFramework]);

        // Should NOT be queued for checking since it's already valid
        Assert.Empty(dependenciesToCheck);
    }

    [Fact]
    public void ProcessDependencySet_WithIgnoredDependency_MarksAsContainsIgnoredDependency()
    {
        // Arrange
        var targetFramework = new NugetTargetFramework("net8.0");
        var currentVersion = new PackageVersion(
            Version: new Version(1, 0, 0),
            IsPrerelease: false,
            ReleaseLabels: [],
            ReleaseLabelsComparer: NugetReleaseLabelComparer.Instance
        );

        var newVersion = new PackageVersion(
            Version: new Version(2, 0, 0),
            IsPrerelease: false,
            ReleaseLabels: [],
            ReleaseLabelsComparer: NugetReleaseLabelComparer.Instance
        );

        var currentVersions = new Dictionary<string, IDictionary<string, PackageVersion>>
        {
            ["TestPackage"] = new Dictionary<string, PackageVersion>
            {
                ["net8.0"] = currentVersion
            }
        };

        // Setup ignore resolver to ignore this upgrade
        // The version change 1.0.0 -> 2.0.0 is a major version change
        var ignoreRules = new List<IIgnoreRule>
        {
            new ExplicitIgnoreRule(
                DependencyName: "TestPackage",
                UpdateTypes: ["version-update:semver-major"]
            )
        };
        var logger = Substitute.For<ILogger>();
        var ignoreResolver = new IgnoreResolver(ignoreRules, logger);

        var packageFlags = new Dictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>>();
        var dependenciesToCheck = new Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)>();

        var packageDependency = new PackageDependencyInfo(
            Id: "TestPackage",
            MinVersion: newVersion
        );

        var dependencySet = new DependencySet(
            TargetFramework: "net8.0",
            Packages: [packageDependency]
        );

        // Act
        var result = _processor.ProcessDependencySet(
            ignoreResolver,
            currentVersions,
            packageFlags,
            dependenciesToCheck,
            dependencySet,
            [targetFramework]
        );

        // Assert
        var expectedPackage = Assert.Single(result);
        Assert.Equal(new Package("TestPackage", newVersion), expectedPackage);

        // Should be marked as containing ignored dependency
        var flags = Assert.Contains(expectedPackage, packageFlags);
        Assert.Equal(EDependencyFlag.ContainsIgnoredDependency, flags[targetFramework]);

        // Should NOT be queued for checking since it's ignored
        Assert.Empty(dependenciesToCheck);
    }

    [Fact]
    public void ProcessDependencySet_WithMultiplePackages_ReturnsAllPackages()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var ignoreResolver = new IgnoreResolver([], logger);
        var currentVersions = new Dictionary<string, IDictionary<string, PackageVersion>>();
        var packageFlags = new Dictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>>();
        var dependenciesToCheck = new Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)>();
        var targetFramework = new NugetTargetFramework("net8.0");

        var version1 = new PackageVersion(
            Version: new Version(1, 0, 0),
            IsPrerelease: false,
            ReleaseLabels: [],
            ReleaseLabelsComparer: NugetReleaseLabelComparer.Instance
        );

        var version2 = new PackageVersion(
            Version: new Version(2, 0, 0),
            IsPrerelease: false,
            ReleaseLabels: [],
            ReleaseLabelsComparer: NugetReleaseLabelComparer.Instance
        );

        var dependencySet = new DependencySet(
            TargetFramework: "net8.0",
            Packages:
            [
                new PackageDependencyInfo("Package1", version1),
                new PackageDependencyInfo("Package2", version2)
            ]
        );

        // Act
        var result = _processor.ProcessDependencySet(
            ignoreResolver,
            currentVersions,
            packageFlags,
            dependenciesToCheck,
            dependencySet,
            [targetFramework]
        );

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(new Package("Package1", version1), result);
        Assert.Contains(new Package("Package2", version2), result);

        Assert.Equal(2, packageFlags.Count);
        Assert.Equal(2, dependenciesToCheck.Count);
    }

    [Fact]
    public void ProcessDependencySet_CalledTwiceWithSamePackage_DoesNotReprocessTargetFramework()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var ignoreResolver = new IgnoreResolver([], logger);
        var currentVersions = new Dictionary<string, IDictionary<string, PackageVersion>>();
        var packageFlags = new Dictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>>();
        var dependenciesToCheck = new Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)>();
        var targetFramework = new NugetTargetFramework("net8.0");

        var packageVersion = new PackageVersion(
            Version: new Version(1, 0, 0),
            IsPrerelease: false,
            ReleaseLabels: [],
            ReleaseLabelsComparer: NugetReleaseLabelComparer.Instance
        );

        var packageDependency = new PackageDependencyInfo(
            Id: "TestPackage",
            MinVersion: packageVersion
        );

        var dependencySet = new DependencySet(
            TargetFramework: "net8.0",
            Packages: [packageDependency]
        );

        // Act - Process the same dependency set twice
        _processor.ProcessDependencySet(
            ignoreResolver,
            currentVersions,
            packageFlags,
            dependenciesToCheck,
            dependencySet,
            [targetFramework]
        );

        var initialCheckCount = dependenciesToCheck.Count;

        _processor.ProcessDependencySet(
            ignoreResolver,
            currentVersions,
            packageFlags,
            dependenciesToCheck,
            dependencySet,
            [targetFramework]
        );

        // Assert - Should not queue again since framework is already processed
        Assert.Equal(initialCheckCount, dependenciesToCheck.Count);
    }

    [Theory]
    [ClassData(typeof(FutureDependenciesClassData))]
    public void ProcessDependenciesToUpdate_WithRealWorldData_ProcessesAllDependencies(
        IReadOnlyCollection<KeyValuePair<NugetDependency, IReadOnlyCollection<PackageVersionWithDependencySets>>> dependencies,
        DependencyProcessingResult expectedResult,
        IReadOnlyDictionary<string, PackageVersionWithDependencySets?> _
    )
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var ignoreResolver = new IgnoreResolver([], logger);
        var currentVersions = new Dictionary<string, IDictionary<string, PackageVersion>>();

        // Convert ALL dependencies to the format expected by ProcessDependenciesToUpdate
        var dependenciesToUpdate = dependencies.AsValueEnumerable()
            .ToDictionary(
                kvp => kvp.Key,
                IReadOnlyCollection<PossiblePackageVersion> (kvp) => kvp.Value.AsValueEnumerable()
                    .Select(pkgVersion => new PossiblePackageVersion(
                        pkgVersion,
                        pkgVersion.DependencySets.TryGetValue(EPackageSource.Default, out var depSets)
                            ? depSets
                            : []
                    ))
                    .ToList()
            );

        // Act
        var result = _processor.ProcessDependenciesToUpdate(
            ignoreResolver,
            currentVersions,
            dependenciesToUpdate
        );

        // Assert
        Assert.NotEmpty(result.PackageFlags);
        Assert.NotEmpty(result.DependenciesToCheck);

        // Verify the result matches the expected result
        Assert.Equal(expectedResult.PackageFlags.Count, result.PackageFlags.Count);

        foreach (var (expectedPackage, expectedFlags) in expectedResult.PackageFlags)
        {
            var actualFlags = Assert.Contains(expectedPackage, result.PackageFlags);

            Assert.Equal(expectedFlags.Count, actualFlags.Count);
            foreach (var (framework, expectedFlag) in expectedFlags)
            {
                Assert.Equal(expectedFlag, Assert.Contains(framework, actualFlags));
            }
        }
    }

    [Theory]
    [ClassData(typeof(FutureDependenciesClassData))]
    public void ProcessDependenciesToUpdate_WithRealWorldData_QueuesUnknownDependencies(
        IReadOnlyCollection<KeyValuePair<NugetDependency, IReadOnlyCollection<PackageVersionWithDependencySets>>> dependencies,
        DependencyProcessingResult expectedResult,
        IReadOnlyDictionary<string, PackageVersionWithDependencySets?> _
    )
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var ignoreResolver = new IgnoreResolver([], logger);
        var currentVersions = new Dictionary<string, IDictionary<string, PackageVersion>>();

        // Convert ALL dependencies to the format expected by ProcessDependenciesToUpdate
        var dependenciesToUpdate = dependencies.AsValueEnumerable()
            .ToDictionary(
                kvp => kvp.Key,
                IReadOnlyCollection<PossiblePackageVersion> (kvp) => kvp.Value.AsValueEnumerable()
                    .Select(pkgVersion => new PossiblePackageVersion(
                        pkgVersion,
                        pkgVersion.DependencySets.TryGetValue(EPackageSource.Default, out var depSets)
                            ? depSets
                            : []
                    ))
                    .ToList()
            );

        // Act
        var result = _processor.ProcessDependenciesToUpdate(
            ignoreResolver,
            currentVersions,
            dependenciesToUpdate
        );

        // Assert - Dependencies without current versions should be queued for checking
        Assert.NotEmpty(result.DependenciesToCheck);
        Assert.Equal(expectedResult.DependenciesToCheck.Count, result.DependenciesToCheck.Count);

        // Verify the queued dependencies match expected (order doesn't matter)
        var expectedList = expectedResult.DependenciesToCheck;
        var actualList = result.DependenciesToCheck;

        Assert.Equal(expectedList.Count, actualList.Count);

        // Check that all expected packages are in the actual queue
        foreach (var (expectedPackage, expectedFrameworks) in expectedList)
        {
            var actualEntry = actualList.AsValueEnumerable().FirstOrDefault(x => x.Package.Equals(expectedPackage));
            Assert.NotEqual(default, actualEntry);
            Assert.Equal(expectedFrameworks.Count, actualEntry.NugetTargetFrameworks.Count);
        }
    }

    [Theory]
    [ClassData(typeof(FutureDependenciesClassData))]
    public void ProcessDependenciesToUpdate_WithRealWorldData_CountsExpectedPackages(
        IReadOnlyCollection<KeyValuePair<NugetDependency, IReadOnlyCollection<PackageVersionWithDependencySets>>> dependencies,
        DependencyProcessingResult expectedResult,
        IReadOnlyDictionary<string, PackageVersionWithDependencySets?> _
    )
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var ignoreResolver = new IgnoreResolver([], logger);
        var currentVersions = new Dictionary<string, IDictionary<string, PackageVersion>>();

        // Convert ALL dependencies to the format expected by ProcessDependenciesToUpdate
        var dependenciesToUpdate = dependencies.AsValueEnumerable()
            .ToDictionary(
                kvp => kvp.Key,
                IReadOnlyCollection<PossiblePackageVersion> (kvp) => kvp.Value.AsValueEnumerable()
                    .Select(pkgVersion => new PossiblePackageVersion(
                        pkgVersion,
                        pkgVersion.DependencySets.TryGetValue(EPackageSource.Default, out var depSets)
                            ? depSets
                            : []
                    ))
                    .ToList()
            );

        // Act
        var result = _processor.ProcessDependenciesToUpdate(
            ignoreResolver,
            currentVersions,
            dependenciesToUpdate
        );

        // Assert - Should have processed exactly the expected number of packages
        Assert.Equal(expectedResult.PackageFlags.Count, result.PackageFlags.Count);

        // Verify all expected packages are present
        foreach (var expectedPackage in expectedResult.PackageFlags.Keys)
        {
            Assert.Contains(expectedPackage, result.PackageFlags.Keys);
        }
    }
}
