import type { ErrorResponse, GitHubPullRequestBody } from "./types.ts";
import { createPullRequestViaProxy } from "./core/proxy-service.ts";
import { decodeBase64Key } from "./core/utils.ts";

interface Env {
  GITHUB_APP_ID: string;
  GITHUB_APP_KEY?: string;
  GITHUB_APP_KEY_BASE64?: string;
}

const REPOS_PULLS_REGEX = /^\/repos\/([^/]+)\/([^/]+)\/pulls$/;

export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    if (request.method === "OPTIONS") {
      return handleCors();
    }

    if (request.method !== "POST") {
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
  if (authHeader?.startsWith("Bearer ")) {
    return authHeader.slice(7);
  }
  if (authHeader?.startsWith("token ")) {
    return authHeader.slice(6);
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
