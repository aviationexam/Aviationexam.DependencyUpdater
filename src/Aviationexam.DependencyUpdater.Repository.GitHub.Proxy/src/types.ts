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

export interface CreatePullRequestInput {
  owner: string;
  repository: string;
  title: string;
  body: string;
  head: string;
  base: string;
  draft?: boolean;
}

export interface GitHubPullRequest {
  id: number;
  number: number;
  html_url: string;
  state: string;
  title: string;
  head: {
    ref: string;
    sha: string;
  };
  base: {
    ref: string;
  };
}

export interface CreatePullRequestResponse {
  number: number;
  url: string;
}

export interface TokenRequest {
  owner: string;
  repository: string;
}

export interface TokenResponse {
  token: string;
  expiresAt: string;
}
