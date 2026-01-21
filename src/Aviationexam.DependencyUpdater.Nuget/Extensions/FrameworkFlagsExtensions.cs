using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Frameworks;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class FrameworkFlagsExtensions
{
    public static bool TryGetCompatibleFramework(
        this IDictionary<NugetTargetFramework, EDependencyFlag> frameworkFlags,
        NugetTargetFramework targetFramework,
        out EDependencyFlag dependencyFlag
    )
    {
        if (frameworkFlags.TryGetValue(targetFramework, out dependencyFlag))
        {
            return true;
        }

        var requestedNugetFramework = NuGetFramework.Parse(
            targetFramework.TargetFramework,
            DefaultFrameworkNameProvider.Instance
        );
        foreach (var (nugetTargetFramework, flag) in frameworkFlags)
        {
            var nugetFramework = NuGetFramework.Parse(
                nugetTargetFramework.TargetFramework,
                DefaultFrameworkNameProvider.Instance
            );

            if (DefaultCompatibilityProvider.Instance.IsCompatible(requestedNugetFramework, nugetFramework))
            {
                dependencyFlag = flag;
                return true;
            }
        }

        return false;
    }
}
