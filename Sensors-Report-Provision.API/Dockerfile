FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Set environment variables
ENV DOTNET_USE_POLLING_FILE_WATCHER="false"
ENV DOTNET_MODIFIABLE_ASSEMBLIES="debug"

# Add labels for metadata
LABEL maintainer="viobiscu"
LABEL name="viobiscu/sensors-report-provision-api"
LABEL version="1.0"
LABEL description="SensorsReport Provision API service"

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore project files
COPY Sensors-Report-Provision.API/Sensors-Report-Provision.API.csproj ./
RUN dotnet restore Sensors-Report-Provision.API.csproj

# Copy source code
COPY Sensors-Report-Provision.API/ ./
RUN dotnet build "Sensors-Report-Provision.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sensors-Report-Provision.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Copy NLog configuration
COPY --from=publish /app/publish/nlog.config .

ENTRYPOINT ["dotnet", "Sensors-Report-Provision.API.dll"]