using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace Aviationexam.DependencyUpdater.Vcs.Git.Tests;

public class GitSourceVersioningWorkspaceSubmoduleTests
{
    private static readonly GitCredentialsConfiguration NoCredentials = new();

    [Fact]
    public void UpdateSubmodule_ReturnsCommitsBetweenSourceBranchAndUpstream()
    {
        // Arrange
        using var root = new TemporaryDirectoryProvider(NullLogger.Instance);
        var fixture = SubmoduleFixture.Create(root.TemporaryDirectory);

        var initialSha = fixture.SubmoduleSha;
        var secondSha = fixture.CommitToSubmodule("Second change");
        var thirdSha = fixture.CommitToSubmodule("Third change\n\nWith a body");

        using var sourceVersioning = new GitSourceVersioningFactory(TimeProvider.System, NullLoggerFactory.Instance)
            .CreateSourceVersioning(fixture.SuperprojectClone);

        using var workspace = sourceVersioning.CreateWorkspace(
            NoCredentials,
            Path.Join(root.TemporaryDirectory, "worktree"),
            sourceBranchName: "main",
            branchName: "update/submodule",
            worktreeName: "update-submodule"
        );

        // Act
        var result = workspace.UpdateSubmodule("sub", "main", "main");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("sub", result.Path);
        Assert.Equal(fixture.SubmoduleRemote, result.RemoteUrl);
        Assert.Equal(initialSha, result.FromCommit);
        Assert.Equal(thirdSha, result.ToCommit);
        Assert.Equal([thirdSha, secondSha], result.Commits.Select(x => x.Sha));
        Assert.Equal("Third change\n\nWith a body\n", result.Commits[0].Message);
        Assert.True(workspace.HasUncommitedChanges());
    }

    [Fact]
    public void UpdateSubmodule_WithoutNewCommits_ReturnsEmptyCommitList()
    {
        // Arrange
        using var root = new TemporaryDirectoryProvider(NullLogger.Instance);
        var fixture = SubmoduleFixture.Create(root.TemporaryDirectory);

        using var sourceVersioning = new GitSourceVersioningFactory(TimeProvider.System, NullLoggerFactory.Instance)
            .CreateSourceVersioning(fixture.SuperprojectClone);

        using var workspace = sourceVersioning.CreateWorkspace(
            NoCredentials,
            Path.Join(root.TemporaryDirectory, "worktree"),
            sourceBranchName: "main",
            branchName: "update/submodule",
            worktreeName: "update-submodule"
        );

        // Act
        var result = workspace.UpdateSubmodule("sub", "main", "main");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fixture.SubmoduleSha, result.FromCommit);
        Assert.Equal(fixture.SubmoduleSha, result.ToCommit);
        Assert.Empty(result.Commits);
        Assert.False(workspace.HasUncommitedChanges());
    }

    [Fact]
    public void UpdateSubmodule_UnknownBranch_ReturnsNull()
    {
        // Arrange
        using var root = new TemporaryDirectoryProvider(NullLogger.Instance);
        var fixture = SubmoduleFixture.Create(root.TemporaryDirectory);

        using var sourceVersioning = new GitSourceVersioningFactory(TimeProvider.System, NullLoggerFactory.Instance)
            .CreateSourceVersioning(fixture.SuperprojectClone);

        using var workspace = sourceVersioning.CreateWorkspace(
            NoCredentials,
            Path.Join(root.TemporaryDirectory, "worktree"),
            sourceBranchName: "main",
            branchName: "update/submodule",
            worktreeName: "update-submodule"
        );

        // Act
        var result = workspace.UpdateSubmodule("sub", "does-not-exist", "main");

        // Assert
        Assert.Null(result);
    }

    private sealed class SubmoduleFixture
    {
        public required string SubmoduleRemote { get; init; }
        public required string SubmoduleWorkingCopy { get; init; }
        public required string SuperprojectClone { get; init; }
        public required string SubmoduleSha { get; init; }

        public static SubmoduleFixture Create(string root)
        {
            var submoduleRemote = Path.Join(root, "sub-remote.git");
            var submoduleWorkingCopy = Path.Join(root, "sub-work");
            var superprojectRemote = Path.Join(root, "super-remote.git");
            var superprojectWorkingCopy = Path.Join(root, "super-work");
            var superprojectClone = Path.Join(root, "super-clone");

            Git(root, "init", "--bare", "-b", "main", submoduleRemote);
            Git(root, "clone", "-q", submoduleRemote, submoduleWorkingCopy);
            File.WriteAllText(Path.Join(submoduleWorkingCopy, "README.md"), "initial");
            Git(submoduleWorkingCopy, "add", ".");
            Git(submoduleWorkingCopy, "commit", "-q", "-m", "Initial commit");
            Git(submoduleWorkingCopy, "push", "-q", "origin", "HEAD:main");
            var submoduleSha = Git(submoduleWorkingCopy, "rev-parse", "HEAD").Trim();

            Git(root, "init", "--bare", "-b", "main", superprojectRemote);
            Git(root, "clone", "-q", superprojectRemote, superprojectWorkingCopy);
            File.WriteAllText(Path.Join(superprojectWorkingCopy, "README.md"), "superproject");
            Git(superprojectWorkingCopy, "add", ".");
            Git(superprojectWorkingCopy, "commit", "-q", "-m", "Initial commit");
            Git(superprojectWorkingCopy, "-c", "protocol.file.allow=always", "submodule", "add", "-q", "-b", "main", submoduleRemote, "sub");
            Git(superprojectWorkingCopy, "commit", "-q", "-m", "Add submodule");
            Git(superprojectWorkingCopy, "push", "-q", "origin", "HEAD:main");

            Git(root, "-c", "protocol.file.allow=always", "clone", "-q", "--recurse-submodules", superprojectRemote, superprojectClone);

            return new SubmoduleFixture
            {
                SubmoduleRemote = submoduleRemote,
                SubmoduleWorkingCopy = submoduleWorkingCopy,
                SuperprojectClone = superprojectClone,
                SubmoduleSha = submoduleSha,
            };
        }

        public string CommitToSubmodule(string message)
        {
            File.AppendAllText(Path.Join(SubmoduleWorkingCopy, "README.md"), Environment.NewLine + message);
            Git(SubmoduleWorkingCopy, "add", ".");
            Git(SubmoduleWorkingCopy, "commit", "-q", "-m", message);
            Git(SubmoduleWorkingCopy, "push", "-q", "origin", "HEAD:main");

            return Git(SubmoduleWorkingCopy, "rev-parse", "HEAD").Trim();
        }

        private static string Git(string workingDirectory, params string[] arguments)
        {
            var startInfo = new ProcessStartInfo("git")
            {
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            foreach (var argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            startInfo.Environment["GIT_AUTHOR_NAME"] = "Test";
            startInfo.Environment["GIT_AUTHOR_EMAIL"] = "test@example.com";
            startInfo.Environment["GIT_COMMITTER_NAME"] = "Test";
            startInfo.Environment["GIT_COMMITTER_EMAIL"] = "test@example.com";

            using var process = Process.Start(startInfo)!;
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"git {string.Join(' ', arguments)} failed ({process.ExitCode}): {error}");
            }

            return output;
        }
    }
}
