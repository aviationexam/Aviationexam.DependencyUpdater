using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record DependencyToCheck(
    Package Package,
    NugetPackageCondition Condition,
    IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks
);
