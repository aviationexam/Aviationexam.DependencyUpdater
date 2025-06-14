using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record NugetUpdaterContext(
    IReadOnlyCollection<NugetSource> NugetConfigurations,
    IReadOnlyCollection<NugetDependency> Dependencies
);
