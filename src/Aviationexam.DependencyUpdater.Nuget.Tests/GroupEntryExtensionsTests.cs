using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Versioning;
using System.Collections.Generic;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class GroupEntryExtensionsTests
{
    [Fact]
    public void GetTitle_SinglePackageSingleFramework_IncludesFrameworkSuffix()
    {
        var groupEntry = new GroupEntry("TestPackage", []);
        var updateResults = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("TestPackage", "1.0.0", "2.0.0", condition: "net10.0")
        };

        var result = groupEntry.GetTitle(updateResults);

        Assert.Equal("Bump TestPackage from 1.0.0 to 2.0.0 for net10.0", result);
    }

    [Fact]
    public void GetTitle_SinglePackageMultipleFrameworksSameVersion_NoFrameworkSuffix()
    {
        var groupEntry = new GroupEntry("TestPackage", []);
        var updateResults = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("TestPackage", "1.5.0", "2.0.0")
        };

        var result = groupEntry.GetTitle(updateResults);

        Assert.Equal("Bump TestPackage from 1.5.0 to 2.0.0", result);
    }

    [Fact]
    public void GetTitle_SinglePackageMultipleFrameworksDifferentVersions_ShowsVersionRange()
    {
        var groupEntry = new GroupEntry("Meziantou.Extensions.Logging.Xunit.v3", []);
        var updateResults = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("Meziantou.Extensions.Logging.Xunit.v3", "1.1.17", "1.1.22", condition: "net9.0"),
            CreateUpdateCandidate("Meziantou.Extensions.Logging.Xunit.v3", "1.1.21", "1.1.22", condition: "net10.0")
        };

        var result = groupEntry.GetTitle(updateResults);

        Assert.Equal("Bump Meziantou.Extensions.Logging.Xunit.v3 from 1.1.17-1.1.21 to 1.1.22", result);
    }

    [Fact]
    public void GetTitle_SinglePackageOnlyOneFrameworkUpdating_ShowsFrameworkSuffix()
    {
        var groupEntry = new GroupEntry("TestPackage", []);
        var updateResults = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("TestPackage", "2.0.0", "3.0.0", condition: "net10.0")
        };

        var result = groupEntry.GetTitle(updateResults);

        Assert.Equal("Bump TestPackage from 2.0.0 to 3.0.0 for net10.0", result);
    }

    [Fact]
    public void GetTitle_MultiplePackages_GeneratesGroupTitle()
    {
        var groupEntry = new GroupEntry("TestGroup", []);
        var updateResults = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("Package1", "1.0.0", "1.5.0"),
            CreateUpdateCandidate("Package2", "2.0.0", "2.5.0")
        };

        var result = groupEntry.GetTitle(updateResults);

        Assert.Equal("Bump TestGroup group – 2 updates", result);
    }

    [Fact]
    public void GetTitle_MultiplePackagesSameVersions_GeneratesGroupTitleWithVersions()
    {
        var groupEntry = new GroupEntry("TestGroup", []);
        var updateResults = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("Package1", "1.0.0", "2.0.0"),
            CreateUpdateCandidate("Package2", "1.0.0", "2.0.0")
        };

        var result = groupEntry.GetTitle(updateResults);

        Assert.Equal("Bump TestGroup group – 2 updates from 1.0.0 to 2.0.0", result);
    }

    [Fact]
    public void GetTitle_PackageWithNullVersion_FallsBackToUnknown()
    {
        var groupEntry = new GroupEntry("NewPackage", []);
        var updateResults = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("NewPackage", fromVersion: null, "2.0.0")
        };

        var result = groupEntry.GetTitle(updateResults);

        Assert.Equal("Bump NewPackage from unknown to 2.0.0", result);
    }

    [Fact]
    public void GetCommitMessage_SinglePackageMultipleFrameworksDifferentVersions_ShowsMultipleLines()
    {
        var updateResults = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("Meziantou.Extensions.Logging.Xunit.v3", "1.1.17", "1.1.22", condition: "net9.0"),
            CreateUpdateCandidate("Meziantou.Extensions.Logging.Xunit.v3", "1.1.21", "1.1.22", condition: "net10.0")
        };

        var result = updateResults.GetCommitMessage();

        Assert.Equal(NormalizeLineEndings("""
            Updates 1 packages:

            - Update Meziantou.Extensions.Logging.Xunit.v3 from 1.1.17 to 1.1.22 for net9.0
            - Update Meziantou.Extensions.Logging.Xunit.v3 from 1.1.21 to 1.1.22 for net10.0

            """), result);
    }

    [Fact]
    public void GetCommitMessage_SinglePackageSingleFramework_ShowsFrameworkSuffix()
    {
        var updateResults = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("TestPackage", "1.0.0", "2.0.0", condition: "net10.0")
        };

        var result = updateResults.GetCommitMessage();

        Assert.Equal(NormalizeLineEndings("""
            Updates 1 packages:

            - Update TestPackage from 1.0.0 to 2.0.0 for net10.0

            """), result);
    }

    [Fact]
    public void GetCommitMessage_SinglePackageNoCondition_NoFrameworkSuffix()
    {
        var updateResults = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("TestPackage", "1.5.0", "2.0.0")
        };

        var result = updateResults.GetCommitMessage();

        Assert.Equal(NormalizeLineEndings("""
            Updates 1 packages:

            - Update TestPackage from 1.5.0 to 2.0.0

            """), result);
    }

    [Fact]
    public void GetTitleAndCommitMessage_RealWorldScenario_DifferentVersionTransitions()
    {
        // Arrange - net9.0 gets 1.1.17 (max supported), net10.0 gets 1.1.22
        var groupEntry = new GroupEntry("Meziantou.Extensions.Logging.Xunit.v3", []);
        var updateResults = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("Meziantou.Extensions.Logging.Xunit.v3", "1.1.16", "1.1.17", condition: "net9.0"),
            CreateUpdateCandidate("Meziantou.Extensions.Logging.Xunit.v3", "1.1.17", "1.1.22", condition: "net10.0")
        };

        // Act
        var title = groupEntry.GetTitle(updateResults);
        var commitMessage = updateResults.GetCommitMessage();

        // Assert - Title shows version range for both updates
        Assert.Equal("Bump Meziantou.Extensions.Logging.Xunit.v3 from 1.1.16-1.1.17 to 1.1.17-1.1.22", title);

        // Assert - Description shows detailed per-framework updates
        Assert.Equal(NormalizeLineEndings("""
            Updates 1 packages:

            - Update Meziantou.Extensions.Logging.Xunit.v3 from 1.1.16 to 1.1.17 for net9.0
            - Update Meziantou.Extensions.Logging.Xunit.v3 from 1.1.17 to 1.1.22 for net10.0

            """), commitMessage);
    }

    private static NugetUpdateCandidate CreateUpdateCandidate(
        string packageName,
        string? fromVersion,
        string toVersion,
        string? condition = null
    )
    {
        VersionRange? versionRange = null;
        if (fromVersion is not null)
        {
            versionRange = new VersionRange(new NuGetVersion(fromVersion));
        }

        return new NugetUpdateCandidate(
            new NugetDependency(
                new NugetFile("Test.csproj", ENugetFileType.Csproj),
                new NugetPackageReference(packageName, versionRange, condition),
                [new NugetTargetFramework(condition ?? "net10.0")]
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
    }

    private static string NormalizeLineEndings(string input)
        => input.ReplaceLineEndings();
}
