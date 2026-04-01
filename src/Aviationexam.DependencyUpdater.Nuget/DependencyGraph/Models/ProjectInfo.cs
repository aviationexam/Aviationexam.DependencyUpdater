using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;

public sealed record ProjectInfo(
    string ProjectName,
    string RelativePath,
    IReadOnlyCollection<NugetDependency> PackageReferences,
    IReadOnlyCollection<ProjectReference> ProjectReferences
);
