using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget;

public class NugetPackageConfig : PackageConfig
{
    /// <summary>
    /// The target frameworks to update packages for.
    /// </summary>
    public required IReadOnlyCollection<NugetTargetFramework> TargetFrameworks { get; init; }
}
