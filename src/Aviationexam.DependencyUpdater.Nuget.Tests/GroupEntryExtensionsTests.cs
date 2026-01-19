using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class GroupEntryExtensionsTests
{
    [Fact]
    public void GetTitle_SinglePackageSingleFramework_IncludesFrameworkSuffix()
    {
        var fromVersions = new Dictionary<string, PackageVersion>
        {
            ["net10.0"] = new NuGetVersion("1.0.0").MapToPackageVersion()
        };

        var groupEntry = new GroupEntry("TestPackage", []);
        var updateResults = new List<NugetUpdateResult>
        {
            CreateUpdateResult("TestPackage", fromVersions, "2.0.0", ["net10.0"])
        };

        var result = groupEntry.GetTitle(updateResults);

        Assert.Equal("Bump TestPackage from 1.0.0 to 2.0.0 for net10.0", result);
    }

    [Fact]
    public void GetTitle_SinglePackageMultipleFrameworksSameVersion_NoFrameworkSuffix()
    {
        var fromVersions = new Dictionary<string, PackageVersion>
        {
            ["net9.0"] = new NuGetVersion("1.5.0").MapToPackageVersion(),
            ["net10.0"] = new NuGetVersion("1.5.0").MapToPackageVersion()
        };

        var groupEntry = new GroupEntry("TestPackage", []);
        var updateResults = new List<NugetUpdateResult>
        {
            CreateUpdateResult("TestPackage", fromVersions, "2.0.0", ["net9.0", "net10.0"])
        };

        var result = groupEntry.GetTitle(updateResults);

        Assert.Equal("Bump TestPackage from 1.5.0 to 2.0.0", result);
    }

    [Fact]
    public void GetTitle_SinglePackageMultipleFrameworksDifferentVersions_ShowsVersionRange()
    {
        var fromVersions = new Dictionary<string, PackageVersion>
        {
            ["net9.0"] = new NuGetVersion("1.1.17").MapToPackageVersion(),
            ["net10.0"] = new NuGetVersion("1.1.21").MapToPackageVersion()
        };

        var groupEntry = new GroupEntry("Meziantou.Extensions.Logging.Xunit.v3", []);
        var updateResults = new List<NugetUpdateResult>
        {
            CreateUpdateResult("Meziantou.Extensions.Logging.Xunit.v3", fromVersions, "1.1.22", ["net9.0", "net10.0"])
        };

        var result = groupEntry.GetTitle(updateResults);

        Assert.Equal("Bump Meziantou.Extensions.Logging.Xunit.v3 from 1.1.17-1.1.21 to 1.1.22", result);
    }

    [Fact]
    public void GetTitle_SinglePackageOnlyOneFrameworkUpdating_ShowsFrameworkSuffix()
    {
        var fromVersions = new Dictionary<string, PackageVersion>
        {
            ["net10.0"] = new NuGetVersion("2.0.0").MapToPackageVersion()
        };

        var groupEntry = new GroupEntry("TestPackage", []);
        var updateResults = new List<NugetUpdateResult>
        {
            CreateUpdateResult("TestPackage", fromVersions, "3.0.0", ["net10.0"])
        };

        var result = groupEntry.GetTitle(updateResults);

        Assert.Equal("Bump TestPackage from 2.0.0 to 3.0.0 for net10.0", result);
    }

    [Fact]
    public void GetTitle_MultiplePackages_GeneratesGroupTitle()
    {
        var groupEntry = new GroupEntry("TestGroup", []);
        var updateResults = new List<NugetUpdateResult>
        {
            CreateUpdateResult("Package1", new Dictionary<string, PackageVersion> { ["net10.0"] = new NuGetVersion("1.0.0").MapToPackageVersion() }, "1.5.0", ["net10.0"]),
            CreateUpdateResult("Package2", new Dictionary<string, PackageVersion> { ["net10.0"] = new NuGetVersion("2.0.0").MapToPackageVersion() }, "2.5.0", ["net10.0"])
        };

        var result = groupEntry.GetTitle(updateResults);

        Assert.Equal("Bump TestGroup group – 2 updates", result);
    }

    [Fact]
    public void GetTitle_PackageWithNoFromVersions_FallsBackToCurrentVersion()
    {
        var groupEntry = new GroupEntry("NewPackage", []);
        var updateResults = new List<NugetUpdateResult>
        {
            CreateUpdateResult("NewPackage", new Dictionary<string, PackageVersion>(), "2.0.0", ["net10.0"])
        };

        var result = groupEntry.GetTitle(updateResults);

        Assert.Equal("Bump NewPackage from 1.0.0 to 2.0.0", result);
    }

    [Fact]
    public void GetCommitMessage_SinglePackageMultipleFrameworksDifferentVersions_ShowsMultipleLines()
    {
        var fromVersions = new Dictionary<string, PackageVersion>
        {
            ["net9.0"] = new NuGetVersion("1.1.17").MapToPackageVersion(),
            ["net10.0"] = new NuGetVersion("1.1.21").MapToPackageVersion()
        };

        var updateResults = new List<NugetUpdateResult>
        {
            CreateUpdateResult("Meziantou.Extensions.Logging.Xunit.v3", fromVersions, "1.1.22", ["net9.0", "net10.0"])
        };

        var result = updateResults.GetCommitMessage();

        Assert.Equal("""
            Updates 1 packages:

            - Update Meziantou.Extensions.Logging.Xunit.v3 from 1.1.17 to 1.1.22 for net9.0
            - Update Meziantou.Extensions.Logging.Xunit.v3 from 1.1.21 to 1.1.22 for net10.0

            """, result);
    }

    [Fact]
    public void GetCommitMessage_SinglePackageSingleFramework_ShowsFrameworkSuffix()
    {
        var fromVersions = new Dictionary<string, PackageVersion>
        {
            ["net10.0"] = new NuGetVersion("1.0.0").MapToPackageVersion()
        };

        var updateResults = new List<NugetUpdateResult>
        {
            CreateUpdateResult("TestPackage", fromVersions, "2.0.0", ["net10.0"])
        };

        var result = updateResults.GetCommitMessage();

        Assert.Equal("""
            Updates 1 packages:

            - Update TestPackage from 1.0.0 to 2.0.0 for net10.0

            """, result);
    }

    [Fact]
    public void GetCommitMessage_SinglePackageMultipleFrameworksSameVersion_NoFrameworkSuffix()
    {
        var fromVersions = new Dictionary<string, PackageVersion>
        {
            ["net9.0"] = new NuGetVersion("1.5.0").MapToPackageVersion(),
            ["net10.0"] = new NuGetVersion("1.5.0").MapToPackageVersion()
        };

        var updateResults = new List<NugetUpdateResult>
        {
            CreateUpdateResult("TestPackage", fromVersions, "2.0.0", ["net9.0", "net10.0"])
        };

        var result = updateResults.GetCommitMessage();

        Assert.Equal("""
            Updates 1 packages:

            - Update TestPackage from 1.5.0 to 2.0.0

            """, result);
    }

    [Fact]
    public void GetTitleAndCommitMessage_RealWorldScenario_DifferentVersionTransitions()
    {
        // Arrange - net9.0 gets 1.1.17 (max supported), net10.0 gets 1.1.22
        var fromVersionsNet9 = new Dictionary<string, PackageVersion>
        {
            ["net9.0"] = new NuGetVersion("1.1.16").MapToPackageVersion()
        };

        var fromVersionsNet10 = new Dictionary<string, PackageVersion>
        {
            ["net10.0"] = new NuGetVersion("1.1.17").MapToPackageVersion()
        };

        var groupEntry = new GroupEntry("Meziantou.Extensions.Logging.Xunit.v3", []);
        var updateResults = new List<NugetUpdateResult>
        {
            CreateUpdateResult("Meziantou.Extensions.Logging.Xunit.v3", fromVersionsNet9, "1.1.17", ["net9.0"]),
            CreateUpdateResult("Meziantou.Extensions.Logging.Xunit.v3", fromVersionsNet10, "1.1.22", ["net10.0"])
        };

        // Act
        var title = groupEntry.GetTitle(updateResults);
        var commitMessage = updateResults.GetCommitMessage();

        // Assert - Title shows version range for both updates
        Assert.Equal("Bump Meziantou.Extensions.Logging.Xunit.v3 from 1.1.16-1.1.17 to 1.1.17-1.1.22", title);

        // Assert - Description shows detailed per-framework updates
        Assert.Equal("""
            Updates 1 packages:

            - Update Meziantou.Extensions.Logging.Xunit.v3 from 1.1.16 to 1.1.17 for net9.0
            - Update Meziantou.Extensions.Logging.Xunit.v3 from 1.1.17 to 1.1.22 for net10.0

            """, commitMessage);
    }

    private static NugetUpdateResult CreateUpdateResult(
        string packageName,
        Dictionary<string, PackageVersion> fromVersionsPerFramework,
        string toVersion,
        string[] targetFrameworks,
        string? condition = null
    )
    {
        var targetFrameworkList = new List<NugetTargetFramework>();
        foreach (var tf in targetFrameworks)
        {
            targetFrameworkList.Add(new NugetTargetFramework(tf));
        }

        var candidate = new NugetUpdateCandidate(
            new NugetDependency(
                new NugetFile("Test.csproj", ENugetFileType.Csproj),
                new NugetPackageReference(packageName, new VersionRange(new NuGetVersion("1.0.0")), condition),
                targetFrameworkList
            ),
            new PossiblePackageVersion(
                new PackageVersionWithDependencySets(
                    new NuGetVersion(toVersion).MapToPackageVersion()
                )
                {
                    DependencySets = new Dictionary<EPackageSource, IReadOnlyCollection<DependencySet>>()
                },
                []
            )
        );

        return new NugetUpdateResult(candidate, fromVersionsPerFramework);
    }
}
