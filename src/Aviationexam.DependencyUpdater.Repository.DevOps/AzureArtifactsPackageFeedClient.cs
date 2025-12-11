using Aviationexam.DependencyUpdater.Repository.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public sealed class AzureArtifactsPackageFeedClient(
    AzureDevOpsUndocumentedClient azureDevOpsUndocumentedClient
) : IPackageFeedClient
{
    public async Task EnsurePackageVersionIsAvailableAsync(
        string packageName,
        string packageVersion,
        CancellationToken cancellationToken
    )
    {
        var versions = await azureDevOpsUndocumentedClient.GetContributionHierarchyQueryAsync(
            packageName,
            cancellationToken
        );

        if (versions is null)
        {
            return;
        }

        var version = versions.AsValueEnumerable().SingleOrDefault(x => x.NormalizedVersion == packageVersion);

        if (version?.IsLocal is false or null)
        {
            await azureDevOpsUndocumentedClient.ManualUpstreamIngestionAsync(
                packageName,
                packageVersion,
                cancellationToken
            );
        }
    }
}
