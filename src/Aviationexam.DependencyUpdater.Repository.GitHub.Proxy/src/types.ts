export interface GitHubAppConfig {
  appId: string;
  privateKey: string;
}

export interface ErrorResponse {
  error: "unauthorized" | "not_found" | "invalid_request" | "internal_error";
  message: string;
}

export type Result<T, E = Error> =
  | { success: true; data: T }
  | { success: false; error: E };

export interface GitHubRepository {
  id: number;
  name: string;
  full_name: string;
  owner: {
    login: string;
  };
}

export interface GitHubInstallation {
  id: number;
  account: {
    login: string;
  };
}

export interface GitHubInstallationToken {
  token: string;
  expires_at: string;
}

export interface GitHubPullRequestBody {
  title: string;
  body?: string;
  head: string;
  base: string;
  draft?: boolean;
  maintainer_can_modify?: boolean;
}

export interface CreatePullRequestInput extends GitHubPullRequestBody {
  owner: string;
  repository: string;
}

export interface GitHubPullRequest {
  id: number;
  number: number;
  url: string;
  html_url: string;
  diff_url: string;
  patch_url: string;
  state: string;
  title: string;
  body: string | null;
  created_at: string;
  updated_at: string;
  merged_at: string | null;
  merge_commit_sha: string | null;
  head: {
    label: string;
    ref: string;
    sha: string;
    user: { login: string; id: number };
    repo: GitHubRepository;
  };
  base: {
    label: string;
    ref: string;
    sha: string;
    user: { login: string; id: number };
    repo: GitHubRepository;
  };
  user: { login: string; id: number };
  draft: boolean;
  merged: boolean;
  mergeable: boolean | null;
  mergeable_state: string;
  comments: number;
  commits: number;
  additions: number;
  deletions: number;
  changed_files: number;
}

export interface TokenRequest {
  owner: string;
  repository: string;
}

export interface TokenResponse {
  token: string;
  expiresAt: string;
}
