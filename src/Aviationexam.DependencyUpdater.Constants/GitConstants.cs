namespace Aviationexam.DependencyUpdater.Constants;

public static class GitConstants
{
    public const string DefaultRemote = "origin";
    public const string HeadsPrefix = "refs/heads/";
    public const string UpdaterBranchPrefix = "dependency-updater/";

    public static string RemoteRef(string name) => $"refs/remotes/{name}/";
}
