using System;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Interfaces;

public interface ISourceVersioning : IDisposable
{
    void RunGitWorktreePrune(
        string repositoryPath
    );

    ISourceVersioningWorkspace CreateWorkspace(
        GitCredentialsConfiguration gitCredentials,
        string targetDirectory,
        string? sourceBranchName,
        string branchName,
        string worktreeName
    );

    IEnumerable<string> GetSubmodules();
}
