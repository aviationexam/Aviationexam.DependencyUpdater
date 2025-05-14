using Aviationexam.DependencyUpdater.Interfaces;
using LibGit2Sharp;
using System.IO;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Vcs.Git;

public sealed class GitSourceVersioningWorkspace(
    Repository rootRepository,
    Worktree worktree
) : ISourceVersioningWorkspace
{
    public void Dispose()
    {
        var worktreeName = worktree.Name;
        var branchName = worktree.WorktreeRepository.Head.FriendlyName;

        worktree.WorktreeRepository.Worktrees.Prune(worktree, ifLocked: false);

        var existingBranch = rootRepository.Branches.SingleOrDefault(x => x.FriendlyName == branchName);
        if (existingBranch is not null)
        {
            rootRepository.Branches.Remove(existingBranch);
        }

        existingBranch = rootRepository.Branches.SingleOrDefault(x => x.FriendlyName == worktreeName);
        if (existingBranch is not null)
        {
            rootRepository.Branches.Remove(existingBranch);
        }
    }

    public string GetWorkspaceDirectory() => worktree.WorktreeRepository.Info.WorkingDirectory;

    public bool IsPathInsideRepository(
        string fullPath
    )
    {
        var workingDirectory = GetWorkspaceDirectory();

        if (!fullPath.StartsWith(workingDirectory))
        {
            return false;
        }

        foreach (var submodule in worktree.WorktreeRepository.Submodules)
        {
            var submodulePath = Path.Combine(workingDirectory, submodule.Path);

            if (fullPath.StartsWith(submodulePath))
            {
                return false;
            }
        }

        return true;
    }

    public bool HasUncommitedChanges() => worktree.WorktreeRepository.RetrieveStatus().IsDirty;

    public void CommitChanges(string message)
    {
        //throw new System.NotImplementedException();
    }

    public void Push()
    {
        //throw new System.NotImplementedException();
    }
}
