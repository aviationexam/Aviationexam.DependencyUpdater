using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

internal sealed class VersionEntry
{
    public required string DisplayVersion { get; set; }
    public required string NormalizedVersion { get; set; }
    public required IReadOnlyCollection<Upstream> Upstreams { get; set; }
    public LocalInstance? LocalInstance { get; set; }
    public required string SelectedUpstreamId { get; set; }
    public required bool IsLocal { get; set; }
}
