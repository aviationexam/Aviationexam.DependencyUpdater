using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

internal sealed class Upstream
{
    public required string UpstreamName { get; set; }
    public required string UpstreamId { get; set; }
    public required bool IsUpstreamForLocalVersion { get; set; }
    public required int Origin { get; set; }
    public required IReadOnlyCollection<SourceChainItem> SourceChain { get; set; }
}
