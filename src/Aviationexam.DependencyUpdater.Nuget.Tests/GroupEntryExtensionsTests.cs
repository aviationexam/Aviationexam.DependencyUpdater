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
    public void GetTitle_SinglePackageSingleFramework_GeneratesCorrectTitle()
    {
        var currentPackageVersions = new Dictionary<string, IDictionary<string, PackageVersion>>
        {
            ["TestPackage"] = new Dictionary<string, PackageVersion>
            {
                ["net10.0"] = new NuGetVersion("1.0.0").MapToPackageVersion()
            }
        };

        var groupEntry = new GroupEntry("TestPackage", []);
        var nugetUpdateCandidates = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("TestPackage", "1.0.0", ["net10.0"])
        };

        var result = groupEntry.GetTitle(nugetUpdateCandidates, currentPackageVersions);

        Assert.Equal("Bump TestPackage from 1.0.0 to 2.0.0", result);
    }

    [Fact]
    public void GetTitle_SinglePackageMultipleFrameworksSameVersion_UsesCommonVersion()
    {
        var currentPackageVersions = new Dictionary<string, IDictionary<string, PackageVersion>>
        {
            ["TestPackage"] = new Dictionary<string, PackageVersion>
            {
                ["net9.0"] = new NuGetVersion("1.5.0").MapToPackageVersion(),
                ["net10.0"] = new NuGetVersion("1.5.0").MapToPackageVersion()
            }
        };

        var groupEntry = new GroupEntry("TestPackage", []);
        var nugetUpdateCandidates = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("TestPackage", "1.5.0", ["net9.0", "net10.0"])
        };

        var result = groupEntry.GetTitle(nugetUpdateCandidates, currentPackageVersions);

        Assert.Equal("Bump TestPackage from 1.5.0 to 2.0.0", result);
    }

    [Fact]
    public void GetTitle_SinglePackageMultipleFrameworksDifferentVersions_UsesMinimumVersion()
    {
        var currentPackageVersions = new Dictionary<string, IDictionary<string, PackageVersion>>
        {
            ["Meziantou.Extensions.Logging.Xunit.v3"] = new Dictionary<string, PackageVersion>
            {
                ["net9.0"] = new NuGetVersion("1.1.17").MapToPackageVersion(),
                ["net10.0"] = new NuGetVersion("1.1.21").MapToPackageVersion()
            }
        };

        var groupEntry = new GroupEntry("Meziantou.Extensions.Logging.Xunit.v3", []);
        var nugetUpdateCandidates = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("Meziantou.Extensions.Logging.Xunit.v3", "1.1.21", ["net9.0", "net10.0"])
        };

        var result = groupEntry.GetTitle(nugetUpdateCandidates, currentPackageVersions);

        Assert.Equal("Bump Meziantou.Extensions.Logging.Xunit.v3 from 1.1.17 to 2.0.0", result);
    }

    [Fact]
    public void GetTitle_SinglePackageOnlyOneFrameworkUpdating_UsesCorrectVersion()
    {
        var currentPackageVersions = new Dictionary<string, IDictionary<string, PackageVersion>>
        {
            ["TestPackage"] = new Dictionary<string, PackageVersion>
            {
                ["net9.0"] = new NuGetVersion("1.0.0").MapToPackageVersion(),
                ["net10.0"] = new NuGetVersion("2.0.0").MapToPackageVersion()
            }
        };

        var groupEntry = new GroupEntry("TestPackage", []);
        var nugetUpdateCandidates = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("TestPackage", "2.0.0", ["net10.0"])
        };

        var result = groupEntry.GetTitle(nugetUpdateCandidates, currentPackageVersions);

        Assert.Equal("Bump TestPackage from 2.0.0 to 2.0.0", result);
    }

    [Fact]
    public void GetTitle_MultiplePackages_GeneratesGroupTitle()
    {
        var currentPackageVersions = new Dictionary<string, IDictionary<string, PackageVersion>>();
        var groupEntry = new GroupEntry("TestGroup", []);
        var nugetUpdateCandidates = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("Package1", "1.0.0", ["net10.0"]),
            CreateUpdateCandidate("Package2", "2.0.0", ["net10.0"])
        };

        var result = groupEntry.GetTitle(nugetUpdateCandidates, currentPackageVersions);

        Assert.Equal("Bump TestGroup group – 2 updates", result);
    }

    [Fact]
    public void GetTitle_PackageNotInDictionary_FallsBackToNugetPackageVersion()
    {
        var currentPackageVersions = new Dictionary<string, IDictionary<string, PackageVersion>>();
        var groupEntry = new GroupEntry("NewPackage", []);
        var nugetUpdateCandidates = new List<NugetUpdateCandidate>
        {
            CreateUpdateCandidate("NewPackage", "1.0.0", ["net10.0"])
        };

        var result = groupEntry.GetTitle(nugetUpdateCandidates, currentPackageVersions);

        Assert.Equal("Bump NewPackage from 1.0.0 to 2.0.0", result);
    }

    private static NugetUpdateCandidate CreateUpdateCandidate(
        string packageName,
        string currentVersion,
        string[] targetFrameworks
    )
    {
        var targetFrameworkList = new List<NugetTargetFramework>();
        foreach (var tf in targetFrameworks)
        {
            targetFrameworkList.Add(new NugetTargetFramework(tf));
        }

        return new NugetUpdateCandidate(
            new NugetDependency(
                new NugetFile("Test.csproj", ENugetFileType.Csproj),
                new NugetPackageReference(packageName, new VersionRange(new NuGetVersion(currentVersion))),
                targetFrameworkList
            ),
            new PossiblePackageVersion(
                new PackageVersionWithDependencySets(
                    new NuGetVersion("2.0.0").MapToPackageVersion()
                )
                {
                    DependencySets = new Dictionary<EPackageSource, IReadOnlyCollection<DependencySet>>()
                },
                []
            )
        );
    }
}
