using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Filtering;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget.Grouping;

public sealed class PackageGrouper(
    PackageFilterer packageFilterer,
    GroupResolverFactory groupResolverFactory
)
{
    public Queue<(IReadOnlyCollection<NugetUpdateCandidate> NugetUpdateCandidates, GroupEntry GroupEntry)> GroupPackagesForUpdate(
        DependencyAnalysisResult dependencyAnalysisResult,
        IReadOnlyCollection<GroupEntry> groupEntries
    )
    {
        var groupResolver = groupResolverFactory.Create(groupEntries);
        var packagesToUpdate = packageFilterer.FilterPackagesToUpdate(dependencyAnalysisResult)
            .Select(x => new
            {
                NugetUpdateCandidate = x,
                GroupEntry = groupResolver.ResolveGroup(x.NugetDependency.NugetPackage.GetPackageName()),
            })
            .GroupBy(x => x.GroupEntry, x => x.NugetUpdateCandidate);

        var groupedPackagesToUpdateQueue = new Queue<(IReadOnlyCollection<NugetUpdateCandidate> NugetUpdateCandidates, GroupEntry GroupEntry)>();

        foreach (var grouping in packagesToUpdate)
        {
            var groupEntry = grouping.Key;
            if (groupEntry == groupResolver.Empty)
            {
                foreach (var nugetUpdateCandidate in grouping)
                {
                    groupedPackagesToUpdateQueue.Enqueue((
                        [nugetUpdateCandidate],
                        new GroupEntry($"{nugetUpdateCandidate.NugetDependency.NugetPackage.GetPackageName()}/{nugetUpdateCandidate.PossiblePackageVersion.PackageVersion.GetSerializedVersion()}", [])
                    ));
                }
            }
            else
            {
                groupedPackagesToUpdateQueue.Enqueue((grouping.ToList(), groupEntry));
            }
        }

        return groupedPackagesToUpdateQueue;
    }
}
