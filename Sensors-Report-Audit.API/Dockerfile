FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Set environment variables
ENV ASPNETCORE_URLS="http://+:80"
ENV DOTNET_USE_POLLING_FILE_WATCHER="false"
ENV DOTNET_MODIFIABLE_ASSEMBLIES="debug"

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore project files
COPY ["SensorsReportAudit.API/SensorsReportAudit.API.csproj", "SensorsReportAudit.API/"]
COPY ["SensorsReportAudit/SensorsReportAudit.csproj", "SensorsReportAudit/"]
RUN dotnet restore "SensorsReportAudit.API/SensorsReportAudit.API.csproj"

# Copy source code
COPY . .
WORKDIR "/src/SensorsReportAudit.API"
RUN dotnet build "SensorsReportAudit.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SensorsReportAudit.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SensorsReportAudit.API.dll"]