using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
            logger.LogError("File not found: {path}", path);
            return null;
        }

        using var dependabotConfigurationStream = fileSystem.FileOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(dependabotConfigurationStream);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        return deserializer.Deserialize<DependabotConfiguration>(reader);
    }
}
