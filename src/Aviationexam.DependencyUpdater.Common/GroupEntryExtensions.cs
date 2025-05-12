namespace Aviationexam.DependencyUpdater.Common;

public static class GroupEntryExtensions
{
    public static string GetBranchName(
        this GroupEntry groupEntry
    ) => $"dependency-updater/group-{groupEntry.GroupName}";
}
