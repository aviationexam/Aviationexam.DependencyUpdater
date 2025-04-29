using Aviationexam.DependencyUpdater.Interfaces;

namespace Aviationexam.DependencyUpdater.ConfigurationParser;

public sealed class ConfigurationFinder(
    IFileSystem filesystem
)
{
    public IEnumerable<string> GetAllDependabotFiles(
        string directoryPath
    )
    {
        var updaterPath = Path.Join(directoryPath, ".github/updater.yml");
        var hasUpdaterConfig = false;
        if (filesystem.Exists(updaterPath))
        {
            yield return updaterPath;
            hasUpdaterConfig = true;
        }

        updaterPath = Path.Join(directoryPath, ".azuredevops/updater.yml");
        if (filesystem.Exists(updaterPath))
        {
            yield return updaterPath;
            hasUpdaterConfig = true;
        }

        if (hasUpdaterConfig)
        {
            yield break;
        }

        var path = Path.Join(directoryPath, ".github/dependabot.yml");
        if (filesystem.Exists(path))
        {
            yield return path;
        }

        path = Path.Join(directoryPath, ".azuredevops/dependabot.yml");
        if (filesystem.Exists(path))
        {
            yield return path;
        }
    }
}
