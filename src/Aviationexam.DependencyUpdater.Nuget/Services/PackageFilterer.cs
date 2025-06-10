using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using NuGet.Protocol;
using System.Collections.Generic;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class PackageFilterer
{
    public IEnumerable<KeyValuePair<NugetDependency, PackageVersion<PackageSearchMetadataRegistration>>> FilterPackagesToUpdate(
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
                yield return KeyValuePair.Create(
                    dependency,
                    newPossiblePackageVersions
                        .OrderByDescending(x => x.PackageVersion)
                        .First()
                        .PackageVersion
                );
            }
        }
    }
}
