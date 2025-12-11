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

        public IReadOnlyCollection<KeyValuePair<string, DependabotConfiguration.Registry.Entity>> ExtractFeeds(
            string registryType
        ) => config.Registries
            .Select(x => KeyValuePair.Create(x.Key.GetString(), x.Value.As<DependabotConfiguration.Registry.Entity>()))
            .Where(x => x.Value.Type.GetString() == registryType)
            .ToList();

        public IReadOnlyCollection<T> ExtractFeeds<T>(
            string registryType,
            Func<string, DependabotConfiguration.Registry.Entity, T> selector
        ) => config.ExtractFeeds(registryType)
            .Select(x => selector(x.Key, x.Value))
            .ToList();
    }
}
