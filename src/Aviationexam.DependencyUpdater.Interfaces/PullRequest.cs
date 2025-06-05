namespace Aviationexam.DependencyUpdater.Interfaces;

public record PullRequest(
    string PullRequestId,
    string BranchName,
    string BranchTipCommitId,
    bool IsEmptyBranch
);
