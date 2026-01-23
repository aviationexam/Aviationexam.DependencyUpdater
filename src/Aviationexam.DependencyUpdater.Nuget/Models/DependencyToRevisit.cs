using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record DependencyToRevisit(
    Package Package,
    NugetPackageCondition Condition,
    IReadOnlyCollection<Package> Dependencies
);
