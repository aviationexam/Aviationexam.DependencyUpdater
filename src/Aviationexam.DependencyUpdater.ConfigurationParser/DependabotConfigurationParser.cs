using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Yaml2JsonNode;
using YamlDotNet.RepresentationModel;

namespace Aviationexam.DependencyUpdater.ConfigurationParser;

public class DependabotConfigurationParser(
    IFileSystem fileSystem,
    ILogger<DependabotConfigurationParser> logger
)
{
    public DependabotConfiguration? Parse(string path)
    {
        if (!fileSystem.Exists(path))
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("File not found: {path}", path);
            }
            return null;
        }

        using var dependabotConfigurationStream = fileSystem.FileOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(dependabotConfigurationStream);

        var stream = new YamlStream();
        stream.Load(reader);

        var dependabotAsJson = stream.ToJsonNode().SingleOrDefault();
        if (dependabotAsJson is null)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("Failed to convert YAML to JSON of the {path}.", path);
            }
            return null;
        }

        var jsonDocument = JsonDocument.Parse(dependabotAsJson.ToJsonString());

        return DependabotConfiguration.FromJson(jsonDocument.RootElement);
    }
}
