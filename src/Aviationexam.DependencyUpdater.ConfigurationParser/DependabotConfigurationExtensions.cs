using System.Text.Json;

namespace Aviationexam.DependencyUpdater.ConfigurationParser;

public static class DependabotConfigurationExtensions
{
    extension(DependabotConfiguration config)
    {
        public IReadOnlyCollection<DependabotConfiguration.Update> ExtractEcosystemUpdates(
            string packageEcosystem
        ) => config.Updates
            .Where(x => x.PackageEcosystem.GetString() == packageEcosystem)
            .ToList();

        public IReadOnlyCollection<KeyValuePair<string, DependabotConfiguration.Registry.AdditionalPropertiesEntity>> ExtractFeeds(
            string registryType
        ) => config.Registries.ValueKind is JsonValueKind.Object
            ? config.Registries
                .Select(x => KeyValuePair.Create(x.Key.GetString(), x.Value.As<DependabotConfiguration.Registry.AdditionalPropertiesEntity>()))
                .Where(x => x.Value.Type.GetString() == registryType)
                .ToList()
            : [];

        public IReadOnlyCollection<T> ExtractFeeds<T>(
            string registryType,
            Func<string, DependabotConfiguration.Registry.AdditionalPropertiesEntity, T> selector
        ) => config.ExtractFeeds(registryType)
            .Select(x => selector(x.Key, x.Value))
            .ToList();
    }
}
