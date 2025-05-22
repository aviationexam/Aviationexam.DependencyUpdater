using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;

namespace Aviationexam.DependencyUpdater;

public static class DirectiveConfigurationExtensions
{
    public static IConfigurationBuilder AddCommandLineDirectives(
        this IConfigurationBuilder config, ParseResult commandline,
        string name
    )
    {
        ArgumentNullException.ThrowIfNull(commandline);
        ArgumentNullException.ThrowIfNull(name);

        if (!commandline.Directives.TryGetValues(name, out var directives))
        {
            return config;
        }

        return config.AddInMemoryCollection(directives.Select(s =>
        {
            var parts = s.Split(['='], count: 2);

            return new KeyValuePair<string, string?>(
                parts[0],
                parts.Length > 1 ? parts[1] : null
            );
        }));
    }
}
