using Aviationexam.DependencyUpdater.Interfaces;
using LibGit2Sharp;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Vcs.Git;

public sealed class GitSourceVersioningWorkspace(
    Worktree worktree,
    string worktreeDirectory
) : ISourceVersioningWorkspace
{
    private readonly Repository _repository = new(worktreeDirectory);

    public void Dispose()
    {
        _repository.Dispose();
        worktree.WorktreeRepository.Worktrees.Prune(worktree, ifLocked: false);
    }

    public string GetWorkspaceDirectory() => worktreeDirectory;

    public bool HasUncommitedChanges() => _repository.RetrieveStatus()
        .Modified
        .Where(x => x.FilePath.Length > 0)
        .Any();

    public void CommitChanges(string message)
    {
        //throw new System.NotImplementedException();
    }

    public void Push()
    {
        //throw new System.NotImplementedException();
    }
}
