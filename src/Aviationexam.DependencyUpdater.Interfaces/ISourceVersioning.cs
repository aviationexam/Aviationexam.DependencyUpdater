using System;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Interfaces;

public interface ISourceVersioning : IDisposable
{
    ISourceVersioningWorkspace CreateWorkspace(
        string targetDirectory,
        string? sourceBranchName,
        string branchName,
        string worktreeName
    );

    IEnumerable<string> GetSubmodules();
}
