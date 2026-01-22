import type {
  GitHubInstallation,
  GitHubInstallationToken,
  GitHubPullRequest,
  GitHubPullRequestBody,
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

export async function validateCallerHasRepoAccess(
  callerToken: string,
  owner: string,
  repository: string
): Promise<Result<GitHubRepository, GitHubApiError>> {
  // First, verify the token can access the repository at all
  const repoResponse = await fetch(
    `${GITHUB_API_BASE}/repos/${owner}/${repository}`,
    { headers: createHeaders(callerToken) }
  );

  if (!repoResponse.ok) {
    const errorBody = await repoResponse.text();
    logGitHubApiError({
      operation: "validateCallerHasRepoAccess:repo",
      owner,
      repository,
      status: repoResponse.status,
      url: `${GITHUB_API_BASE}/repos/${owner}/${repository}`,
      responseBody: errorBody,
      requestId: repoResponse.headers.get("x-github-request-id"),
      rateLimitRemaining: repoResponse.headers.get("x-ratelimit-remaining"),
    });
    return {
      success: false,
      error: {
        status: repoResponse.status,
        message: `Token does not have access to ${owner}/${repository}`,
      },
    };
  }

  const repoData = (await repoResponse.json()) as GitHubRepository;

  // For GITHUB_TOKEN (installation tokens), the permissions object returns all false
  // even when the token has actual write access. The only reliable way to check
  // write permission is to attempt a write operation.
  // Creating an empty blob is safe (doesn't affect repo) and requires contents:write.
  const blobResponse = await fetch(
    `${GITHUB_API_BASE}/repos/${owner}/${repository}/git/blobs`,
    {
      method: "POST",
      headers: {
        ...createHeaders(callerToken),
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ content: "", encoding: "utf-8" }),
    }
  );

  if (!blobResponse.ok) {
    const errorBody = await blobResponse.text();
    logGitHubApiError({
      operation: "validateCallerHasRepoAccess:blob",
      owner,
      repository,
      status: blobResponse.status,
      url: `${GITHUB_API_BASE}/repos/${owner}/${repository}/git/blobs`,
      responseBody: errorBody,
      requestId: blobResponse.headers.get("x-github-request-id"),
      rateLimitRemaining: blobResponse.headers.get("x-ratelimit-remaining"),
      requestBody: { content: "", encoding: "utf-8" },
    });
    return {
      success: false,
      error: {
        status: 403,
        message: `Token does not have write access to ${owner}/${repository}`,
      },
    };
  }

  return { success: true, data: repoData };
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
    logGitHubApiError({
      operation: "getInstallationForRepo",
      owner,
      repository,
      status: response.status,
      url: `${GITHUB_API_BASE}/repos/${owner}/${repository}/installation`,
      responseBody: errorBody,
      requestId: response.headers.get("x-github-request-id"),
      rateLimitRemaining: response.headers.get("x-ratelimit-remaining"),
    });
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
    logGitHubApiError({
      operation: "createInstallationToken",
      owner: "",
      repository: "",
      status: response.status,
      url: `${GITHUB_API_BASE}/app/installations/${installationId}/access_tokens`,
      responseBody: errorBody,
      requestId: response.headers.get("x-github-request-id"),
      rateLimitRemaining: response.headers.get("x-ratelimit-remaining"),
      requestBody: { installationId },
    });
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
  owner: string,
  repository: string,
  body: GitHubPullRequestBody
): Promise<Result<GitHubPullRequest, GitHubApiError>> {
  const response = await fetch(
    `${GITHUB_API_BASE}/repos/${owner}/${repository}/pulls`,
    {
      method: "POST",
      headers: {
        ...createHeaders(installationToken),
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
    }
  );

  if (!response.ok) {
    const errorBody = await response.text();
    logGitHubApiError({
      operation: "createPullRequest",
      owner,
      repository,
      status: response.status,
      url: `${GITHUB_API_BASE}/repos/${owner}/${repository}/pulls`,
      responseBody: errorBody,
      requestId: response.headers.get("x-github-request-id"),
      rateLimitRemaining: response.headers.get("x-ratelimit-remaining"),
      requestBody: summarizePullRequestBody(body),
    });
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

type GitHubApiErrorLogContext = {
  operation: string;
  owner: string;
  repository: string;
  status: number;
  url: string;
  responseBody: string;
  requestId: string | null;
  rateLimitRemaining: string | null;
  requestBody?: Record<string, unknown>;
};

function logGitHubApiError(context: GitHubApiErrorLogContext): void {
  const payload: Record<string, unknown> = {
    operation: context.operation,
    owner: context.owner || undefined,
    repository: context.repository || undefined,
    status: context.status,
    url: context.url,
    requestId: context.requestId,
    rateLimitRemaining: context.rateLimitRemaining,
    responseBody: context.responseBody,
  };

  if (context.requestBody) {
    payload.requestBody = context.requestBody;
  }

  if (context.status >= 500) {
    console.error("GitHub API request failed", payload);
  } else {
    console.warn("GitHub API request failed", payload);
  }
}

function summarizePullRequestBody(
  body: GitHubPullRequestBody
): Record<string, unknown> {
  return {
    titleLength: body.title?.length ?? 0,
    hasBody: Boolean(body.body),
    bodyLength: body.body?.length ?? 0,
    head: body.head,
    base: body.base,
    draft: body.draft ?? false,
    maintainerCanModify: body.maintainer_can_modify ?? false,
  };
}
