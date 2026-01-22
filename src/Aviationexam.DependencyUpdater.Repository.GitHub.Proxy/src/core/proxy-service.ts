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
