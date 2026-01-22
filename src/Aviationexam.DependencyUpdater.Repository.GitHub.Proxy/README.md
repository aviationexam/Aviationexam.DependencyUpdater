# GitHub App Proxy for Aviationexam.DependencyUpdater

A proxy service that creates Pull Requests using a GitHub App identity, allowing CI workflows to trigger on PRs created by the Dependency Updater.

## Why This Exists

When using `GITHUB_TOKEN` from GitHub Actions, PRs created by the token don't trigger CI workflows (this is GitHub's design to prevent infinite loops). This proxy solves that by:

1. Receiving PR creation requests authenticated with the caller's `GITHUB_TOKEN`
2. Validating the caller has access to the target repository
3. Creating the PR using a GitHub App identity (which triggers CI)

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│  User's GitHub Actions                                          │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  Aviationexam.DependencyUpdater                           │  │
│  │  POST /api/pull-request                                   │  │
│  │  Authorization: Bearer <GITHUB_TOKEN>                     │  │
│  └─────────────────────────┬─────────────────────────────────┘  │
└────────────────────────────┼────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  Proxy (Cloudflare Worker)                                      │
│  1. Validate caller's token has repo access                     │
│  2. Generate JWT for GitHub App                                 │
│  3. Get installation token for the repo                         │
│  4. Create PR using installation token                          │
│  5. Return PR number and URL (never expose tokens)              │
└─────────────────────────────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  GitHub                                                         │
│  PR created by App identity → CI triggers ✅                    │
└─────────────────────────────────────────────────────────────────┘
```

## Modes

### HTTP Mode (Cloudflare Worker) - Production

Creates PRs via HTTP API. Never exposes tokens.

```bash
POST https://your-worker.workers.dev/api/pull-request
Authorization: Bearer <GITHUB_TOKEN>
Content-Type: application/json

{
  "owner": "aviationexam",
  "repository": "Aviationexam.DependencyUpdater",
  "title": "Update dependencies",
  "body": "This PR updates...",
  "head": "feature/update-deps",
  "base": "main",
  "draft": false
}
```

Response:
```json
{
  "number": 123,
  "url": "https://github.com/aviationexam/Aviationexam.DependencyUpdater/pull/123"
}
```

### CLI Mode - Local Testing

Returns installation access token for local development/testing.

```bash
export GITHUB_APP_ID=12345
export GITHUB_APP_KEY="$(cat private-key.pem)"
export GITHUB_TOKEN="ghp_xxx"

bun run cli --owner aviationexam --repository Aviationexam.DependencyUpdater
# Outputs: ghs_installation_token_xxx
```

## Setup

### Prerequisites

1. Create a GitHub App with these permissions:
   - Repository permissions:
     - Contents: Read & Write
     - Pull requests: Read & Write
     - Issues: Read & Write (for labels)
     - Metadata: Read

2. Install the app on repositories you want to use

3. Note the App ID and generate a private key

### Development

```bash
bun install
bun test
bun run typecheck
```

### Deployment to Cloudflare Workers

```bash
# Set secrets (do this once)
wrangler secret put GITHUB_APP_ID
wrangler secret put GITHUB_APP_KEY

# Deploy
bun run deploy
```

## Security

- The proxy never returns or exposes installation tokens
- All requests are validated against the caller's `GITHUB_TOKEN`
- The caller can only create PRs in repositories their token has access to
- GitHub App credentials are stored as Cloudflare Worker secrets

## API Reference

### POST /api/pull-request

Creates a pull request using the GitHub App identity.

**Headers:**
- `Authorization: Bearer <GITHUB_TOKEN>` - Required

**Body:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| owner | string | Yes | Repository owner |
| repository | string | Yes | Repository name |
| title | string | Yes | PR title |
| body | string | No | PR description |
| head | string | Yes | Branch with changes |
| base | string | Yes | Target branch |
| draft | boolean | No | Create as draft PR |

**Responses:**

- `201 Created` - PR created successfully
- `400 Bad Request` - Invalid request body
- `401 Unauthorized` - Missing/invalid token or no repo access
- `404 Not Found` - GitHub App not installed on repository
- `500 Internal Server Error` - Server configuration error
