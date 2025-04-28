using System;
using System.Collections.Generic;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class DependencyParser(
    CsprojParser csprojParser,
    DirectoryPackagesPropsParser directoryPackagesPropsParser
)
{
    public IEnumerable<NugetDependency> GetAllDependencies(
        IEnumerable<NugetFile> nugetFiles
    ) => nugetFiles.Aggregate<NugetFile, IEnumerable<NugetDependency>>(
        [],
        (current, nugetFile) => current.Concat(nugetFile.Type switch
        {
            ENugetFileType.Csproj => csprojParser.Parse(nugetFile),
            ENugetFileType.DirectoryPackagesProps => directoryPackagesPropsParser.Parse(nugetFile),
            ENugetFileType.NugetConfig => [],
            _ => throw new ArgumentOutOfRangeException(nameof(nugetFile.Type), nugetFile.Type, null),
        })
    );
}
