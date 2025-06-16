using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Vcs.Git.Extensions;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Vcs.Git;

public sealed class GitSourceVersioning(
    Repository repository,
    TimeProvider timeProvider,
    ILogger<GitSourceVersioning> logger
) : ISourceVersioning
{
    public void RunGitWorktreePrune(
        string repositoryPath
    )
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "worktree prune",
            WorkingDirectory = repositoryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process();
        process.StartInfo = startInfo;

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                logger.LogTrace("[worktree prune] {Line}", args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                logger.LogError("[worktree prune] {Line}", args.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            logger.LogError("[worktree prune] failed with exit code {ExitCode}", process.ExitCode);
        }
    }

    public ISourceVersioningWorkspace CreateWorkspace(
        GitCredentialsConfiguration gitCredentials,
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
                    FetchOptions =
                    {
                        CredentialsProvider = (_, _, _) => gitCredentials.ToGitCredentials(),
                    },
                }
            );
        }

        return new GitSourceVersioningWorkspace(
            gitCredentials,
            repository,
            worktree,
            timeProvider,
            logger
        );
    }

    public IEnumerable<string> GetSubmodules() => repository.Submodules.Select(x => x.Name);

    public void Dispose() => repository.Dispose();
}
