# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Aviationexam.DependencyUpdater is a .NET command-line tool for automating dependency updates in projects using Azure DevOps Git repositories and NuGet artifact feeds. It reads Dependabot-style configuration files and processes updates accordingly.

## Build Commands

### Build the solution
```bash
dotnet restore --nologo
dotnet build --no-restore --nologo --configuration Release
```

### Run tests
```bash
# Run all tests
dotnet test --no-build --configuration Release

# Run tests with TRX report output
dotnet test --no-build --configuration Release --results-directory TestResults --report-trx
```

### Code formatting
```bash
# Check formatting (CI mode)
dotnet format --no-restore --verify-no-changes -v diag

# Apply formatting
dotnet format
```

### Run the tool locally
The main project `Aviationexam.DependencyUpdater` is the CLI entry point. Run with:
```bash
dotnet run --project src/Aviationexam.DependencyUpdater/Aviationexam.DependencyUpdater.csproj -- \
  --directory "/path/to/repo" \
  --git-password "<token>" \
  --organization "<org>" \
  --project "<project-id>" \
  --repository "<repo-id>" \
  --pat "<pat>" \
  --account-id "<account-id>" \
  --nuget-project "<nuget-project-id>" \
  --nuget-feed-id "<feed-id>" \
  --nuget-service-host "<service-host>" \
  --access-token-resource-id "499b84ac-1321-427f-aa17-267ca6975798"
```

## Solution Structure

The solution uses a modular architecture with clear separation of concerns:

- **Aviationexam.DependencyUpdater** - Main CLI application entry point with command-line argument parsing (System.CommandLine)
- **Aviationexam.DependencyUpdater.ConfigurationParser** - Parses Dependabot v2 YAML configuration files and converts them to internal models
- **Aviationexam.DependencyUpdater.Common** - Shared domain models and utilities (Package, IgnoreResolver, GroupResolver, etc.)
- **Aviationexam.DependencyUpdater.Nuget** - NuGet-specific package resolution and version handling
- **Aviationexam.DependencyUpdater.Vcs.Git** - Git operations using LibGit2Sharp
- **Aviationexam.DependencyUpdater.Repository.DevOps** - Azure DevOps integration (pull requests, artifact feeds, upstream ingestion)
- **Aviationexam.DependencyUpdater.Interfaces** - Core interfaces and contracts
- **Aviationexam.DependencyUpdater.DefaultImplementations** - Default implementations for file system and environment variable access
- **Aviationexam.DependencyUpdater.Constants** - Shared constants
- **Aviationexam.DependencyUpdater.TestsInfrastructure** - Shared test utilities

## Project Configuration

- **Solution file**: `Aviationexam.DependencyUpdater.slnx` (XML-based solution format)
- **Central Package Management**: Uses `Directory.Packages.props` with `ManagePackageVersionsCentrally` enabled
- **Target frameworks**: Multi-targeting .NET 9.0 and .NET 10.0
- **Test framework**: xUnit v3 with Microsoft.Testing.Platform runner (configured in global.json)

## Key Architecture Patterns

### Dependency Injection
The application uses Microsoft.Extensions.DependencyInjection throughout. Service registration is organized via `ServiceCollectionExtensions` classes in each project.

### Configuration Binding
Command-line arguments are bound to configuration objects using custom binders (e.g., `SourceConfigurationBinder`, `DevOpsConfigurationBinder`) in Program.cs.

### VCS Abstraction
Git operations are abstracted through `ISourceVersioningFactory` and `ISourceVersioningWorkspace` interfaces, with implementations in the Vcs.Git project.

### Package Management
- `IPackageManager` interface abstracts package operations
- NuGet implementation supports reading package references from project files and resolving versions
- Caching layer to minimize redundant API calls

### Ignore and Group Resolution
- `IgnoreResolver` - Determines which package updates to skip based on Dependabot ignore rules (supports wildcards)
- `GroupResolver` - Groups related package updates together according to Dependabot group configuration

## Configuration File Format

The tool reads standard Dependabot v2 configuration, typically at `.github/dependabot.yml`.

The Dependabot schema is automatically downloaded from https://www.schemastore.org/dependabot-2.0.json during build and used by Corvus.Json.SourceGenerator to generate strongly-typed models. The schema file is downloaded to `src/Aviationexam.DependencyUpdater.ConfigurationParser/dependabot-2.0.json` if it doesn't exist (see the `DownloadDependabotSchema` target in the ConfigurationParser.csproj).

Key configuration features:
- Package ecosystem specification (e.g., "nuget")
- Update schedules and grouping rules
- Ignore patterns for dependencies or version ranges
- Directory-based configurations

## Azure DevOps Integration

The tool has undocumented Azure DevOps API integrations in `Repository.DevOps`:
- `AzureDevOpsUndocumentedClient` - Handles upstream package ingestion for Azure Artifacts feeds
- Supports authentication via PAT or optional AZ CLI sidecar service
- Can fetch and ingest package versions from upstream sources (e.g., nuget.org) into private feeds

## Development Notes

- **Warnings as errors**: Enabled in Release configuration
- **Blocked packages**: Build fails if `Devlooped.SponsorLink` is detected as a dependency
- **NU1507 warning**: Suppressed globally (related to central package management)
- **Analyzers**: Uses Meziantou.Analyzer for code quality

## Common Development Workflows

### Running a specific test
```bash
dotnet test src/Aviationexam.DependencyUpdater.Nuget.Tests/Aviationexam.DependencyUpdater.Nuget.Tests.csproj --filter "FullyQualifiedName~TestName"
```

### Working with Git operations
Git operations are in `Vcs.Git` project using LibGit2Sharp. The `GitSourceVersioningFactory` creates workspace instances that manage branches, commits, and pull requests.

### Adding new package sources
Extend `IPackageManager` interface and create implementation similar to the Nuget project structure. Register in DI container via `ServiceCollectionExtensions`.
