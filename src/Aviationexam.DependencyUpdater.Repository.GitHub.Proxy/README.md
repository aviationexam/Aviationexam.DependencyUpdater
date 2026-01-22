# GitHub App Proxy for Aviationexam.DependencyUpdater

A transparent proxy service that creates Pull Requests using a GitHub App identity, allowing CI workflows to trigger on PRs created by the Dependency Updater.

## Why This Exists

When using `GITHUB_TOKEN` from GitHub Actions, PRs created by the token don't trigger CI workflows (this is GitHub's design to prevent infinite loops). This proxy solves that by:

1. Receiving PR creation requests authenticated with the caller's `GITHUB_TOKEN`
2. Validating the caller has write access to the target repository
3. Creating the PR using a GitHub App identity (which triggers CI)

## Transparent Proxy

The proxy mimics GitHub's API endpoint exactly:

```
POST /repos/{owner}/{repo}/pulls
```

This means you can use it as a drop-in replacement by simply changing the base URL in your GitHub client (e.g., Octokit). The request/response format is identical to GitHub's API.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│  User's GitHub Actions                                          │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  Aviationexam.DependencyUpdater (Octokit)                 │  │
│  │  POST /repos/{owner}/{repo}/pulls                         │  │
│  │  Authorization: Bearer <GITHUB_TOKEN>                     │  │
│  └─────────────────────────┬─────────────────────────────────┘  │
└────────────────────────────┼────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  Proxy (Cloudflare Worker)                                      │
│  1. Validate caller's token has write access (via blob create)  │
│  2. Generate JWT for GitHub App                                 │
│  3. Get installation token for the repo                         │
│  4. Create PR using installation token                          │
│  5. Return full GitHub PR response (never expose tokens)        │
└─────────────────────────────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  GitHub                                                         │
│  PR created by App identity → CI triggers ✅                    │
└─────────────────────────────────────────────────────────────────┘
```

## Usage

### HTTP Mode (Cloudflare Worker) - Production

The proxy accepts the same request format as GitHub's API:

```bash
POST https://your-worker.workers.dev/repos/aviationexam/Aviationexam.DependencyUpdater/pulls
Authorization: Bearer <GITHUB_TOKEN>
Content-Type: application/json

{
  "title": "Update dependencies",
  "body": "This PR updates...",
  "head": "feature/update-deps",
  "base": "main",
  "draft": false
}
```

Response: Full GitHub Pull Request object (same as GitHub API)

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
wrangler secret put GITHUB_APP_KEY_BASE64

# Deploy
bun run deploy
```

## Security

- The proxy never returns or exposes installation tokens
- All requests are validated by attempting a write operation (blob creation)
- The caller can only create PRs in repositories their token has write access to
- GitHub App credentials are stored as Cloudflare Worker secrets

## API Reference

### POST /repos/{owner}/{repo}/pulls

Creates a pull request using the GitHub App identity.

**Headers:**
- `Authorization: Bearer <GITHUB_TOKEN>` or `Authorization: token <GITHUB_TOKEN>` - Required

**Body:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| title | string | Yes | PR title |
| body | string | No | PR description |
| head | string | Yes | Branch with changes |
| base | string | Yes | Target branch |
| draft | boolean | No | Create as draft PR |
| maintainer_can_modify | boolean | No | Allow maintainer edits |

**Responses:**

- `201 Created` - PR created successfully (returns full GitHub PR object)
- `400 Bad Request` - Invalid request body
- `401 Unauthorized` - Missing/invalid token or no repo access
- `403 Forbidden` - Token does not have write access to repository
- `404 Not Found` - GitHub App not installed on repository
- `500 Internal Server Error` - Server configuration error
