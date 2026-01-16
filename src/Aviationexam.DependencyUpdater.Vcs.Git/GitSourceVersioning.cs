using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Vcs.Git.Extensions;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Vcs.Git;

public sealed class GitSourceVersioning(
    Repository repository,
    TimeProvider timeProvider,
    ILogger<GitSourceVersioning> logger
) : ISourceVersioning
{
    private readonly Lock _gitRepositoryLock = new();

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
            if (!string.IsNullOrWhiteSpace(args.Data) && logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
            {
                logger.LogTrace("[worktree prune] {Line}", args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data) && logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Error))
            {
                logger.LogError("[worktree prune] {Line}", args.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0 && logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Error))
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
        lock (_gitRepositoryLock)
        {
            try
            {
                var existingWorktree = repository.Worktrees
                    .AsValueEnumerable()
                    .Where(x => x is not null)
                    .SingleOrDefault(x => x.Name == worktreeName);
                if (existingWorktree is not null)
                {
                    repository.Worktrees.Prune(existingWorktree, ifLocked: false);
                }

                var existingBranch = repository.Branches.AsValueEnumerable().SingleOrDefault(x => x.FriendlyName == branchName);
                if (existingBranch is not null)
                {
                    repository.Branches.Remove(existingBranch);
                }

                existingBranch = repository.Branches.AsValueEnumerable().SingleOrDefault(x => x.FriendlyName == worktreeName);
                if (existingBranch is not null)
                {
                    repository.Branches.Remove(existingBranch);
                }

                var worktree = repository.Worktrees.Add(name: worktreeName, path: targetDirectory, isLocked: false);
                var worktreeRepository = worktree.WorktreeRepository;
                worktreeRepository.Branches.Rename(worktreeName, branchName);

                if (
                    sourceBranchName is not null
                    && repository.Branches.AsValueEnumerable().Any(x => x.FriendlyName == sourceBranchName)
                )
                {
                    worktreeRepository.Reset(ResetMode.Hard, repository.Branches[sourceBranchName].Tip);
                }

                foreach (var submodule in worktreeRepository.Submodules)
                {
                    worktreeRepository.Submodules.Update(
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
                    _gitRepositoryLock,
                    gitCredentials,
                    repository,
                    worktree,
                    timeProvider,
                    logger
                );
            }
            catch (Exception e)
            {
                if (logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Error))
                {
                    logger.LogError(e, "Failed to create Git worktree '{WorktreeName}' in '{TargetDirectory}'", worktreeName, targetDirectory);
                }

                throw;
            }
        }
    }

    public IEnumerable<string> GetSubmodules() => repository.Submodules.AsValueEnumerable().Select(x => x.Name).ToList();

    public void Dispose() => repository.Dispose();
}
