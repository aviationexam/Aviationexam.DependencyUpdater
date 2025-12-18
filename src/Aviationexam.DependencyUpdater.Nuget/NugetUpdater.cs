using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Configurations;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Factories;
using Aviationexam.DependencyUpdater.Nuget.Grouping;
using Aviationexam.DependencyUpdater.Nuget.Services;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class NugetUpdater(
    ISourceVersioningFactory sourceVersioningFactory,
    SubmoduleUpdater submoduleUpdater,
    NugetContextFactory contextFactory,
    NugetVersionFetcherFactory nugetVersionFetcherFactory,
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

        sourceVersioning.RunGitWorktreePrune(repositoryConfig.RepositoryPath);

        var knownPullRequests = await submoduleUpdater.UpdateSubmodulesAsync(
            sourceVersioning,
            repositoryConfig,
            authConfig,
            gitCredentialsConfiguration,
            gitMetadataConfig,
            packageConfig.ExecuteRestore,
            packageConfig.RestoreDirectory,
            updater,
            cancellationToken
        );

        var nugetUpdaterContext = contextFactory.CreateContext(repositoryConfig, packageConfig.TargetFrameworks);

        var currentPackageVersions = nugetUpdaterContext.GetCurrentPackageVersions();
        var currentPackageVersionsPerTargetFramework = nugetUpdaterContext.GetCurrentPackageVersionsPerTargetFramework();
        var sourceRepositories = nugetUpdaterContext.GetSourceRepositories(
            authConfig.NugetFeedAuthentications,
            packageConfig.FallbackRegistries,
            nugetVersionFetcherFactory
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
            packageConfig.ExecuteRestore,
            packageConfig.RestoreDirectory,
            groupedPackagesToUpdate,
            currentPackageVersionsPerTargetFramework,
            updater,
            cancellationToken
        );

        // Clean up abandoned pull requests
        await pullRequestManager.CleanupAbandonedPullRequestsAsync(
            sourceDirectory: repositoryConfig.SubdirectoryPath ?? string.Empty,
            updater,
            [.. knownPullRequests.AsValueEnumerable().Concat(updatedPullRequests)],
            cancellationToken
        );
    }
}
