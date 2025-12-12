# GitHub Action Usage Guide

This document explains how to use Aviationexam.DependencyUpdater as a GitHub Action to automate dependency updates in your repository.

## Quick Start

### 1. Create Configuration File

Create a configuration file at `.github/updater.yml` or `.github/dependabot.yml` in your repository. This file follows the [Dependabot v2 configuration schema](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file).

**Example configuration:**

```yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
    groups:
      # Group all patch updates together
      patch-updates:
        update-types:
          - "patch"
      # Group specific packages
      microsoft-extensions:
        patterns:
          - "Microsoft.Extensions.*"
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
| `config-file` | Path to updater configuration file | Auto-detected (`.github/updater.yml` or `.github/dependabot.yml`) | No |
| `directory` | Repository directory to process | `${{ github.workspace }}` | No |
| `reset-cache` | Clear dependency cache before processing | `false` | No |
| `owner` | GitHub repository owner | `${{ github.repository_owner }}` | No |
| `repository` | GitHub repository name | `${{ github.event.repository.name }}` | No |

### Advanced Examples

#### Custom Configuration File Location

```yaml
- name: Update dependencies
  uses: aviationexam/Aviationexam.DependencyUpdater@v1
  with:
    github-token: ${{ secrets.GITHUB_TOKEN }}
    config-file: .config/dependency-updates.yml
```

#### Clear Cache on Each Run

```yaml
- name: Update dependencies
  uses: aviationexam/Aviationexam.DependencyUpdater@v1
  with:
    github-token: ${{ secrets.GITHUB_TOKEN }}
    reset-cache: 'true'
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

#### Multiple Jobs for Different Directories

```yaml
jobs:
  update-main-project:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: aviationexam/Aviationexam.DependencyUpdater@v1
        with:
          github-token: ${{ secrets.GITHUB_TOKEN }}
          config-file: .github/updater-main.yml
  
  update-tools:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: aviationexam/Aviationexam.DependencyUpdater@v1
        with:
          github-token: ${{ secrets.GITHUB_TOKEN }}
          config-file: .github/updater-tools.yml
```

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

The configuration file follows the Dependabot v2 YAML schema. Here are the most commonly used options:

### Basic Structure

```yaml
version: 2
updates:
  - package-ecosystem: "nuget"  # Currently only "nuget" is supported
    directory: "/"               # Directory containing packages
    schedule:
      interval: "daily"          # daily, weekly, or monthly
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
