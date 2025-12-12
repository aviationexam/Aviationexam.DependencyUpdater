# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/", "src/"]
COPY ["props/", "props/"]
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Packages.props", "Directory.Packages.props"]
COPY ["WarningConfiguration.targets", "WarningConfiguration.targets"]
COPY ["global.json", "global.json"]
COPY ["nuget.config", "nuget.config"]
COPY ["Aviationexam.DependencyUpdater.slnx", "Aviationexam.DependencyUpdater.slnx"]

# Restore dependencies
RUN dotnet restore src/Aviationexam.DependencyUpdater/Aviationexam.DependencyUpdater.csproj

# Build and publish the application
RUN dotnet publish src/Aviationexam.DependencyUpdater/Aviationexam.DependencyUpdater.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    --framework net10.0

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

# Install Git (required for LibGit2Sharp operations)
RUN apt-get update && \
    apt-get install -y git && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Set entrypoint to the dependency updater
ENTRYPOINT ["dotnet", "Aviationexam.DependencyUpdater.dll"]
