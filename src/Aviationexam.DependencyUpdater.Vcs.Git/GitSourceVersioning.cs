using Aviationexam.DependencyUpdater.Interfaces;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Vcs.Git;

public sealed class GitSourceVersioning(
    Repository repository,
    TimeProvider timeProvider,
    ILogger<GitSourceVersioning> logger
) : ISourceVersioning
{
    public ISourceVersioningWorkspace CreateWorkspace(
        string targetDirectory,
        string? sourceBranchName,
        string branchName,
        string worktreeName
    )
    {
        var existingWorktree = repository.Worktrees
            .Where(x => x is not null)
            .SingleOrDefault(x => x.Name == worktreeName);
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

        if (
            sourceBranchName is not null
            && repository.Branches.Any(x => x.FriendlyName == sourceBranchName)
        )
        {
            worktree.WorktreeRepository.Reset(ResetMode.Hard, repository.Branches[sourceBranchName].Tip);
        }

        foreach (var submodule in worktree.WorktreeRepository.Submodules)
        {
            worktree.WorktreeRepository.Submodules.Update(
                submodule.Name,
                new SubmoduleUpdateOptions
                {
                    Init = true,
                }
            );
        }

        return new GitSourceVersioningWorkspace(
            repository,
            worktree,
            timeProvider,
            logger
        );
    }

    public void Dispose()
    {
        repository.Dispose();
    }
}
