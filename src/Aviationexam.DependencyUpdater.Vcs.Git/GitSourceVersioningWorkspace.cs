using Aviationexam.DependencyUpdater.Constants;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Vcs.Git.Extensions;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Vcs.Git;

public sealed class GitSourceVersioningWorkspace(
    Lock gitRepositoryLock,
    GitCredentialsConfiguration gitCredentials,
    Repository rootRepository,
    Worktree worktree,
    TimeProvider timeProvider,
    ILogger logger
) : ISourceVersioningWorkspace
{
    private readonly Repository _worktreeRepository = worktree.WorktreeRepository;

    public void Dispose()
    {
        lock (gitRepositoryLock)
        {
            var worktreeName = worktree.Name;
            var branchName = _worktreeRepository.Head.FriendlyName;

            _worktreeRepository.Worktrees.Prune(worktree, ifLocked: false);

            var existingBranch = rootRepository.Branches.AsValueEnumerable().SingleOrDefault(x => x.FriendlyName == branchName);
            if (existingBranch is not null)
            {
                rootRepository.Branches.Remove(existingBranch);
            }

            existingBranch = rootRepository.Branches.AsValueEnumerable().SingleOrDefault(x => x.FriendlyName == worktreeName);
            if (existingBranch is not null)
            {
                rootRepository.Branches.Remove(existingBranch);
            }

            _worktreeRepository.Dispose();
        }
    }

    public string GetWorkspaceDirectory() => _worktreeRepository.Info.WorkingDirectory;

    public string GetBranchName() => _worktreeRepository.Head.FriendlyName;

    public bool IsPathInsideRepository(
        string fullPath
    )
    {
        var workingDirectory = GetWorkspaceDirectory();

        if (!fullPath.StartsWith(workingDirectory))
        {
            return false;
        }

        foreach (var submodule in _worktreeRepository.Submodules)
        {
            var submodulePath = Path.Join(workingDirectory, submodule.Path);

            if (fullPath.StartsWith(submodulePath))
            {
                return false;
            }
        }

        return true;
    }

    public void UpdateSubmodule(
        string submodule,
        string branch
    )
    {
        var submoduleInstance = _worktreeRepository.Submodules[submodule];

        using var submoduleRepository = new Repository(Path.Join(_worktreeRepository.Info.WorkingDirectory, submoduleInstance.Path));

        var remote = submoduleRepository.Network.Remotes[GitConstants.DefaultRemote];

        lock (gitRepositoryLock)
        {
            Commands.Fetch(submoduleRepository, remote.Name, [
                $"+{GitConstants.HeadsPrefix}{branch}:{GitConstants.RemoteRef(GitConstants.DefaultRemote)}{branch}",
            ], new FetchOptions
            {
                CredentialsProvider = (_, _, _) => gitCredentials.ToGitCredentials(),
            }, logMessage: null);
        }

        var upstreamBranch = submoduleRepository.Branches[$"{GitConstants.DefaultRemote}/{branch}"];

        if (upstreamBranch is not null)
        {
            submoduleRepository.Reset(ResetMode.Hard, upstreamBranch.Tip);
        }
    }

    public bool HasUncommitedChanges() => _worktreeRepository.RetrieveStatus().IsDirty;

    public void CommitChanges(
        string message,
        string authorName,
        string authorEmail
    )
    {
        lock (gitRepositoryLock)
        {
            // Stage all changes (modified, added, removed)
            Commands.Stage(_worktreeRepository, "*");

            // Check if there is anything to commit
            if (!_worktreeRepository.RetrieveStatus().IsDirty)
            {
                return;
            }

            // Create the commit
            var signature = new Signature(
                name: authorName,
                email: authorEmail,
                timeProvider.GetUtcNow()
            );

            _worktreeRepository.Commit(message, signature, signature);
        }
    }

    public void TryPullRebase(
        string? sourceBranchName,
        string authorName,
        string authorEmail
    )
    {
        // Check for unstaged changes before attempting rebase
        if (HasUncommitedChanges())
        {
            logger.LogWarning("Unstaged changes exist in workdir, skipping rebase for branch {Branch}", _worktreeRepository.Head.FriendlyName);

            return;
        }

        var branch = _worktreeRepository.Head;
        var originalHead = branch.Tip;

        var remote = _worktreeRepository.Network.Remotes[GitConstants.DefaultRemote];

        lock (gitRepositoryLock)
        {
            Commands.Fetch(_worktreeRepository, remote.Name, [
                $"+{GitConstants.HeadsPrefix}{branch.FriendlyName}:{GitConstants.RemoteRef(GitConstants.DefaultRemote)}{branch.FriendlyName}",
                $"+{GitConstants.HeadsPrefix}{sourceBranchName}:{GitConstants.RemoteRef(GitConstants.DefaultRemote)}{sourceBranchName}",
            ], new FetchOptions
            {
                CredentialsProvider = (_, _, _) => gitCredentials.ToGitCredentials(),
            }, logMessage: null);
        }

        var upstreamBranch = _worktreeRepository.Branches[$"{GitConstants.DefaultRemote}/{branch.FriendlyName}"];

        if (upstreamBranch is not null)
        {
            _worktreeRepository.Reset(ResetMode.Hard, upstreamBranch.Tip);
        }

        var sourceBranch = _worktreeRepository.Branches[$"{GitConstants.DefaultRemote}/{sourceBranchName}"];
        if (sourceBranch is null)
        {
            logger.LogWarning("Target branch {SourceBranchName} not found locally", sourceBranchName);
            return; // remote branch doesn't exist
        }

        logger.LogInformation("Rebasing {CurrentBranch} onto {SourceBranch}", branch.FriendlyName, sourceBranchName);

        lock (gitRepositoryLock)
        {
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

                var rebaseResult = _worktreeRepository.Rebase.Start(_worktreeRepository.Head, sourceBranch, sourceBranch, identity, options);
                logger.LogInformation("Rebase status: {RebaseResult}", rebaseResult.Status);

                if (rebaseResult.Status is RebaseStatus.Conflicts)
                {
                    ResetToOriginalHead(_worktreeRepository, originalHead, branch);
                }
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Rebase failed — restoring HEAD and workspace");

                ResetToOriginalHead(_worktreeRepository, originalHead, branch);
            }
        }
    }

    private void ResetToOriginalHead(Repository repository, Commit originalHead, Branch branch)
    {
        try
        {
            // Only abort rebase if a rebase is in progress
            var gitDir = repository.Info.Path;
            var rebaseMerge = Path.Combine(gitDir, "rebase-merge");
            var rebaseApply = Path.Combine(gitDir, "rebase-apply");

            if (
                Directory.Exists(rebaseMerge)
                || Directory.Exists(rebaseApply)
            )
            {
                repository.Rebase.Abort();
            }
        }
        catch (NotFoundException)
        {
            // No rebase in progress, ignore
        }

        repository.Reset(ResetMode.Hard, originalHead);
        Commands.Checkout(repository, branch);
    }

    public bool Push(string? sourceBranchName)
    {
        var canonicalName = _worktreeRepository.Head.CanonicalName;

        var sourceBranch = _worktreeRepository.Branches[$"{GitConstants.DefaultRemote}/{sourceBranchName}"];
        var remotePushedBranch = _worktreeRepository.Branches[$"{GitConstants.DefaultRemote}/{_worktreeRepository.Head.FriendlyName}"];
        var pushedBranch = _worktreeRepository.Branches[_worktreeRepository.Head.FriendlyName];

        if (
            remotePushedBranch is null
            && sourceBranch.Tip.Equals(pushedBranch.Tip)
        )
        {
            logger.LogInformation("Branch {BranchName} is empty, skipping push", canonicalName);

            return false;
        }

        logger.LogInformation("Push {BranchName}", canonicalName);

        _worktreeRepository.Network.Push(
            remote: _worktreeRepository.Network.Remotes[GitConstants.DefaultRemote],
            pushRefSpec: $"+{canonicalName}:{canonicalName}",
            pushOptions: new PushOptions
            {
                CredentialsProvider = (_, _, _) => gitCredentials.ToGitCredentials(),
            }
        );

        return true;
    }
}
