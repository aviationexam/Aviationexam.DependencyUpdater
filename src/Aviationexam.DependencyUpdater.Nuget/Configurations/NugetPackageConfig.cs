using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Configurations;

public class NugetPackageConfig : PackageConfig
{
    /// <summary>
    /// The target frameworks to update packages for.
    /// </summary>
    public required IReadOnlyCollection<NugetTargetFramework> TargetFrameworks { get; init; }
}
