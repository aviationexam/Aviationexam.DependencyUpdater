using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget;

public class DirectoryPackagesPropsParser(
    IFileSystem fileSystem,
    ILogger<DirectoryPackagesPropsParser> logger
)
{
    public IEnumerable<NugetDependency> Parse(NugetFile nugetFile)
    {
        return [];
    }
}
