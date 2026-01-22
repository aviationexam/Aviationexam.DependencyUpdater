import type {
  CreatePullRequestInput,
  GitHubInstallation,
  GitHubInstallationToken,
  GitHubPullRequest,
  GitHubRepository,
  Result,
} from "../types.ts";

const GITHUB_API_BASE = "https://api.github.com";
const USER_AGENT = "Aviationexam-DependencyUpdater-Proxy/1.0";

export interface GitHubApiError {
  status: number;
  message: string;
}

function createHeaders(token: string): Record<string, string> {
  return {
    Authorization: `Bearer ${token}`,
    Accept: "application/vnd.github+json",
    "User-Agent": USER_AGENT,
    "X-GitHub-Api-Version": "2022-11-28",
  };
}

interface GitHubRepositoryWithPermissions extends GitHubRepository {
  permissions?: {
    admin: boolean;
    maintain?: boolean;
    push: boolean;
    triage?: boolean;
    pull: boolean;
  };
}

export async function validateCallerHasRepoAccess(
  callerToken: string,
  owner: string,
  repository: string
): Promise<Result<GitHubRepository, GitHubApiError>> {
  const response = await fetch(
    `${GITHUB_API_BASE}/repos/${owner}/${repository}`,
    { headers: createHeaders(callerToken) }
  );

  if (!response.ok) {
    return {
      success: false,
      error: {
        status: response.status,
        message: `Token does not have access to ${owner}/${repository}`,
      },
    };
  }

  const data = (await response.json()) as GitHubRepositoryWithPermissions;

  if (!data.permissions?.push) {
    return {
      success: false,
      error: {
        status: 403,
        message: `Token does not have write access to ${owner}/${repository}`,
      },
    };
  }

  return { success: true, data };
}

export async function getInstallationForRepo(
  appJwt: string,
  owner: string,
  repository: string
): Promise<Result<GitHubInstallation, GitHubApiError>> {
  const response = await fetch(
    `${GITHUB_API_BASE}/repos/${owner}/${repository}/installation`,
    { headers: createHeaders(appJwt) }
  );

  if (!response.ok) {
    const errorBody = await response.text();
    return {
      success: false,
      error: {
        status: response.status,
        message:
          response.status === 404
            ? `GitHub App is not installed on ${owner}/${repository}`
            : `Failed to get installation: ${errorBody}`,
      },
    };
  }

  const data = (await response.json()) as GitHubInstallation;
  return { success: true, data };
}

export async function createInstallationToken(
  appJwt: string,
  installationId: number
): Promise<Result<GitHubInstallationToken, GitHubApiError>> {
  const response = await fetch(
    `${GITHUB_API_BASE}/app/installations/${installationId}/access_tokens`,
    {
      method: "POST",
      headers: createHeaders(appJwt),
    }
  );

  if (!response.ok) {
    const errorBody = await response.text();
    return {
      success: false,
      error: {
        status: response.status,
        message: `Failed to create installation token: ${errorBody}`,
      },
    };
  }

  const data = (await response.json()) as GitHubInstallationToken;
  return { success: true, data };
}

export async function createPullRequest(
  installationToken: string,
  input: CreatePullRequestInput
): Promise<Result<GitHubPullRequest, GitHubApiError>> {
  const response = await fetch(
    `${GITHUB_API_BASE}/repos/${input.owner}/${input.repository}/pulls`,
    {
      method: "POST",
      headers: {
        ...createHeaders(installationToken),
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        title: input.title,
        body: input.body,
        head: input.head,
        base: input.base,
        draft: input.draft ?? false,
      }),
    }
  );

  if (!response.ok) {
    const errorBody = await response.text();
    return {
      success: false,
      error: {
        status: response.status,
        message: `Failed to create pull request: ${errorBody}`,
      },
    };
  }

  const data = (await response.json()) as GitHubPullRequest;
  return { success: true, data };
}
