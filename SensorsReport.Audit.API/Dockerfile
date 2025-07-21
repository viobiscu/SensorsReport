FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Set environment variables
ENV ASPNETCORE_URLS="http://+:80"
ENV DOTNET_USE_POLLING_FILE_WATCHER="false"
ENV DOTNET_MODIFIABLE_ASSEMBLIES="debug"
ENV ASPNETCORE_ENVIRONMENT="Production"

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for dependency resolution
COPY SensorsReport.Audit.API/SensorsReport.Audit.API.csproj ./SensorsReport.Audit.API/
COPY SensorsReport.Audit/SensorsReport.Audit.csproj ./SensorsReport.Audit/
COPY SensorsReport.Api.Core/SensorsReport.Api.Core.csproj ./SensorsReport.Api.Core/
COPY Directory.Build.targets ./

# Restore dependencies
RUN dotnet restore ./SensorsReport.Audit.API/SensorsReport.Audit.API.csproj

# Copy source code
COPY SensorsReport.Audit.API/ ./SensorsReport.Audit.API/
COPY SensorsReport.Audit/ ./SensorsReport.Audit/
COPY SensorsReport.Api.Core/ ./SensorsReport.Api.Core/

# Build application
WORKDIR /src/SensorsReport.Audit.API
RUN dotnet build "SensorsReport.Audit.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SensorsReport.Audit.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ARG VERSION=latest
LABEL version=${VERSION}

ENTRYPOINT ["dotnet", "SensorsReport.Audit.API.dll"]
