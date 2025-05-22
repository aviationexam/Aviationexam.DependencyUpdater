using Aviationexam.DependencyUpdater;
using System.CommandLine;
using System.IO;


var directoryOption = new Option<string>(
    name: "--directory",
    description: "The directory containing the repository to update dependencies in",
    getDefaultValue: () => Directory.GetCurrentDirectory()
);

var rootCommand = new RootCommand("Dependency updater tool that processes dependency updates based on configuration files.")
    {
        directoryOption
    };

rootCommand.SetHandler(async (directory) =>
{
    await DefaultCommandHandler.ExecuteAsync(args, directory);
}, directoryOption);

return await rootCommand.InvokeAsync(args);

