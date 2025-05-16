using Aviationexam.DependencyUpdater.Constants;

namespace Aviationexam.DependencyUpdater.Common;

public static class GroupEntryExtensions
{
    public static string GetBranchName(
        this GroupEntry groupEntry,
        string updater
    ) => groupEntry.Patterns.Count == 0
        ? $"{GitConstants.UpdaterBranchPrefix}{updater}/package-{groupEntry.GroupName}"
        : $"{GitConstants.UpdaterBranchPrefix}{updater}/group-{groupEntry.GroupName}";
}
