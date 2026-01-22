import type {
  ErrorResponse,
  GitHubAppConfig,
  Result,
  TokenRequest,
  TokenResponse,
} from "../types.ts";
import {
  createInstallationToken,
  getInstallationForRepo,
  type GitHubApiError,
  validateCallerHasRepoAccess,
} from "./github-client.ts";
import { generateJwt } from "./jwt.ts";

export type TokenServiceError = ErrorResponse & { statusCode: number };

export async function getInstallationAccessToken(
  appConfig: GitHubAppConfig,
  callerToken: string,
  request: TokenRequest
): Promise<Result<TokenResponse, TokenServiceError>> {
  const accessResult = await validateCallerHasRepoAccess(
    callerToken,
    request.owner,
    request.repository
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
    request.owner,
    request.repository
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

  return {
    success: true,
    data: {
      token: tokenResult.data.token,
      expiresAt: tokenResult.data.expires_at,
    },
  };
}

function mapToServiceError(
  apiError: GitHubApiError,
  errorType: ErrorResponse["error"]
): TokenServiceError {
  return {
    statusCode: apiError.status,
    error: errorType,
    message: apiError.message,
  };
}
