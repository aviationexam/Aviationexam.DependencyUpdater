# GitHub Action Usage Guide

This document explains how to use Aviationexam.DependencyUpdater as a GitHub Action to automate dependency updates in your repository.

## Quick Start

### 1. Create Configuration File

Create a configuration file at `.github/updater.yml` (preferred) or `.github/dependabot.yml` (fallback) in your repository. The configuration is based on the [Dependabot v2 schema](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file) with additional custom fields.

**Configuration file discovery order:**
1. `.github/updater.yml` (preferred)
2. `.azuredevops/updater.yml`
3. `.github/dependabot.yml` (fallback)
4. `.azuredevops/dependabot.yml` (fallback)

**Example configuration:**

```yaml
version: 2

# Define all registries at the root level
# Use token field with environment variables for authentication
registries:
  my-private-feed:
    type: nuget-feed
    url: https://pkgs.dev.azure.com/org/_packaging/feed/nuget/v3/index.json
    token: PAT:${{ DEVOPS_TOKEN }}  # Resolved from environment variable
    nuget-feed-version: V3
  
  nuget-org:
    type: nuget-feed
    url: https://api.nuget.org/v3/index.json
    nuget-feed-version: V3

updates:
  - package-ecosystem: "nuget"
    directory: "/"

    # Optional: Fallback framework when not auto-detected
    targetFramework: "net9.0"

    # Optional: Custom commit author (defaults to GitHub Actions bot)
    commit-author: "DependencyBot"
    commit-author-email: "bot@example.com"

    # Optional: Reviewers to assign to PRs
    reviewers:
      - "tech-lead"
      - "team-reviewer"

    # Optional: Execute dotnet restore (default: true)
    execute-restore: true

    # Optional: Custom restore directory
    restore-directory: "./src"

    # Optional: Use private registries and public fallbacks (both defined at root)
    registries:
      - my-private-feed
    fallback-registries:
      - nuget-org

    # Optional: Update submodules
    update-submodules:
      - path: "submodules/shared-lib"
        branch: "main"

    # Group related packages into a single PR
    groups:
      # Group specific packages by pattern
      microsoft-extensions:
        patterns:
          - "Microsoft.Extensions.*"
          - "System.*"

    # Ignore specific dependencies or version ranges
    ignore:
      # Ignore major version updates for a specific package
      - dependency-name: "Newtonsoft.Json"
        update-types: ["version-update:semver-major"]
```

### 2. Create Workflow File

Create a workflow file at `.github/workflows/dependency-updates.yml`:

```yaml
name: Dependency Updates

on:
  schedule:
    # Run every Monday at 8am UTC
    - cron: '0 8 * * 1'

  # Allow manual triggering from Actions tab
  workflow_dispatch:

permissions:
  contents: write      # Create branches and commits
  pull-requests: write # Create pull requests
  issues: write        # Create and apply labels to pull requests
```

### 3. Commit and Push

Commit both files to your repository and push to GitHub. The action will run according to your schedule or when manually triggered.

## Configuration

### Action Inputs

All inputs are optional and have sensible defaults:

| Input            | Description                                                         | Default                               | Required |
|------------------|---------------------------------------------------------------------|---------------------------------------|----------|
| `github-token`   | GitHub token for authentication                                     | `${{ github.token }}`                 | No       |
| `directory`      | Repository directory to process                                     | `${{ github.workspace }}`             | No       |
| `reset-cache`    | Clear dependency cache before processing                            | `false`                               | No       |
| `owner`          | GitHub repository owner                                             | `${{ github.repository_owner }}`      | No       |
| `repository`     | GitHub repository name                                              | `${{ github.event.repository.name }}` | No       |
| `dotnet-version` | .NET SDK version to use (use `skip` to skip .NET setup)             | `10.0.x`                              | No       |
| `tool-version`   | Tool version to install (`latest` or specific version like `0.4.0`) | `0.4.0`                               | No       |

**Notes:**
- Configuration files are automatically discovered. No need to specify the path.
- The `tool-version` defaults to the current release version matching the action tag. Use `latest` to always install the newest stable release.

### Advanced Examples

#### Clear Cache on Each Run

```yaml
- name: Update dependencies
  uses: aviationexam/Aviationexam.DependencyUpdater@v1
  with:
    github-token: ${{ secrets.GITHUB_TOKEN }}
    reset-cache: 'true'
```

#### Use Specific .NET SDK Version

```yaml
- name: Update dependencies
  uses: aviationexam/Aviationexam.DependencyUpdater@v1
  with:
    github-token: ${{ secrets.GITHUB_TOKEN }}
    dotnet-version: '10.0.x'  # Use .NET 10
```

#### Skip .NET Setup (Already Installed)

If .NET SDK is already installed by a previous step or available on the runner:

