using Aviationexam.DependencyUpdater.Interfaces;
using LibGit2Sharp;
using System;

namespace Aviationexam.DependencyUpdater.Vcs.Git;

public sealed class GitSourceVersioningFactory(
    TimeProvider timeProvider
) : ISourceVersioningFactory
{
    public ISourceVersioning CreateSourceVersioning(
        string sourceDirectory
    ) => new GitSourceVersioning(
#pragma warning disable CA2000
        new Repository(sourceDirectory),
#pragma warning restore CA2000
        timeProvider
    );
}
