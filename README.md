[![Build Status](https://github.com/aviationexam/Aviationexam.DependencyUpdater/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/aviationexam/Aviationexam.DependencyUpdater/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Aviationexam.DependencyUpdater.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Aviationexam.DependencyUpdater/)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Faviationexam%2Fdependency-updater%2Fshield%2FAviationexam.DependencyUpdater%2Flatest&label=Aviationexam.DependencyUpdater)](https://f.feedz.io/aviationexam/dependency-updater/packages/Aviationexam.DependencyUpdater/latest/download)

# Aviationexam.DependencyUpdater

A command-line tool for automating dependency updates in projects using Git repositories and NuGet feeds. Supports multiple repository platforms including Azure DevOps and GitHub.

## Features

- Updates dependencies in a local Git repository based on Dependabot configuration files
- Supports multiple repository platforms (Azure DevOps, GitHub)
- Supports Azure DevOps Git and NuGet artifact feeds with upstream ingestion
- Handles authentication for private repositories and feeds
- Platform-agnostic package management for NuGet

## Installation

This project is distributed as a .NET tool. To install it globally, run:

```sh
dotnet tool install --global Aviationexam.DependencyUpdater
```

Or to update to the latest version:

```sh
dotnet tool update --global Aviationexam.DependencyUpdater
```

### Nightly builds

You can install the latest nightly build from the Feedz.io feed:

```sh
dotnet tool install --global Aviationexam.DependencyUpdater --prerelease --add-source https://f.feedz.io/aviationexam/dependency-updater/nuget/index.json
```

## Usage

### Azure DevOps

Run the tool from your terminal:

```sh
dotnet dependency-updater \
  --directory "/path/to/repo" \
  --git-username "<git-username>" \
  --git-password "<git-password-or-token>" \
  --platform AzureDevOps \
  --azure-organization "<azure-devops-org>" \
  --azure-project <project-id> \
  --azure-repository <repository-id> \
  --azure-pat "<azure-devops-pat>" \
  --azure-account-id <account-id> \
  --azure-nuget-project <nuget-project-id> \
  --azure-nuget-feed-id <nuget-feed-id> \
  --azure-nuget-service-host '<service-host>' \
  --azure-access-token-resource-id 499b84ac-1321-427f-aa17-267ca6975798 \
  --azure-az-side-car-address '<az-side-car-url>' \
  --azure-az-side-car-token '<az-side-car-token>' \
  --reset-cache
```

### GitHub

GitHub support is planned but not yet implemented:

```sh
dotnet dependency-updater \
  --directory "/path/to/repo" \
  --git-username "<git-username>" \
  --git-password "<git-password-or-token>" \
  --platform GitHub \
  --github-owner "<owner>" \
  --github-repository "<repo>" \
  --github-token "<github-token>"
```

## Arguments

### Common Arguments

| Argument       | Required | Default | Description                                                     |
|----------------|:--------:|:-------:|-----------------------------------------------------------------|
| --directory    |    Y     |   cwd   | Path to the local Git repository                                |
| --git-username |    N     |   ''    | Username for remote Git authentication                          |
| --git-password |    Y     |         | Password or personal access token for remote Git authentication |
| --platform     |    Y     |         | Repository platform: `AzureDevOps` or `GitHub`                  |
| --reset-cache  |    N     |  false  | Clears the internal dependency cache before processing updates  |

### Azure DevOps Arguments

Required when `--platform AzureDevOps`:

| Argument                          | Required | Description                                                                                |
|-----------------------------------|:--------:|--------------------------------------------------------------------------------------------|
| --azure-organization              |    Y     | Azure DevOps organization name                                                             |
| --azure-project                   |    Y     | Azure DevOps project containing the target repository                                      |
| --azure-repository                |    Y     | Name of the Azure DevOps Git repository                                                    |
| --azure-pat                       |    Y     | Azure DevOps personal access token                                                         |
| --azure-account-id                |    Y     | Azure DevOps user or service account ID                                                    |
| --azure-nuget-project             |    Y     | Azure DevOps project containing the NuGet artifacts feed                                   |
| --azure-nuget-feed-id             |    Y     | ID of the Azure Artifacts NuGet feed                                                       |
| --azure-nuget-service-host        |    Y     | Internal Azure DevOps service host identifier                                              |
| --azure-access-token-resource-id  |    Y     | Azure AD resource ID for upstream ingestion (always: 499b84ac-1321-427f-aa17-267ca6975798) |
| --azure-az-side-car-address       |    N     | URL for AZ sidecar service (optional)                                                      |
| --azure-az-side-car-token         |    N     | Token for AZ sidecar service (optional)                                                    |

### GitHub Arguments

Required when `--platform GitHub` (not yet implemented):

| Argument           | Required | Description                             |
|--------------------|:--------:|-----------------------------------------|
| --github-owner     |    Y     | GitHub repository owner (user or org)   |
| --github-repository|    Y     | GitHub repository name                  |
| --github-token     |    Y     | GitHub personal access token            |
