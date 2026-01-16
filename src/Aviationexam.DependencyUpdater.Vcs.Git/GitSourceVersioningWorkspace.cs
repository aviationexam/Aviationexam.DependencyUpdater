using Aviationexam.DependencyUpdater.Constants;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Vcs.Git.Extensions;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
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
    private Repository _worktreeRepository = worktree.WorktreeRepository;

    public void Dispose()
    {
        lock (gitRepositoryLock)
        {
            var worktreeName = worktree.Name;
            if (Directory.Exists(GetWorkspaceDirectory()))
            {
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
            }

            _worktreeRepository.Dispose();
        }
    }

    public string GetWorkspaceDirectory()
    {
        lock (gitRepositoryLock)
        {
            return _worktreeRepository.Info.WorkingDirectory;
        }
    }

    public string GetBranchName()
    {
        lock (gitRepositoryLock)
        {
            return _worktreeRepository.Head.FriendlyName;
        }
    }

    public bool IsPathInsideRepository(
        string fullPath
    )
    {
        var workingDirectory = GetWorkspaceDirectory();

        if (!fullPath.StartsWith(workingDirectory))
        {
            return false;
        }

        lock (gitRepositoryLock)
        {
            foreach (var submodule in _worktreeRepository.Submodules)
            {
                var submodulePath = Path.Join(workingDirectory, submodule.Path);

                if (fullPath.StartsWith(submodulePath))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void UpdateSubmodule(
        string submodule,
        string branch
    )
    {
        lock (gitRepositoryLock)
        {
            var submoduleInstance = _worktreeRepository.Submodules[submodule];

            using var submoduleRepository = new Repository(Path.Join(_worktreeRepository.Info.WorkingDirectory, submoduleInstance.Path));

            var remote = submoduleRepository.Network.Remotes[GitConstants.DefaultRemote];

            Commands.Fetch(submoduleRepository, remote.Name, [
                $"+{GitConstants.HeadsPrefix}{branch}:{GitConstants.RemoteRef(GitConstants.DefaultRemote)}{branch}",
            ], new FetchOptions
            {
                CredentialsProvider = (_, _, _) => gitCredentials.ToGitCredentials(),
            }, logMessage: null);

            var upstreamBranch = submoduleRepository.Branches[$"{GitConstants.DefaultRemote}/{branch}"];

            if (upstreamBranch is not null)
            {
                submoduleRepository.Reset(ResetMode.Hard, upstreamBranch.Tip);
            }
        }
    }

    public bool HasUncommitedChanges()
    {
        lock (gitRepositoryLock)
        {
            return _worktreeRepository.RetrieveStatus().IsDirty;
        }
    }

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
            _worktreeRepository = worktree.WorktreeRepository;
        }
    }

    public void TryPullRebase(
        string? sourceBranchName,
        string authorName,
        string authorEmail
    )
    {
        if (GetWorkspaceDirectory() is { } dir && !Directory.Exists(dir))
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Workspace Directory {WorkspaceDirectory} does not exists", dir);
            }
            return;
        }

        // Check for unstaged changes before attempting rebase
        if (HasUncommitedChanges())
        {
            lock (gitRepositoryLock)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Unstaged changes exist in workdir, skipping rebase for branch {Branch}", _worktreeRepository.Head.FriendlyName);
                }
            }

            return;
        }

        lock (gitRepositoryLock)
        {
            var branch = _worktreeRepository.Head;
            var originalHead = branch.Tip;

            var remote = _worktreeRepository.Network.Remotes[GitConstants.DefaultRemote];

            Commands.Fetch(_worktreeRepository, remote.Name, [
                $"+{GitConstants.HeadsPrefix}{branch.FriendlyName}:{GitConstants.RemoteRef(GitConstants.DefaultRemote)}{branch.FriendlyName}",
                $"+{GitConstants.HeadsPrefix}{sourceBranchName}:{GitConstants.RemoteRef(GitConstants.DefaultRemote)}{sourceBranchName}",
            ], new FetchOptions
            {
                CredentialsProvider = (_, _, _) => gitCredentials.ToGitCredentials(),
            }, logMessage: null);

            _worktreeRepository = worktree.WorktreeRepository;

            var upstreamBranch = _worktreeRepository.Branches[$"{GitConstants.DefaultRemote}/{branch.FriendlyName}"];

            if (upstreamBranch is not null)
            {
                _worktreeRepository.Reset(ResetMode.Hard, upstreamBranch.Tip);
                _worktreeRepository = worktree.WorktreeRepository;
            }

            var sourceBranch = _worktreeRepository.Branches[$"{GitConstants.DefaultRemote}/{sourceBranchName}"];
            if (sourceBranch is null)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Target branch {SourceBranchName} not found locally", sourceBranchName);
                }
                return; // remote branch doesn't exist
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Rebasing {CurrentBranch} onto {SourceBranch}", branch.FriendlyName, sourceBranchName);
            }

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
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Rebase status: {RebaseResult}", rebaseResult.Status);
                }

                if (rebaseResult.Status is RebaseStatus.Conflicts)
                {
                    ResetToOriginalHead(_worktreeRepository, originalHead, branch);
                }

                _worktreeRepository = worktree.WorktreeRepository;
            }
            catch (Exception exception)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(exception, "Rebase failed — restoring HEAD and workspace");
                }

                ResetToOriginalHead(_worktreeRepository, originalHead, branch);
                _worktreeRepository = worktree.WorktreeRepository;
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
        lock (gitRepositoryLock)
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
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Branch {BranchName} is empty, skipping push", canonicalName);
                }

                return false;
            }

            if (
                remotePushedBranch is not null
                && AreBranchesEquivalent(pushedBranch, remotePushedBranch, sourceBranch)
            )
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Branch {BranchName} has identical content and base as remote, skipping push",
                        canonicalName
                    );
                }

                return false;
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Push {BranchName}", canonicalName);
            }

            _worktreeRepository.Network.Push(
                remote: _worktreeRepository.Network.Remotes[GitConstants.DefaultRemote],
                pushRefSpec: $"+{canonicalName}:{canonicalName}",
                pushOptions: new PushOptions
                {
                    CredentialsProvider = (_, _, _) => gitCredentials.ToGitCredentials(),
                }
            );
        }


        return true;
    }

    /// <summary>
    /// Checks if local and remote branches have identical content and share the same merge base.
    /// Used to skip redundant force pushes after rebase (rebase changes commit IDs via committer timestamp).
    /// </summary>
    private bool AreBranchesEquivalent(Branch localBranch, Branch remoteBranch, Branch sourceBranch)
    {
        var localTip = localBranch.Tip;
        var remoteTip = remoteBranch.Tip;

        if (localTip.Id.Equals(remoteTip.Id))
        {
            return true;
        }

        var localMergeBase = _worktreeRepository.ObjectDatabase.FindMergeBase(localTip, sourceBranch.Tip);
        var remoteMergeBase = _worktreeRepository.ObjectDatabase.FindMergeBase(remoteTip, sourceBranch.Tip);

        if (localMergeBase is null || remoteMergeBase is null)
        {
            return false;
        }

        if (!localMergeBase.Id.Equals(remoteMergeBase.Id))
        {
            return false;
        }

        using var changes = _worktreeRepository.Diff.Compare<TreeChanges>(localTip.Tree, remoteTip.Tree);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Branch diff: {ChangeCount} file(s) changed between local {LocalCommit} and remote {RemoteCommit}",
                changes.Count,
                localTip.Id.Sha[..7],
                remoteTip.Id.Sha[..7]
            );
        }

        return changes.Count == 0;
    }
}
