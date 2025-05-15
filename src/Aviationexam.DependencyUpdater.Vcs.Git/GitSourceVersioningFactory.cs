using Aviationexam.DependencyUpdater.Interfaces;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;

namespace Aviationexam.DependencyUpdater.Vcs.Git;

public sealed class GitSourceVersioningFactory(
    TimeProvider timeProvider,
    ILoggerFactory loggerFactory
) : ISourceVersioningFactory
{
    public ISourceVersioning CreateSourceVersioning(
        string sourceDirectory
    ) => new GitSourceVersioning(
#pragma warning disable CA2000
        new Repository(sourceDirectory),
#pragma warning restore CA2000
        timeProvider,
        loggerFactory.CreateLogger<GitSourceVersioning>()
    );
}
