[![Build Status](https://github.com/aviationexam/Aviationexam.DependencyUpdater/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/aviationexam/Aviationexam.DependencyUpdater/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Aviationexam.DependencyUpdater.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Aviationexam.DependencyUpdater/)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Faviationexam%2Fdependency-updater%2Fshield%2FAviationexam.DependencyUpdater%2Flatest&label=Aviationexam.DependencyUpdater)](https://f.feedz.io/aviationexam/dependency-updater/packages/Aviationexam.DependencyUpdater/latest/download)

# Aviationexam.DependencyUpdater

A command-line tool for automating dependency updates in projects using Git repositories and NuGet feeds. Supports multiple repository platforms including Azure DevOps and GitHub.

**For GitHub users:** Use the [GitHub Action](#github-action-recommended) for the easiest setup experience.

## Features

- Updates dependencies in a local Git repository based on Dependabot configuration files
- Supports multiple repository platforms (Azure DevOps, GitHub)
- Supports Azure DevOps Git and NuGet artifact feeds with upstream ingestion
- Handles authentication for private repositories and feeds
- Platform-agnostic package management for NuGet
- Available as a GitHub Action for seamless CI/CD integration

## Installation

### GitHub Action (Recommended)

The easiest way to use this tool in GitHub repositories is via the GitHub Action:

1. Create a configuration file at `.github/updater.yml` or `.github/dependabot.yml`
2. Create a workflow file at `.github/workflows/dependency-updates.yml`:

```yaml
name: Dependency Updates

on:
  schedule:
    - cron: '0 8 * * 1'  # Every Monday at 8am UTC
  workflow_dispatch:

permissions:
  contents: write
  pull-requests: write

jobs:
  update-dependencies:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: aviationexam/Aviationexam.DependencyUpdater@v1
        with:
          github-token: ${{ secrets.GITHUB_TOKEN }}
```

See the [GitHub Action documentation](docs/GITHUB_ACTION.md) for detailed configuration options and examples.

### .NET Global Tool

This project is also distributed as a .NET tool. To install it globally, run:

```sh
dotnet tool install --global Aviationexam.DependencyUpdater
```

Or to update to the latest version:

```sh
dotnet tool update --global Aviationexam.DependencyUpdater
```

#### Nightly builds

You can install the latest nightly build from the Feedz.io feed:

```sh
dotnet tool install --global Aviationexam.DependencyUpdater --prerelease --add-source https://f.feedz.io/aviationexam/dependency-updater/nuget/index.json
```

## Usage

### Azure DevOps

Run the tool from your terminal using the `AzureDevOps` subcommand:

```sh
dotnet dependency-updater  \
  --directory "/path/to/repo" \
  --git-username "<git-username>" \
  --git-password "<git-password-or-token>" \
  AzureDevOps \
  --organization "<azure-devops-org>" \
  --project <project-id> \
  --repository <repository-id> \
  --pat "<azure-devops-pat>" \
  --account-id <account-id> \
  --nuget-project <nuget-project-id> \
  --nuget-feed-id <nuget-feed-id> \
  --nuget-service-host '<service-host>' \
  --access-token-resource-id 499b84ac-1321-427f-aa17-267ca6975798 \
  --az-side-car-address '<az-side-car-url>' \
  --az-side-car-token '<az-side-car-token>' \
  --reset-cache
```

### GitHub

Run the tool from your terminal using the `GitHub` subcommand:

```sh
dotnet dependency-updater \
  --directory "/path/to/repo" \
  --git-username "<git-username>" \
  --git-password "<git-password-or-token>" \
  GitHub \
  --owner "<owner>" \
  --repository "<repo>" \
  --token "<github-token>"
```

**Required GitHub PAT Scopes:**
- `repo` - Full control of private repositories

## Arguments

### Common Arguments

Available for all platform subcommands:

| Argument       | Required | Default | Description                                                     |
|----------------|:--------:|:-------:|-----------------------------------------------------------------|
| --directory    |    Y     |   cwd   | Path to the local Git repository                                |
| --git-username |    N     |   ''    | Username for remote Git authentication                          |
| --git-password |    Y     |         | Password or personal access token for remote Git authentication |
| --reset-cache  |    N     |  false  | Clears the internal dependency cache before processing updates  |

### Azure DevOps Arguments

Required when using the `AzureDevOps` subcommand:

| Argument                     | Required | Description                                                                                |
|------------------------------|:--------:|--------------------------------------------------------------------------------------------|
| --organization               |    Y     | Azure DevOps organization name                                                             |
| --project                    |    Y     | Azure DevOps project containing the target repository                                      |
| --repository                 |    Y     | Name of the Azure DevOps Git repository                                                    |
| --pat                        |    Y     | Azure DevOps personal access token                                                         |
| --account-id                 |    Y     | Azure DevOps user or service account ID                                                    |
| --nuget-project              |    Y     | Azure DevOps project containing the NuGet artifacts feed                                   |
| --nuget-feed-id              |    Y     | ID of the Azure Artifacts NuGet feed                                                       |
| --nuget-service-host         |    Y     | Internal Azure DevOps service host identifier                                              |
| --access-token-resource-id   |    Y     | Azure AD resource ID for upstream ingestion (always: 499b84ac-1321-427f-aa17-267ca6975798) |
| --az-side-car-address        |    N     | URL for AZ sidecar service (optional)                                                      |
| --az-side-car-token          |    N     | Token for AZ sidecar service (optional)                                                    |

### GitHub Arguments

Required when using the `GitHub` subcommand:

| Argument     | Required | Description                                                 |
|--------------|:--------:|-------------------------------------------------------------|
| --owner      |    Y     | GitHub repository owner (organization or user account name) |
| --repository |    Y     | GitHub repository name                                      |
| --token      |    Y     | GitHub personal access token (requires `repo` scope)        |
