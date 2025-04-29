using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget;

public static class NugetUpdaterContextExtensions
{
    private const string DefaultNugetSourceKey = "nuget.org";
    private const string DefaultNugetSourceUrl = "https://api.nuget.org/v3/index.json";

    public static IEnumerable<KeyValuePair<NugetDependency, IReadOnlyCollection<NugetSource>>> MapSourceToDependency(
        this NugetUpdaterContext context,
        ILogger logger
    )
    {
        var configurations = context.NugetConfigurations
            .SelectMany(x =>
                x.PackageMapping.Select(p => (PackageMapping: p, NugetSource: x))
            );

        var explicitMappings = new Dictionary<string, NugetSource>();
        var wildcardMappings = new List<KeyValuePair<string, NugetSource>>();

        foreach (var (packageMapping, nugetSource) in configurations)
        {
            if (packageMapping.IsWildcard())
            {
                wildcardMappings.Add(KeyValuePair.Create(packageMapping.Pattern.TrimEnd('*'), nugetSource));
            }
            else
            {
                explicitMappings.Add(packageMapping.Pattern, nugetSource);
            }
        }

        if (context.NugetConfigurations.Count == 0)
        {
            foreach (var dependency in context.Dependencies)
            {
                yield return KeyValuePair.Create<NugetDependency, IReadOnlyCollection<NugetSource>>(dependency, [
                    new NugetSource(DefaultNugetSourceKey, DefaultNugetSourceUrl, NugetSourceVersion.V3, []),
                ]);
            }

            yield break;
        }

        if (
            explicitMappings.Count == 0
            && wildcardMappings.Count == 0
        )
        {
            foreach (var dependency in context.Dependencies)
            {
                yield return KeyValuePair.Create(dependency, context.NugetConfigurations);
            }

            yield break;
        }

        var sortedWildcardMappings = wildcardMappings.OrderByDescending(x => x.Key.Length).ToList();

        foreach (var dependency in context.Dependencies)
        {
            var packageName = dependency.NugetPackage.GetPackageName();

            if (explicitMappings.TryGetValue(packageName, out var explicitNugetSource))
            {
                yield return KeyValuePair.Create<NugetDependency, IReadOnlyCollection<NugetSource>>(dependency, [explicitNugetSource]);
                continue;
            }

            var found = false;
            foreach (var (pattern, nugetSource) in sortedWildcardMappings)
            {
                if (packageName.StartsWith(pattern, StringComparison.Ordinal))
                {
                    found = true;
                    yield return KeyValuePair.Create<NugetDependency, IReadOnlyCollection<NugetSource>>(dependency, [nugetSource]);

                    break;
                }
            }

            if (found)
            {
                continue;
            }

            logger.LogWarning("Unable to find packageSource for dependency {dependencyName}", packageName);
        }
    }
}