```yaml
- name: Setup .NET (custom configuration)
  uses: actions/setup-dotnet@v5
  with:
    dotnet-version: |
      9.0.x
      10.0.x

- name: Update dependencies
  uses: aviationexam/Aviationexam.DependencyUpdater@v1
  with:
    github-token: ${{ secrets.GITHUB_TOKEN }}
    dotnet-version: 'skip'  # Skip .NET setup, already configured
```

#### Use Specific Tool Version

```yaml
- name: Update dependencies
  uses: aviationexam/Aviationexam.DependencyUpdater@v1
  with:
    github-token: ${{ secrets.GITHUB_TOKEN }}
    tool-version: '0.3.0'  # Use specific version
```

#### Always Use Latest Tool Version

```yaml
- name: Update dependencies
  uses: aviationexam/Aviationexam.DependencyUpdater@v1
  with:
    github-token: ${{ secrets.GITHUB_TOKEN }}
    tool-version: 'latest'  # Always install latest stable release
```

#### Run on Different Schedules

```yaml
on:
  schedule:
    # Daily at midnight UTC
    - cron: '0 0 * * *'

    # Every 6 hours
    - cron: '0 */6 * * *'

    # First day of every month at 9am UTC
    - cron: '0 9 1 * *'
```

#### Process Different Directories

If you have multiple configuration entries in your `.github/updater.yml` with different directories, they will all be processed in a single run:

```yaml
# .github/updater.yml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/src/MainProject"
    groups:
      all-updates:
        patterns: ["*"]

  - package-ecosystem: "nuget"
    directory: "/tools"
    groups:
      tool-updates:
        patterns: ["*"]
```

## How It Works

The GitHub Action is implemented as a **composite action** that:

1. Sets up the .NET SDK using `actions/setup-dotnet@v5`
2. Installs the `Aviationexam.DependencyUpdater` global tool
3. Runs the tool with the GitHub platform configuration

This approach provides:
- **Fast startup** - No Docker image build required
- **Transparency** - All steps visible in action logs
- **Flexibility** - Easy to customize .NET SDK version
- **Efficiency** - Leverages GitHub Actions caching for .NET

## Permissions

The GitHub Action requires the following permissions to function properly:

- **contents: write** - Create branches and commits for dependency updates
- **pull-requests: write** - Create pull requests with the updates
- **issues: write** - Create and apply labels to pull requests

These permissions are specified in the workflow file:

```yaml
permissions:
  contents: write
  pull-requests: write
  issues: write
```

## Configuration File Format

The configuration file is based on the Dependabot v2 YAML schema with additional custom fields. **Note:** The `schedule` section is not used - the action runs based on your GitHub Actions workflow schedule.

### Basic Structure

```yaml
version: 2
updates:
  - package-ecosystem: "nuget"  # Currently only "nuget" is supported
    directory: "/"               # Directory containing packages (relative to repository root)
```

### Custom Fields

This tool extends the standard Dependabot schema with additional fields:

#### `targetFramework` (optional)
Specifies which .NET framework to use when there's no other hint about the framework version in the project files. This serves as a fallback when the tool cannot automatically determine the target framework:
```yaml
targetFramework: "net9.0"
```

#### `commit-author` and `commit-author-email` (optional)
Customize the commit author for dependency update commits:
```yaml
commit-author: "DependencyBot"
commit-author-email: "bot@example.com"
```
Default: Uses GitHub Actions bot identity

#### `reviewers` (optional)
Assign reviewers to created pull requests:
```yaml
reviewers:
  - "tech-lead"
  - "team-member"
```

#### `execute-restore` (optional)
Control whether `dotnet restore` is executed before analyzing dependencies:
```yaml
execute-restore: true  # default: true
```

#### `restore-directory` (optional)
Specify a custom directory for restore operations:
```yaml
restore-directory: "./src"
```

#### `registries` and `fallback-registries` (in updates section)

Both fields reference registry names defined at the root level:

**`registries`**: List of primary registry names (typically private/authenticated feeds)
**`fallback-registries`**: List of fallback registry names (typically public feeds like nuget.org)

**Usage patterns:**

1. **Private registries with public fallback** (most common):
```yaml
registries:
  - my-private-feed  # Primary: private feed with auth
fallback-registries:
  - nuget-org        # Fallback: public feed
```

2. **Only public registries**:
```yaml
registries:
  - nuget-org
```

3. **Multiple private registries with fallback**:
```yaml
registries:
  - private-feed-1
  - private-feed-2
fallback-registries:
  - nuget-org
```

**Important:** 
- All registry names must be defined at the root level of the configuration
- Credentials are specified directly in `.github/updater.yml` using the `token` field
- Use GitHub secrets for sensitive credentials (e.g., `token: PAT:${{ DEVOPS_TOKEN }}`)

#### `update-submodules` (optional)
Update Git submodules as part of the dependency update process:
```yaml
update-submodules:
  - path: "submodules/shared-library"
    branch: "main"
  - path: "submodules/common-tools"
    branch: "develop"
```

### Standard Dependabot Fields

