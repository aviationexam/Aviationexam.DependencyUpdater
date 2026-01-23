using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Frameworks;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class FrameworkFlagsExtensions
{
    public static bool TryGetCompatibleFramework<T>(
        this IDictionary<NugetTargetFramework, T> targetFrameworkDict,
        NugetTargetFramework targetFramework,
        [MaybeNullWhen(false)] out T dependencyFlag
    ) => targetFrameworkDict.AsValueEnumerable().ToDictionary(
        x => x.Key.TargetFramework,
        x => x.Value
    ).TryGetCompatibleFramework(targetFramework, out dependencyFlag);

    public static bool TryGetCompatibleFramework<T>(
        this IDictionary<string, T> targetFrameworkDict,
        NugetTargetFramework targetFramework,
        [MaybeNullWhen(false)] out T dependencyFlag
    )
    {
        if (targetFrameworkDict.TryGetValue(targetFramework.TargetFramework, out dependencyFlag))
        {
            return true;
        }

        var requestedNugetFramework = NuGetFramework.Parse(
            targetFramework.TargetFramework,
            DefaultFrameworkNameProvider.Instance
        );
        foreach (var (nugetTargetFramework, flag) in targetFrameworkDict)
        {
            var nugetFramework = NuGetFramework.Parse(
                nugetTargetFramework,
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

    public static bool TryGetCompatibleFramework<T>(
        this IDictionary<NugetTargetFrameworkGroup, T> targetFrameworkDict,
        NugetTargetFramework targetFramework,
        [MaybeNullWhen(false)] out T dependencyFlag
    )
    {
        dependencyFlag = default;

        return false;
    }
}
