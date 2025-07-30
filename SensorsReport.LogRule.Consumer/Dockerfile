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
COPY SensorsReport.LogRule.Consumer/SensorsReport.LogRule.Consumer.csproj ./SensorsReport.LogRule.Consumer/
COPY SensorsReport.Api.Core/SensorsReport.Api.Core.csproj ./SensorsReport.Api.Core/
COPY Directory.Build.targets ./

# Restore dependencies
RUN dotnet restore ./SensorsReport.LogRule.Consumer/SensorsReport.LogRule.Consumer.csproj

# Copy source code
COPY SensorsReport.LogRule.Consumer/ ./SensorsReport.LogRule.Consumer/
COPY SensorsReport.Api.Core/ ./SensorsReport.Api.Core/

# Build application
WORKDIR /src/SensorsReport.LogRule.Consumer
RUN dotnet build "SensorsReport.LogRule.Consumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SensorsReport.LogRule.Consumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ARG VERSION=latest
LABEL version=${VERSION}

ENTRYPOINT ["dotnet", "SensorsReport.LogRule.Consumer.dll"]
