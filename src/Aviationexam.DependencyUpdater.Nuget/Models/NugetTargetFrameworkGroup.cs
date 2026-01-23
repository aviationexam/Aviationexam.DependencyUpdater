using NuGet.Frameworks;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record NugetTargetFrameworkGroup(
    IReadOnlyCollection<NugetTargetFramework> TargetFrameworks
)
{
    private IReadOnlyDictionary<NugetTargetFramework, NuGetFramework> NugetFrameworks
    {
        get => field ??= TargetFrameworks.AsValueEnumerable().ToDictionary(
            x => x,
            x => NuGetFramework.Parse(
                x.TargetFramework,
                DefaultFrameworkNameProvider.Instance
            )
        );
    } = null;

    public bool CanBeUsedWith(
        string targetFrameworkTargetFramework,
        [MaybeNullWhen(false)] out NugetTargetFramework matchedTargetFramework
    )
    {
        var requestedNugetFramework = NuGetFramework.Parse(
            targetFrameworkTargetFramework,
            DefaultFrameworkNameProvider.Instance
        );

        foreach (var (targetFramework, nugetFramework) in NugetFrameworks)
        {
            if (DefaultCompatibilityProvider.Instance.IsCompatible(requestedNugetFramework, nugetFramework))
            {
                matchedTargetFramework = targetFramework;
                return true;
            }
        }

        matchedTargetFramework = null;
        return false;
    }
}
