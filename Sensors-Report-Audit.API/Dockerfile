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
COPY Sensors-Report-Audit.API/SensorsReportAudit.API.csproj Sensors-Report-Audit.API/
COPY Sensors-Report-Audit/SensorsReportAudit.csproj Sensors-Report-Audit/
RUN dotnet restore Sensors-Report-Audit.API/SensorsReportAudit.API.csproj

# Copy everything else and build
COPY Sensors-Report-Audit.API/ Sensors-Report-Audit.API/
COPY Sensors-Report-Audit/ Sensors-Report-Audit/
WORKDIR /src/Sensors-Report-Audit.API
RUN dotnet build SensorsReportAudit.API.csproj -c Release -o /app/build

FROM build AS publish
WORKDIR /src/Sensors-Report-Audit.API
RUN dotnet publish SensorsReportAudit.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY Sensors-Report-Audit.API/nlog.config .
ENTRYPOINT ["dotnet", "SensorsReportAudit.API.dll"]