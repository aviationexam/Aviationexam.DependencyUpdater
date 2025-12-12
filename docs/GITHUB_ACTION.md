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
    
    # Optional: Define registries and their URLs
    registries:
      - nuget-feed
    fallback-registries:
      nuget-feed: "https://api.nuget.org/v3/index.json"
    
    # Optional: Update submodules
    update-submodules:
      - path: "submodules/shared-lib"
        branch: "main"
    
    # Group related packages into a single PR
    groups:
      # Group all patch updates together
      patch-updates:
        update-types:
          - "patch"
      # Group specific packages
      microsoft-extensions:
        patterns:
          - "Microsoft.Extensions.*"
    
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

jobs:
  update-dependencies:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
      
      - name: Update dependencies
        uses: aviationexam/Aviationexam.DependencyUpdater@v1
        with:
          github-token: ${{ secrets.GITHUB_TOKEN }}
```

### 3. Commit and Push

Commit both files to your repository and push to GitHub. The action will run according to your schedule or when manually triggered.

## Configuration

### Action Inputs

All inputs are optional and have sensible defaults:

| Input | Description | Default | Required |
|-------|-------------|---------|----------|
| `github-token` | GitHub token for authentication | `${{ github.token }}` | No |
| `directory` | Repository directory to process | `${{ github.workspace }}` | No |
| `reset-cache` | Clear dependency cache before processing | `false` | No |
| `owner` | GitHub repository owner | `${{ github.repository_owner }}` | No |
| `repository` | GitHub repository name | `${{ github.event.repository.name }}` | No |
| `dotnet-version` | .NET SDK version to use | `9.0.x` | No |

**Note:** Configuration files are automatically discovered. No need to specify the path.

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

These permissions are specified in the workflow file:

```yaml
permissions:
  contents: write
  pull-requests: write
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

#### `registries` and `fallback-registries`
Define registries to use for package resolution. The `registries` field lists registry names, and `fallback-registries` maps those names to URLs:

```yaml
registries:
  - nuget-feed
  - custom-feed

fallback-registries:
  nuget-feed: "https://api.nuget.org/v3/index.json"
  custom-feed: "https://pkgs.dev.azure.com/org/_packaging/feed/nuget/v3/index.json"
```

The tool will use registries in the order listed, with `fallback-registries` providing the actual feed URLs.

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

#### `registries` (two formats)

**Format 1: Simple list with fallback-registries (recommended for custom feeds)**
```yaml
updates:
  - package-ecosystem: "nuget"
    directory: "/"
    registries:
      - nuget-org
      - custom-feed
    fallback-registries:
      nuget-org: "https://api.nuget.org/v3/index.json"
      custom-feed: "https://pkgs.dev.azure.com/org/_packaging/feed/nuget/v3/index.json"
```

**Format 2: Standard Dependabot registries definition (for authenticated feeds)**
```yaml
registries:
  my-private-feed:
    type: nuget-feed
    url: https://pkgs.dev.azure.com/org/_packaging/feed/nuget/v3/index.json
    username: username
    password: ${{ secrets.NUGET_TOKEN }}
    nuget-feed-version: v3  # Custom field: v2 or v3

updates:
  - package-ecosystem: "nuget"
    directory: "/"
    registries:
      - my-private-feed
```

### Grouping Updates

Group related packages into a single pull request:

```yaml
groups:
  # Group by pattern
  microsoft-packages:
    patterns:
      - "Microsoft.*"
      - "System.*"
  
  # Group by update type
  minor-and-patch:
    update-types:
      - "minor"
      - "patch"
```

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

| Feature | Aviationexam.DependencyUpdater | GitHub Dependabot |
|---------|-------------------------------|-------------------|
| NuGet Support | ✅ Yes | ✅ Yes |
| Custom Grouping | ✅ Yes | ✅ Yes |
| Ignore Rules | ✅ Yes | ✅ Yes |
| Private Feeds | ✅ Yes (via nuget.config) | ⚠️ Limited |
| Submodules | ✅ Yes | ❌ No |
| Self-Hosted | ✅ Yes (GitHub Actions) | ❌ No |
| Configuration | Dependabot v2 YAML | Dependabot v2 YAML |

## Resources

- [Dependabot Configuration Reference](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file)
- [GitHub Actions Workflow Syntax](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
- [Cron Schedule Expressions](https://crontab.guru/)

## Support

If you encounter issues or have questions:

1. Check the [GitHub Action logs](https://docs.github.com/en/actions/monitoring-and-troubleshooting-workflows/using-workflow-run-logs) for detailed error messages
2. Review this documentation and the [main README](../README.md)
3. [Open an issue](https://github.com/aviationexam/Aviationexam.DependencyUpdater/issues/new) on GitHub
