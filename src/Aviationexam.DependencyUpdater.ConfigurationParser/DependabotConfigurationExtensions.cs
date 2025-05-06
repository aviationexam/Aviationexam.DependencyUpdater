using Corvus.Json;

namespace Aviationexam.DependencyUpdater.ConfigurationParser;

public static class DependabotConfigurationExtensions
{
    public static IReadOnlyCollection<DependabotConfiguration.Update> ExtractEcosystemUpdates(
        this DependabotConfiguration config,
        string packageEcosystem
    ) => config.Updates
        .Where(x => x.PackageEcosystem == packageEcosystem)
        .ToList();

    public static IReadOnlyCollection<KeyValuePair<string, DependabotConfiguration.Registry.Entity>> ExtractFeeds(
        this DependabotConfiguration config,
        string registryType
    ) => config.Registries
        .Select(x => KeyValuePair.Create(x.Key.GetString(), x.Value.As<DependabotConfiguration.Registry.Entity>()))
        .Where(x => x.Value.Type == registryType)
        .Where(x => x.Value.IsValid())
        .ToList();

    public static IReadOnlyCollection<T> ExtractFeeds<T>(
        this DependabotConfiguration config,
        string registryType,
        Func<string, DependabotConfiguration.Registry.Entity, T> selector
    ) => config.ExtractFeeds(registryType)
        .Select(x => selector(x.Key, x.Value))
        .ToList();
}