#### Root-Level `registries`
Define all registries (both private and public) at the root level of the configuration file:

```yaml
version: 2

registries:
  my-private-feed:
    type: nuget-feed
    url: https://pkgs.dev.azure.com/org/_packaging/feed/nuget/v3/index.json
    token: PAT:${{ DEVOPS_TOKEN }}  # Use GitHub secrets for credentials
    nuget-feed-version: V3  # Custom field: V2 or V3
  
  nuget-org:
    type: nuget-feed
    url: https://api.nuget.org/v3/index.json
    nuget-feed-version: V3

updates:
  - package-ecosystem: "nuget"
    directory: "/"
    registries:
      - my-private-feed
    fallback-registries:
      - nuget-org
```

**Authentication:**
- Credentials are specified in the `token` field using environment variable references
- Token format: `PAT:${{ ENV_VAR_NAME }}`
- Environment variables are resolved from the system environment at runtime

**Example configuration with environment variable:**
```yaml
# .github/updater.yml
registries:
  my-private-feed:
    type: nuget-feed
    url: https://pkgs.dev.azure.com/org/_packaging/feed/nuget/v3/index.json
    token: PAT:${{ DEVOPS_TOKEN }}
    nuget-feed-version: V3
```

**Example workflow setting environment variables:**

Environment variables must be set at the **job level** (not step level) for composite actions:

```yaml
jobs:
  update-dependencies:
    runs-on: ubuntu-latest
    env:
      DEVOPS_TOKEN: ${{ secrets.AZURE_DEVOPS_PAT }}  # Set at job level
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Update dependencies
        uses: aviationexam/Aviationexam.DependencyUpdater@v1
        with:
          github-token: ${{ secrets.GITHUB_TOKEN }}
```

**Note:** The `nuget-feed-version` field (V2 or V3) is a custom extension to specify the NuGet API version.

### Grouping Updates

Group related packages into a single pull request using patterns:

```yaml
groups:
  # Group by pattern
  microsoft-packages:
    patterns:
      - "Microsoft.*"
      - "System.*"

  # Group all dependencies together
  all-dependencies:
    patterns:
      - "*"
```

**Note:** The `update-types` field in groups is not supported. Use `patterns` to group packages.

### Ignoring Dependencies

Skip updates for specific packages or version ranges:

```yaml
ignore:
  # Ignore all updates for a package
  - dependency-name: "Legacy.Package"

  # Ignore major version updates
  - dependency-name: "Breaking.Changes.Package"
    update-types: ["version-update:semver-major"]

  # Ignore specific version ranges
  - dependency-name: "Problematic.Package"
    versions: ["1.x", "2.0.0"]
```

### Update Limits

Control how many pull requests are created:

```yaml
open-pull-requests-limit: 5  # Maximum number of open PRs (default: 5)
```

## Troubleshooting

### Action Fails with "Permission Denied"

Ensure your workflow has the required permissions:

```yaml
permissions:
  contents: write
  pull-requests: write
```

### No Pull Requests Created

1. Check that your configuration file exists and is valid YAML
2. Verify that there are actually updates available
3. Check the Action logs for any error messages
4. Ensure dependencies aren't ignored in your configuration

### Build Cache Issues

If you suspect cache issues, use the `reset-cache` input:

```yaml
- uses: aviationexam/Aviationexam.DependencyUpdater@v1
  with:
    github-token: ${{ secrets.GITHUB_TOKEN }}
    reset-cache: 'true'
```

### Manual Triggering

You can manually trigger the workflow from the GitHub Actions tab if you include `workflow_dispatch`:

```yaml
on:
  schedule:
    - cron: '0 8 * * 1'
  workflow_dispatch:  # Enables manual triggering
```

## Comparison with Dependabot

| Feature         | Aviationexam.DependencyUpdater  | GitHub Dependabot  |
|-----------------|---------------------------------|--------------------|
| NuGet Support   | ✅ Yes                           | ✅ Yes              |
| Custom Grouping | ✅ Yes                           | ✅ Yes              |
| Ignore Rules    | ✅ Yes                           | ✅ Yes              |
| Private Feeds   | ✅ Yes (via nuget.config)        | ⚠️ Limited         |
| Submodules      | ✅ Yes                           | ❌ No               |
| Self-Hosted     | ✅ Yes (GitHub Actions)          | ❌ No               |
| Configuration   | Dependabot v2 YAML              | Dependabot v2 YAML |

## Resources

- [Dependabot Configuration Reference](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file)
- [GitHub Actions Workflow Syntax](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
- [Cron Schedule Expressions](https://crontab.guru/)

## Support

If you encounter issues or have questions:

1. Check the [GitHub Action logs](https://docs.github.com/en/actions/monitoring-and-troubleshooting-workflows/using-workflow-run-logs) for detailed error messages
2. Review this documentation and the [main README](../README.md)
3. [Open an issue](https://github.com/aviationexam/Aviationexam.DependencyUpdater/issues/new) on GitHub
