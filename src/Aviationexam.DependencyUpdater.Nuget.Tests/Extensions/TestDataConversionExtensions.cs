using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using System.Collections.Generic;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Tests.Extensions;

/// <summary>
/// Extension methods for converting test data structures to the types used by DependencyUpdateProcessor.
/// </summary>
internal static class TestDataConversionExtensions
{
    /// <summary>
    /// Converts test data to CurrentPackageVersions, extracting the current version from each NugetDependency.
    /// </summary>
    public static CurrentPackageVersions ToCurrentPackageVersions(
        this IReadOnlyCollection<KeyValuePair<NugetDependency, IReadOnlyCollection<PackageVersionWithDependencySets>>> dependencies
    )
    {
        var result = new CurrentPackageVersions();

        foreach (var (dependency, _) in dependencies)
        {
            if (dependency.NugetPackage.GetVersion() is not { } version)
            {
                continue;
            }

            var packageName = dependency.NugetPackage.GetPackageName();
            var condition = dependency.NugetPackage.GetCondition();
            var targetFrameworkGroup = new NugetTargetFrameworkGroup(dependency.TargetFrameworks);

            result.SetVersion(packageName, condition, targetFrameworkGroup, version);
        }

        return result;
    }

    /// <summary>
    /// Converts test data to the dictionary format expected by ProcessDependenciesToUpdate.
    /// </summary>
    public static IReadOnlyDictionary<UpdateCandidate, IReadOnlyCollection<PossiblePackageVersion>> ToDependenciesToUpdate(
        this IReadOnlyCollection<KeyValuePair<NugetDependency, IReadOnlyCollection<PackageVersionWithDependencySets>>> dependencies
    )
    {
        var result = new Dictionary<UpdateCandidate, IReadOnlyCollection<PossiblePackageVersion>>();

        foreach (var (dependency, possibleVersions) in dependencies)
        {
            var currentVersion = dependency.NugetPackage.GetVersion();
            var targetFrameworkGroup = new NugetTargetFrameworkGroup(dependency.TargetFrameworks);

            var updateCandidate = new UpdateCandidate(
                dependency,
                currentVersion,
                targetFrameworkGroup
            );

            var possiblePackageVersions = possibleVersions
                .AsValueEnumerable()
                .Select(v => new PossiblePackageVersion(v, v.DependencySets
                    .AsValueEnumerable()
                    .SelectMany(ds => ds.Value)
                    .ToList()))
                .ToList();

            result[updateCandidate] = possiblePackageVersions;
        }

        return result;
    }
}
