using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

internal class HierarchyQueryRequest
{
    public required IReadOnlyCollection<string> ContributionIds { get; set; }
    public required DataProviderContext DataProviderContext { get; set; }
}
