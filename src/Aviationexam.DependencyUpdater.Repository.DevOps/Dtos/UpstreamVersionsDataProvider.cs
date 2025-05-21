using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

internal sealed class UpstreamVersionsDataProvider
{
    public required string PackageDisplayName { get; set; }
    public required string PackageNormalizedName { get; set; }
    public required bool UserHasPermissionToIngestFromUpstream { get; set; }
    public required UpstreamingBehavior UpstreamingBehavior { get; set; }
    public required bool ExternalVersionsFromUpstreamAvailable { get; set; }
    public required IReadOnlyCollection<VersionEntry> Versions { get; set; }
}
