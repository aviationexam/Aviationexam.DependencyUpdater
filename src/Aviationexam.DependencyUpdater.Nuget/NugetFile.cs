using System.IO;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed record NugetFile(
    string RelativePath,
    ENugetFileType Type
)
{
    public string GetFullPath(
        string repositoryPath
    ) => Path.Join(repositoryPath, RelativePath);
}
