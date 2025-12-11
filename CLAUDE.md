# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Aviationexam.DependencyUpdater is a .NET command-line tool for automating dependency updates in projects using Git repositories and NuGet feeds. It supports multiple repository platforms (currently Azure DevOps, with GitHub planned) and reads Dependabot-style configuration files to process updates accordingly.

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

**Azure DevOps:**
```bash
dotnet run --project src/Aviationexam.DependencyUpdater/Aviationexam.DependencyUpdater.csproj -- \
  --directory "/path/to/repo" \
  --git-password "<token>" \
  --platform azure-devops \
  --azure-organization "<org>" \
  --azure-project "<project-id>" \
  --azure-repository "<repo-id>" \
  --azure-pat "<pat>" \
  --azure-account-id "<account-id>" \
  --azure-nuget-project "<nuget-project-id>" \
  --azure-nuget-feed-id "<feed-id>" \
  --azure-nuget-service-host "<service-host>" \
  --azure-access-token-resource-id "499b84ac-1321-427f-aa17-267ca6975798"
```

**GitHub:** *(not yet implemented)*
```bash
dotnet run --project src/Aviationexam.DependencyUpdater/Aviationexam.DependencyUpdater.csproj -- \
  --directory "/path/to/repo" \
  --git-password "<token>" \
  --platform github \
  --github-owner "<owner>" \
  --github-repository "<repo>" \
  --github-token "<token>"
```

## Solution Structure

The solution uses a modular architecture with clear separation of concerns:

- **Aviationexam.DependencyUpdater** - Main CLI application entry point with command-line argument parsing (System.CommandLine)
- **Aviationexam.DependencyUpdater.ConfigurationParser** - Parses Dependabot v2 YAML configuration files and converts them to internal models
- **Aviationexam.DependencyUpdater.Common** - Shared domain models and utilities (Package, IgnoreResolver, GroupResolver, etc.)
- **Aviationexam.DependencyUpdater.Nuget** - NuGet-specific package resolution and version handling
- **Aviationexam.DependencyUpdater.Vcs.Git** - Git operations using LibGit2Sharp (platform-agnostic)
- **Aviationexam.DependencyUpdater.Repository.DevOps** - Azure DevOps implementation (pull requests, artifact feeds, upstream ingestion)
- **Aviationexam.DependencyUpdater.Interfaces** - Core interfaces and contracts including:
  - Repository abstractions (`IRepositoryClient`, `IPackageFeedClient`, `IRepositoryPlatformConfiguration`)
  - Package management interfaces
  - VCS abstractions
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
Command-line arguments are bound to configuration objects using custom binders (e.g., `SourceConfigurationBinder`, `AzureDevOpsConfigurationBinder`) in Program.cs. Platform selection (`EPlatformSelection` enum) is parsed directly in Program.cs due to IBinder<T> constraints requiring reference types.

### VCS Abstraction
Git operations are abstracted through `ISourceVersioningFactory` and `ISourceVersioningWorkspace` interfaces, with implementations in the Vcs.Git project.

### Package Management
- `IPackageManager` interface abstracts package operations
- NuGet implementation supports reading package references from project files and resolving versions
- Caching layer to minimize redundant API calls

### Ignore and Group Resolution
- `IgnoreResolver` - Determines which package updates to skip based on Dependabot ignore rules (supports wildcards)
- `GroupResolver` - Groups related package updates together according to Dependabot group configuration

### Multi-Platform Repository Support
The tool supports multiple repository platforms through a clean abstraction layer:

**Platform Selection:**
- Required `--platform` CLI argument to select between `azure-devops` or `github`
- Platform-specific configuration arguments (prefixed with `--azure-*` or `--github-*`)
- Platform selection enum: `EPlatformSelection` (AzureDevOps, GitHub)

**Core Abstractions** (in `Aviationexam.DependencyUpdater.Interfaces.Repository`):
- `IRepositoryClient` - Platform-agnostic interface for pull request operations
  - List, get, create, update, and abandon pull requests
  - Implemented by `RepositoryAzureDevOpsClient` (GitHub implementation pending)
- `IPackageFeedClient` - Optional interface for platform-specific package feed operations
  - Azure Artifacts upstream ingestion for Azure DevOps (via `AzureArtifactsPackageFeedClient`)
  - Not used for GitHub (wrapped in `Optional<IPackageFeedClient>`)
- `IRepositoryPlatformConfiguration` - Base interface for platform-specific configurations
  - Implemented by `AzureDevOpsConfiguration` (GitHub configuration pending)

**Architecture Layers:**
1. **VCS Layer** (platform-agnostic) - Git operations via LibGit2Sharp
2. **Repository Platform Layer** - PR and feed operations specific to Azure DevOps or GitHub
3. **Package Management Layer** (platform-agnostic) - NuGet package resolution

**DI Registration:**
- Platform implementations registered using factory pattern in `RepositoryPlatformServiceCollectionExtensions`
- Runtime platform selection based on `EPlatformSelection` enum value
- Conditional registration of `Optional<IPackageFeedClient>` based on platform capabilities
- Azure DevOps registration via `AddRepositoryDevOps()` in `ServiceCollectionExtensions`

**Key Implementation Details:**
- Platform-specific binders (e.g., `AzureDevOpsConfigurationBinder`, `AzureDevOpsUndocumentedConfigurationBinder`) validate platform selection before binding
- Factory pattern resolves `IRepositoryClient` and `Optional<IPackageFeedClient>` at runtime based on platform
- Namespace organization: Repository abstractions in `Aviationexam.DependencyUpdater.Interfaces.Repository`

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

### Adding new repository platforms
To add support for a new platform (e.g., GitHub):
1. Create new project `Aviationexam.DependencyUpdater.Repository.<Platform>`
2. Implement `IRepositoryClient` for PR operations
3. Optionally implement `IPackageFeedClient` if the platform has package feed features
4. Create configuration class implementing `IRepositoryPlatformConfiguration`
5. Add platform to `EPlatformSelection` enum
6. Create configuration binder for platform-specific CLI arguments
7. Update `RepositoryPlatformServiceCollectionExtensions` factory to include new platform
8. Register platform services via `ServiceCollectionExtensions` in the platform project
