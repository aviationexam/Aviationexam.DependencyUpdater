namespace Aviationexam.DependencyUpdater.Common;

public static class GroupEntryExtensions
{
    public static string GetBranchName(
        this GroupEntry groupEntry
    ) => groupEntry.Patterns.Count == 0
        ? $"dependency-updater/group-{groupEntry.GroupName}"
        : $"dependency-updater/package-{groupEntry.GroupName}";
}
