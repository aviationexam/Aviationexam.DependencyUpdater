namespace Aviationexam.DependencyUpdater.Common;

public static class BranchNameGenerator
{
    public const string UpdaterBranchPrefix = "dependency-updater";

    public static string GetBranchNamePrefix(
        string? sourceDirectory,
        string updater
    )
    {
        if (sourceDirectory is "/")
        {
            sourceDirectory = null;
        }

        return string.Join(
            '/',
            UpdaterBranchPrefix,
            updater,
            sourceDirectory ?? "no-subdirectory"
        );
    }

    public static string GetBranchName(
        GroupEntry groupEntry,
        RepositoryConfig repositoryConfig,
        string updater
    ) => string.Join(
        '/',
        GetBranchNamePrefix(repositoryConfig.SubdirectoryPath, updater),
        groupEntry.Patterns.Count == 0
            ? $"package-{groupEntry.GroupName}"
            : $"group-{groupEntry.GroupName}"
    );

    public static string GetBranchNameForSubmodule(
        string submoduleName,
        RepositoryConfig repositoryConfig,
        string updater
    ) => string.Join(
        '/',
        GetBranchNamePrefix(repositoryConfig.SubdirectoryPath, updater),
        "submodule",
        submoduleName
    );
}
