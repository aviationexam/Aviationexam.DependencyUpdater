import type { ErrorResponse, GitHubPullRequestBody } from "./types.ts";
import { createPullRequestViaProxy } from "./core/proxy-service.ts";
import { decodeBase64Key } from "./core/utils.ts";

interface Env {
  GITHUB_APP_ID: string;
  GITHUB_APP_KEY?: string;
  GITHUB_APP_KEY_BASE64?: string;
}

const REPOS_PULLS_REGEX = /^(?:\/api\/v3)?\/repos\/([^/]+)\/([^/]+)\/pulls$/;

export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    if (request.method === "OPTIONS") {
      return handleCors();
    }

    if (request.method !== "POST") {
      logRequestWarning("invalid_method", request);
      return jsonResponse(
        { error: "invalid_request", message: "Method not allowed" },
        405
      );
    }

    const url = new URL(request.url);
    const match = url.pathname.match(REPOS_PULLS_REGEX);

    if (match && match[1] && match[2]) {
      return handleCreatePullRequest(request, env, match[1], match[2]);
    }

    logRequestWarning("not_found", request);
    return jsonResponse({ error: "not_found", message: "Not found" }, 404);
  },
};

async function handleCreatePullRequest(
  request: Request,
  env: Env,
  owner: string,
  repository: string
): Promise<Response> {
  const authHeader = request.headers.get("Authorization");
  const callerToken = extractToken(authHeader);
  if (!callerToken) {
    logAuthHeaderMinimal(request, authHeader);
    return jsonResponse(
      {
        error: "unauthorized",
        message: "Missing or invalid Authorization header",
      },
      401
    );
  }

  let body: GitHubPullRequestBody;
  try {
    body = (await request.json()) as GitHubPullRequestBody;
  } catch {
    return jsonResponse(
      { error: "invalid_request", message: "Invalid JSON body" },
      400
    );
  }

  const validationError = validateGitHubPullRequestBody(body);
  if (validationError) {
    return jsonResponse(
      { error: "invalid_request", message: validationError },
      400
    );
  }

  const appKey = env.GITHUB_APP_KEY ?? (env.GITHUB_APP_KEY_BASE64 ? decodeBase64Key(env.GITHUB_APP_KEY_BASE64) : undefined);

  if (!env.GITHUB_APP_ID || !appKey) {
    return jsonResponse(
      { error: "internal_error", message: "Server configuration error" },
      500
    );
  }

  const result = await createPullRequestViaProxy(
    { appId: env.GITHUB_APP_ID, privateKey: appKey },
    callerToken,
    owner,
    repository,
    body
  );

  if (!result.success) {
    return jsonResponse(
      { error: result.error.error, message: result.error.message },
      result.error.statusCode
    );
  }

  return jsonResponse(result.data, 201);
}

function validateGitHubPullRequestBody(
  input: GitHubPullRequestBody
): string | null {
  if (!input.title) return "title is required";
  if (!input.head) return "head is required";
  if (!input.base) return "base is required";
  return null;
}

function extractToken(authHeader: string | null): string | null {
  const trimmed = authHeader?.trim();
  if (!trimmed) return null;

  const firstSpaceIndex = trimmed.indexOf(" ");
  if (firstSpaceIndex <= 0) return null;

  const scheme = trimmed.slice(0, firstSpaceIndex).toLowerCase();
  const token = trimmed.slice(firstSpaceIndex + 1).trim();
  if (!token) return null;

  if (scheme === "bearer" || scheme === "token") {
    return token;
  }
  return null;
}

function jsonResponse<T>(data: T | ErrorResponse, status: number): Response {
  return new Response(JSON.stringify(data), {
    status,
    headers: {
      "Content-Type": "application/json",
      "Access-Control-Allow-Origin": "*",
      "Access-Control-Allow-Methods": "POST, OPTIONS",
      "Access-Control-Allow-Headers": "Authorization, Content-Type",
    },
  });
}

function logRequestWarning(reason: string, request: Request): void {
  const url = new URL(request.url);
  console.warn("Proxy request rejected", {
    reason,
    method: request.method,
    path: url.pathname,
  });
}

function logAuthHeaderMinimal(
  request: Request,
  authHeader: string | null
): void {
  const hasAuthorizationHeader =
    request.headers.has("authorization") ||
    request.headers.has("Authorization");
  const trimmed = authHeader?.trim() ?? "";
  const spaceIndex = trimmed.indexOf(" ");
  const scheme = spaceIndex > 0 ? trimmed.slice(0, spaceIndex) : trimmed;

  console.warn("Authorization header invalid", {
    hasAuthorizationHeader,
    scheme: scheme ? scheme.toLowerCase() : null,
    length: authHeader?.length ?? 0,
  });
}

function handleCors(): Response {
  return new Response(null, {
    status: 204,
    headers: {
      "Access-Control-Allow-Origin": "*",
      "Access-Control-Allow-Methods": "POST, OPTIONS",
      "Access-Control-Allow-Headers": "Authorization, Content-Type",
    },
  });
}
