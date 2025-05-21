using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

internal sealed class HierarchyQueryResponse
{
    public required DataProviderSharedData DataProviderSharedData { get; init; }
    public required IReadOnlyDictionary<string, UpstreamVersionsDataProvider> DataProviders { get; set; }
}
