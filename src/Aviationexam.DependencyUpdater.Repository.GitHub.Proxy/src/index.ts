import type { CreatePullRequestInput, ErrorResponse } from "./types.ts";
import { createPullRequestViaProxy } from "./core/proxy-service.ts";
import { decodeBase64Key } from "./core/utils.ts";

interface Env {
  GITHUB_APP_ID: string;
  GITHUB_APP_KEY?: string;
  GITHUB_APP_KEY_BASE64?: string;
}

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

    if (url.pathname === "/api/pull-request") {
      return handleCreatePullRequest(request, env);
    }

    return jsonResponse({ error: "not_found", message: "Not found" }, 404);
  },
};

async function handleCreatePullRequest(
  request: Request,
  env: Env
): Promise<Response> {
  const authHeader = request.headers.get("Authorization");
  if (!authHeader?.startsWith("Bearer ")) {
    return jsonResponse(
      {
        error: "unauthorized",
        message: "Missing or invalid Authorization header",
      },
      401
    );
  }
  const callerToken = authHeader.slice(7);

  let body: CreatePullRequestInput;
  try {
    body = (await request.json()) as CreatePullRequestInput;
  } catch {
    return jsonResponse(
      { error: "invalid_request", message: "Invalid JSON body" },
      400
    );
  }

  const validationError = validateCreatePullRequestInput(body);
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

function validateCreatePullRequestInput(
  input: CreatePullRequestInput
): string | null {
  if (!input.owner) return "owner is required";
  if (!input.repository) return "repository is required";
  if (!input.title) return "title is required";
  if (!input.head) return "head is required";
  if (!input.base) return "base is required";
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
