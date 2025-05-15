using Aviationexam.DependencyUpdater.Interfaces;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Vcs.Git;

public sealed class GitSourceVersioningWorkspace(
    Repository rootRepository,
    Worktree worktree,
    TimeProvider timeProvider,
    ILogger logger
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

    public string GetBranchName() => worktree.WorktreeRepository.Head.FriendlyName;

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
            var submodulePath = Path.Join(workingDirectory, submodule.Path);

            if (fullPath.StartsWith(submodulePath))
            {
                return false;
            }
        }

        return true;
    }

    public bool HasUncommitedChanges() => worktree.WorktreeRepository.RetrieveStatus().IsDirty;

    public void CommitChanges(
        string message,
        string authorName,
        string authorEmail
    )
    {
        var repo = worktree.WorktreeRepository;

        // Stage all changes (modified, added, removed)
        Commands.Stage(repo, "*");

        // Check if there is anything to commit
        if (!repo.RetrieveStatus().IsDirty)
        {
            return;
        }

        // Create the commit
        var signature = new Signature(
            name: authorName,
            email: authorEmail,
            timeProvider.GetUtcNow()
        );

        repo.Commit(message, signature, signature);
    }

    public void TryPullRebase(
        string? sourceBranchName,
        string authorName,
        string authorEmail
    )
    {
        var branch = worktree.WorktreeRepository.Head;
        var originalHead = branch.Tip;

        var remote = worktree.WorktreeRepository.Network.Remotes["origin"];

        Commands.Fetch(worktree.WorktreeRepository, remote.Name, [
            $"+refs/heads/{branch.FriendlyName}:refs/remotes/origin/{branch.FriendlyName}",
            $"+refs/heads/{sourceBranchName}:refs/remotes/origin/{sourceBranchName}",
        ], new FetchOptions
        {
            CredentialsProvider = (_, _, _) => new DefaultCredentials(),
        }, null);

        var sourceBranch = worktree.WorktreeRepository.Branches[$"origin/{sourceBranchName}"];
        if (sourceBranch is null)
        {
            logger.LogWarning("Target branch {SourceBranchName} not found locally", sourceBranchName);
            return; // remote branch doesn't exist
        }

        logger.LogInformation("Rebasing {CurrentBranch} onto {SourceBranch}", branch.FriendlyName, sourceBranchName);

        try
        {
            var options = new RebaseOptions
            {
                FileConflictStrategy = CheckoutFileConflictStrategy.Theirs,
            };

            // Create the commit
            var identity = new Identity(
                name: authorName,
                email: authorEmail
            );

            var rebaseResult = worktree.WorktreeRepository.Rebase.Start(branch, sourceBranch, sourceBranch, identity, options);
            logger.LogInformation("Rebase status: {RebaseResult}", rebaseResult.Status);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Rebase failed — restoring HEAD and workspace");

            worktree.WorktreeRepository.Rebase.Abort();
            worktree.WorktreeRepository.Reset(ResetMode.Hard, originalHead);
            Commands.Checkout(worktree.WorktreeRepository, originalHead);
        }
    }

    public void Push()
    {
        var repository = worktree.WorktreeRepository;

        var canonicalName = repository.Head.CanonicalName;

        repository.Network.Push(
            remote: repository.Network.Remotes["origin"],
            pushRefSpec: $"+{canonicalName}:{canonicalName}",
            pushOptions: new PushOptions
            {
                CredentialsProvider = (_, _, _) => new DefaultCredentials(),
            }
        );
    }
}
