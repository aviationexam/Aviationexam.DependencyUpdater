import type {
  CreatePullRequestInput,
  CreatePullRequestResponse,
  ErrorResponse,
  GitHubAppConfig,
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
  input: CreatePullRequestInput
): Promise<Result<CreatePullRequestResponse, ProxyServiceError>> {
  const accessResult = await validateCallerHasRepoAccess(
    callerToken,
    input.owner,
    input.repository
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
    input.owner,
    input.repository
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

  const prResult = await createPullRequest(tokenResult.data.token, input);

  if (!prResult.success) {
    return {
      success: false,
      error: mapToServiceError(prResult.error, "internal_error"),
    };
  }

  return {
    success: true,
    data: {
      number: prResult.data.number,
      url: prResult.data.html_url,
    },
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
