[![Build Status](https://github.com/aviationexam/Aviationexam.DependencyUpdater/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/aviationexam/Aviationexam.DependencyUpdater/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Aviationexam.DependencyUpdater.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Aviationexam.DependencyUpdater/)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Faviationexam%2Fdependency-updater%2Fshield%2FAviationexam.DependencyUpdater%2Flatest&label=Aviationexam.DependencyUpdater)](https://f.feedz.io/aviationexam/dependency-updater/packages/Aviationexam.DependencyUpdater/latest/download)

# Aviationexam.DependencyUpdater

A command-line tool for automating dependency updates in projects using Azure DevOps Git repositories and NuGet artifact feeds.

## Features

- Updates dependencies in a local Git repository based on configuration files
- Supports Azure DevOps Git and NuGet feeds
- Handles authentication for private repositories and feeds

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

Run the tool from your terminal:

```sh
dotnet dependency-updater \
  --directory "/path/to/repo" \
  --git-username "<git-username>" \
  --git-password "<git-password-or-token>" \
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

## Arguments

| Argument                   | Required | Default | Description                                                                                |
|----------------------------|:--------:|:-------:|--------------------------------------------------------------------------------------------|
| --directory                |    Y     |         | Path to the local Git repository                                                           |
| --git-username             |    N     |         | Username for remote Git authentication                                                     |
| --git-password             |    Y     |         | Password or personal access token for remote Git authentication                            |
| --organization             |    Y     |         | Azure DevOps organization name                                                             |
| --project                  |    Y     |         | Azure DevOps project containing the target repository                                      |
| --repository               |    Y     |         | Name of the Azure DevOps Git repository                                                    |
| --pat                      |    Y     |         | Azure DevOps personal access token                                                         |
| --account-id               |    Y     |         | Azure DevOps user or service account ID                                                    |
| --nuget-project            |    Y     |         | Azure DevOps project containing the NuGet artifacts feed                                   |
| --nuget-feed-id            |    Y     |         | ID of the Azure Artifacts NuGet feed                                                       |
| --nuget-service-host       |    Y     |         | Internal Azure DevOps service host identifier                                              |
| --access-token-resource-id |    Y     |         | Azure AD resource ID for upstream ingestion (always: 499b84ac-1321-427f-aa17-267ca6975798) |
| --az-side-car-address      |    N     |   ''    | URL for AZ sidecar service (optional)                                                      |
| --az-side-car-token        |    N     |         | Token for AZ sidecar service (optional)                                                    |
| --reset-cache              |    N     |  false  | Clears the internal dependency cache before processing updates                             |
