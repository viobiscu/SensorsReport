FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Set environment variables
ENV ASPNETCORE_URLS="http://+:80"
ENV DOTNET_USE_POLLING_FILE_WATCHER="false"
ENV DOTNET_MODIFIABLE_ASSEMBLIES="debug"
ENV ASPNETCORE_ENVIRONMENT="Production"

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore project files
COPY Sensors-Report-SMS.API/Sensors-Report-SMS.API.csproj ./Sensors-Report-SMS.API/
COPY SensorsReport.Api.Core/SensorsReport.Api.Core.csproj ./SensorsReport.Api.Core/
RUN dotnet restore ./Sensors-Report-SMS.API/Sensors-Report-SMS.API.csproj

# Copy source code
COPY Sensors-Report-SMS.API/ ./Sensors-Report-SMS.API/
COPY SensorsReport.Api.Core/ ./SensorsReport.Api.Core/
WORKDIR /src/Sensors-Report-SMS.API
RUN pwd
RUN dotnet build "Sensors-Report-SMS.API.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR /src/Sensors-Report-SMS.API
RUN pwd
RUN dotnet publish "Sensors-Report-SMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

ARG VERSION=latest

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT="Production"
COPY --from=publish /app/publish .
ARG VERSION=latest
LABEL version=$VERSION
ENTRYPOINT ["dotnet", "Sensors-Report-SMS.API.dll"]