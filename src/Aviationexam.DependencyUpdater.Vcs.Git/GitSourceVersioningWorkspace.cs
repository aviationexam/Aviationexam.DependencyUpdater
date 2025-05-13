using Aviationexam.DependencyUpdater.Interfaces;
using LibGit2Sharp;

namespace Aviationexam.DependencyUpdater.Vcs.Git;

public sealed class GitSourceVersioningWorkspace(
    Worktree worktree
) : ISourceVersioningWorkspace
{
    public void Dispose() => worktree.WorktreeRepository.Worktrees.Prune(worktree, ifLocked: false);
}
