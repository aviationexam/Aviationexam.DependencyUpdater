using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Filtering;

public sealed class PackageFilterer
{
    public IEnumerable<NugetUpdateCandidate> FilterPackagesToUpdate(
        DependencyAnalysisResult dependencyAnalysisResult
    )
    {
        foreach (var (dependency, possiblePackageVersions) in dependencyAnalysisResult.DependenciesToUpdate)
        {
            var newPossiblePackageVersions = possiblePackageVersions
                .AsValueEnumerable()
                .Select(possiblePackageVersion => possiblePackageVersion with
                {
                    CompatiblePackageDependencyGroups =
                    [
                        .. possiblePackageVersion
                            .CompatiblePackageDependencyGroups
                            .AsValueEnumerable()
                            .Where(group => group.Packages.AsValueEnumerable().All(package =>
                                // Check if all target frameworks have valid flags for this package
                                dependencyAnalysisResult.PackageFlags.TryGetValue(new Package(package.Id, package.VersionRange.MinVersion!.MapToPackageVersion()), out var frameworkFlags)
                                // Ensure all target frameworks for this dependency have valid flags
                                && frameworkFlags.TryGetValue(new NugetTargetFramework(group.TargetFramework.GetShortFolderName()), out var flag)
                                && flag is EDependencyFlag.Valid
                            )),
                    ],
                })
                .Where(x => x.CompatiblePackageDependencyGroups.Count > 0)
                .ToList();

            if (newPossiblePackageVersions.Count > 0)
            {
                var possiblePackageVersion = newPossiblePackageVersions
                    .AsValueEnumerable()
                    .OrderByDescending(x => x.PackageVersion)
                    .First();

                yield return new NugetUpdateCandidate(
                    dependency,
                    possiblePackageVersion
                );
            }
        }
    }
}
