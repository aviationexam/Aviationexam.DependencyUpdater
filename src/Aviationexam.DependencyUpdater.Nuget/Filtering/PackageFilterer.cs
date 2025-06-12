using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using System.Linq;

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
                .Select(possiblePackageVersion => possiblePackageVersion with
                {
                    CompatiblePackageDependencyGroups =
                    [
                        .. possiblePackageVersion
                            .CompatiblePackageDependencyGroups
                            .Where(group => group.Packages.All(package =>
                                dependencyAnalysisResult.PackageFlags.TryGetValue(
                                    new Package(package.Id, package.VersionRange.MinVersion!.MapToPackageVersion()),
                                    out var flag
                                ) && flag is EDependencyFlag.Valid
                            )),
                    ],
                })
                .Where(x => x.CompatiblePackageDependencyGroups.Count > 0)
                .ToList();

            if (newPossiblePackageVersions.Count > 0)
            {
                var possiblePackageVersion = newPossiblePackageVersions
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
