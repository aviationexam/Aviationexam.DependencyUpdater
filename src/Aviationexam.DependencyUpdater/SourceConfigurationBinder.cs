using Aviationexam.DependencyUpdater.Common;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater;

public sealed class SourceConfigurationBinder(
    Option<string> directoryArgument
) : IBinder<SourceConfiguration>
{
    public SourceConfiguration CreateValue(
        ParseResult parseResult
    ) => new()
    {
        Directory = parseResult.GetRequiredValue(directoryArgument),
    };
}
