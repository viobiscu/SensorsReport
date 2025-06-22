FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Set environment variables
ENV ASPNETCORE_URLS="http://+:80"
ENV DOTNET_USE_POLLING_FILE_WATCHER="false"
ENV DOTNET_MODIFIABLE_ASSEMBLIES="debug"

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY Sensors-Report-Audit.API/SensorsReportAudit.API.csproj ./Sensors-Report-Audit.API/
COPY Sensors-Report-Audit/SensorsReportAudit.csproj ./Sensors-Report-Audit/
RUN dotnet restore ./Sensors-Report-Audit.API/SensorsReportAudit.API.csproj

# Copy everything else and build
COPY Sensors-Report-Audit.API/. ./Sensors-Report-Audit.API/
COPY Sensors-Report-Audit/. ./Sensors-Report-Audit/
WORKDIR /src/Sensors-Report-Audit.API
RUN pwd
RUN dotnet build SensorsReportAudit.API.csproj -c Release -o /app/build

FROM build AS publish
WORKDIR /src/Sensors-Report-Audit.API
RUN pwd
RUN dotnet publish SensorsReportAudit.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Build Docker image with version tag from version.txt
ARG VERSION=latest
COPY version.txt ./version.txt

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ARG VERSION=latest
LABEL version=$VERSION
ENTRYPOINT ["dotnet", "SensorsReportAudit.API.dll"]