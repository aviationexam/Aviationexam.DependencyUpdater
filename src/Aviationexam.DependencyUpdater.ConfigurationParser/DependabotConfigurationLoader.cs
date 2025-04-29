namespace Aviationexam.DependencyUpdater.ConfigurationParser;

public class DependabotConfigurationLoader(
    ConfigurationFinder configurationFinder,
    DependabotConfigurationParser dependabotConfigurationParser
)
{
    public IReadOnlyCollection<DependabotConfiguration> LoadConfiguration(
        string directoryPath
    ) => configurationFinder
        .GetAllDependabotFiles(directoryPath)
        .Select(dependabotConfigurationParser.Parse)
        .Where(x => x is not null)
        .Select(x => x!.Value)
        .ToList();
}
