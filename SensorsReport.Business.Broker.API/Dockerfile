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
COPY SensorsReport.Business.Broker.API/SensorsReport.Business.Broker.API.csproj ./
COPY Directory.Build.targets ./
RUN dotnet restore SensorsReport.Business.Broker.API.csproj

# Copy source code
COPY SensorsReport.Business.Broker.API/ ./
RUN dotnet build "SensorsReport.Business.Broker.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SensorsReport.Business.Broker.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SensorsReport.Business.Broker.API.dll"]