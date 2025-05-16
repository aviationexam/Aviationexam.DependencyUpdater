namespace Aviationexam.DependencyUpdater.Common;

public static class GroupEntryExtensions
{
    public static string GetBranchName(
        this GroupEntry groupEntry,
        string updater
    ) => groupEntry.Patterns.Count == 0
        ? $"dependency-updater/{updater}/package-{groupEntry.GroupName}"
        : $"dependency-updater/{updater}/group-{groupEntry.GroupName}";
}
