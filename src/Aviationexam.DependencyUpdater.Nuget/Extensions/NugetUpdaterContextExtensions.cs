using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Configurations;
using Aviationexam.DependencyUpdater.Nuget.Factories;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

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
            .AsValueEnumerable()
            .SelectMany(x =>
                x.PackageMapping.AsValueEnumerable().Select(p => (PackageMapping: p, NugetSource: x))
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

        var sortedWildcardMappings = wildcardMappings.AsValueEnumerable().OrderByDescending(x => x.Key.Length).ToList();

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

    public static IEnumerable<NugetSource> GetSourcesForPackage(
        this NugetUpdaterContext context,
        string packageName,
        ILogger logger
    )
    {
        var configurations = context.NugetConfigurations
            .AsValueEnumerable()
            .SelectMany(x =>
                x.PackageMapping.AsValueEnumerable().Select(p => (PackageMapping: p, NugetSource: x))
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
            yield return new NugetSource(DefaultNugetSourceKey, DefaultNugetSourceUrl, NugetSourceVersion.V3, []);

            yield break;
        }

        if (
            explicitMappings.Count == 0
            && wildcardMappings.Count == 0
        )
        {
            foreach (var nugetConfiguration in context.NugetConfigurations)
            {
                yield return nugetConfiguration;
            }


            yield break;
        }

        var sortedWildcardMappings = wildcardMappings.AsValueEnumerable().OrderByDescending(x => x.Key.Length).ToList();


        if (explicitMappings.TryGetValue(packageName, out var explicitNugetSource))
        {
            yield return explicitNugetSource;
            yield break;
        }

        foreach (var (pattern, nugetSource) in sortedWildcardMappings)
        {
            if (packageName.StartsWith(pattern, StringComparison.Ordinal))
            {
                yield return nugetSource;

                yield break;
            }
        }

        logger.LogWarning("Unable to find packageSource for dependency {dependencyName}", packageName);
    }

    public static IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> GetCurrentPackageVersionsPerTargetFramework(
        this NugetUpdaterContext context
    )
    {
        var result = new Dictionary<string, IDictionary<string, PackageVersion>>();

        foreach (var dependency in context.Dependencies)
        {
            var packageName = dependency.NugetPackage.GetPackageName();
            var version = dependency.NugetPackage.GetVersion();

            if (version is null)
            {
                continue;
            }

            if (!result.TryGetValue(packageName, out var frameworkVersions))
            {
                frameworkVersions = new Dictionary<string, PackageVersion>();
                result[packageName] = frameworkVersions;
            }

            // Add version for each target framework this dependency applies to
            foreach (var targetFramework in dependency.TargetFrameworks)
            {
                var tfm = targetFramework.TargetFramework;

                // There should never be different versions for the same target framework - this indicates a configuration error
                if (frameworkVersions.TryGetValue(tfm, out var existingVersion))
                {
                    if (existingVersion != version)
                    {
                        throw new InvalidOperationException(
                            $"Package '{packageName}' has conflicting versions for target framework '{tfm}': " +
                            $"'{existingVersion.GetSerializedVersion()}' and '{version.GetSerializedVersion()}'. " +
                            $"Each target framework must have exactly one version of each package."
                        );
                    }
                    // Same version, skip adding it again
                }
                else
                {
                    frameworkVersions[tfm] = version;
                }
            }
        }

        return result;
    }

    public static IReadOnlyDictionary<NugetSource, NugetSourceRepository> GetSourceRepositories(
        this NugetUpdaterContext context,
        IReadOnlyCollection<NugetFeedAuthentication> nugetFeedAuthentications,
        IReadOnlyDictionary<string, string> fallbackRegistries,
        NugetVersionFetcherFactory nugetVersionFetcherFactory
    ) => context.NugetConfigurations
        .AsValueEnumerable()
        .GroupBy(x => x.Source)
        .Select(x => x.AsValueEnumerable().First())
        .ToDictionary(
            x => x,
            x => nugetVersionFetcherFactory.CreateSourceRepositories(x, fallbackRegistries, nugetFeedAuthentications)
        );
}
