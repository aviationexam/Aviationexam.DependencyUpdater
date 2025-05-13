using Aviationexam.DependencyUpdater.Interfaces;
using LibGit2Sharp;

namespace Aviationexam.DependencyUpdater.Vcs.Git;

public sealed class GitSourceVersioning(
    Repository repository
) : ISourceVersioning
{
    public ISourceVersioningWorkspace CreateWorkspace(string sourceDirectory, string targetDirectory, string branchName)
    {
        var worktree = repository.Worktrees.Add(branchName, targetDirectory, isLocked: false);

        return new GitSourceVersioningWorkspace(
            worktree,
            targetDirectory
        );
    }

    public void Dispose()
    {
        repository.Dispose();
    }
}
