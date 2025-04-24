using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget;

public class DirectoryPackagesPropsParser(
    ILogger<DirectoryPackagesPropsParser> logger
)
{
    public IEnumerable<NugetDependency> Parse(NugetFile nugetFile)
    {
        return [];
    }
}
