using Aviationexam.DependencyUpdater.Interfaces;
using LibGit2Sharp;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Vcs.Git;

public sealed class GitSourceVersioning(
    Repository repository
) : ISourceVersioning
{
    public ISourceVersioningWorkspace CreateWorkspace(
        string targetDirectory,
        string branchName,
        string worktreeName
    )
    {
        var existingWorktree = repository.Worktrees.SingleOrDefault(x => x.Name == worktreeName);
        if (existingWorktree is not null)
        {
            repository.Worktrees.Prune(existingWorktree, ifLocked: false);
        }

        var existingBranch = repository.Branches.SingleOrDefault(x => x.FriendlyName == branchName);
        if (existingBranch is not null)
        {
            repository.Branches.Remove(existingBranch);
        }

        existingBranch = repository.Branches.SingleOrDefault(x => x.FriendlyName == worktreeName);
        if (existingBranch is not null)
        {
            repository.Branches.Remove(existingBranch);
        }

        var worktree = repository.Worktrees.Add(name: worktreeName, path: targetDirectory, isLocked: false);
        worktree.WorktreeRepository.Branches.Rename(worktreeName, branchName);

        return new GitSourceVersioningWorkspace(
            repository,
            worktree
        );
    }

    public void Dispose()
    {
        repository.Dispose();
    }
}
