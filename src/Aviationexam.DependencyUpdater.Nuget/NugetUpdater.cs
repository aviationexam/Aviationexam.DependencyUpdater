using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Grouping;
using Aviationexam.DependencyUpdater.Nuget.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class NugetUpdater(
    ISourceVersioningFactory sourceVersioningFactory,
    SubmoduleUpdater submoduleUpdater,
    NugetContextFactory contextFactory,
    DependencyAnalyzer dependencyAnalyzer,
    PackageGrouper packageGrouper,
    PackageUpdater packageUpdater,
    PullRequestManager pullRequestManager
)
{
    public async Task ProcessUpdatesAsync(
        RepositoryConfig repositoryConfig,
        GitCredentialsConfiguration gitCredentialsConfiguration,
        GitMetadataConfig gitMetadataConfig,
        NugetPackageConfig packageConfig,
        NugetAuthConfig authConfig,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        const string updater = "nuget";

        using var sourceVersioning = sourceVersioningFactory.CreateSourceVersioning(repositoryConfig.RepositoryPath);

        var knownPullRequests = await submoduleUpdater.UpdateSubmodulesAsync(
            sourceVersioning,
            repositoryConfig,
            authConfig,
            gitCredentialsConfiguration,
            gitMetadataConfig,
            updater,
            cancellationToken
        );

        var nugetUpdaterContext = contextFactory.CreateContext(repositoryConfig, packageConfig.TargetFrameworks);

        var currentPackageVersions = nugetUpdaterContext.GetCurrentPackageVersions();
        var sourceRepositories = nugetUpdaterContext.GetSourceRepositories(
            authConfig.NugetFeedAuthentications,
            packageConfig.FallbackRegistries
        );

        // Analyze dependencies
        var dependencyAnalysisResult = await dependencyAnalyzer.AnalyzeDependenciesAsync(
            nugetUpdaterContext,
            sourceRepositories,
            packageConfig.IgnoreEntries,
            currentPackageVersions,
            cachingConfiguration,
            cancellationToken
        );

        // Group packages for updates
        var groupedPackagesToUpdate = packageGrouper.GroupPackagesForUpdate(
            dependencyAnalysisResult,
            packageConfig.GroupEntries
        );

        // Process package updates and create pull requests
        var updatedPullRequests = await packageUpdater.ProcessPackageUpdatesAsync(
            sourceVersioning,
            repositoryConfig,
            authConfig,
            gitCredentialsConfiguration,
            gitMetadataConfig,
            groupedPackagesToUpdate,
            currentPackageVersions,
            updater,
            cancellationToken
        ).ToListAsync(cancellationToken);

        knownPullRequests = knownPullRequests.Concat(updatedPullRequests);

        // Clean up abandoned pull requests
        await pullRequestManager.CleanupAbandonedPullRequestsAsync(
            updater,
            knownPullRequests.ToList(),
            cancellationToken
        );
    }
}
