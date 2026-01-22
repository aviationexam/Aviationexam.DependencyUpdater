import type {
  ErrorResponse,
  GitHubAppConfig,
  GitHubPullRequest,
  GitHubPullRequestBody,
  Result,
} from "../types.ts";
import {
  createInstallationToken,
  createPullRequest,
  getInstallationForRepo,
  type GitHubApiError,
  validateCallerHasRepoAccess,
} from "./github-client.ts";
import { generateJwt } from "./jwt.ts";

export type ProxyServiceError = ErrorResponse & { statusCode: number };

export async function createPullRequestViaProxy(
  appConfig: GitHubAppConfig,
  callerToken: string,
  owner: string,
  repository: string,
  body: GitHubPullRequestBody
): Promise<Result<GitHubPullRequest, ProxyServiceError>> {
  const accessResult = await validateCallerHasRepoAccess(
    callerToken,
    owner,
    repository
  );

  if (!accessResult.success) {
    logProxyError("validateCallerHasRepoAccess", owner, repository, accessResult.error);
    return {
      success: false,
      error: mapToServiceError(accessResult.error, "unauthorized"),
    };
  }

  const appJwt = await generateJwt(appConfig);

  const installationResult = await getInstallationForRepo(
    appJwt,
    owner,
    repository
  );

  if (!installationResult.success) {
    logProxyError("getInstallationForRepo", owner, repository, installationResult.error);
    return {
      success: false,
      error: mapToServiceError(
        installationResult.error,
        installationResult.error.status === 404 ? "not_found" : "internal_error"
      ),
    };
  }

  const tokenResult = await createInstallationToken(
    appJwt,
    installationResult.data.id
  );

  if (!tokenResult.success) {
    logProxyError("createInstallationToken", owner, repository, tokenResult.error);
    return {
      success: false,
      error: mapToServiceError(tokenResult.error, "internal_error"),
    };
  }

  const prResult = await createPullRequest(
    tokenResult.data.token,
    owner,
    repository,
    body
  );

  if (!prResult.success) {
    logProxyError("createPullRequest", owner, repository, prResult.error, {
      base: body.base,
      head: body.head,
      titleLength: body.title?.length ?? 0,
    });
    return {
      success: false,
      error: mapToServiceError(prResult.error, "internal_error"),
    };
  }

  return {
    success: true,
    data: prResult.data,
  };
}

function mapToServiceError(
  apiError: GitHubApiError,
  errorType: ErrorResponse["error"]
): ProxyServiceError {
  return {
    statusCode: apiError.status,
    error: errorType,
    message: apiError.message,
  };
}

function logProxyError(
  step: string,
  owner: string,
  repository: string,
  error: GitHubApiError,
  context?: Record<string, unknown>
): void {
  const payload: Record<string, unknown> = {
    step,
    owner,
    repository,
    status: error.status,
    message: error.message,
  };

  if (context) {
    payload.context = context;
  }

  if (error.status >= 500) {
    console.error("Proxy GitHub API step failed", payload);
  } else {
    console.warn("Proxy GitHub API step failed", payload);
  }
}
