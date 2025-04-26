# Use the base image from the existing project
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SensorsReportAudit.API/SensorsReportAudit.API.csproj", "SensorsReportAudit.API/"]
COPY ["SensorsReportAudit/SensorsReportAudit.csproj", "SensorsReportAudit/"]
RUN dotnet restore "SensorsReportAudit.API/SensorsReportAudit.API.csproj"
COPY . .
WORKDIR "/src/SensorsReportAudit.API"
RUN dotnet build "SensorsReportAudit.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SensorsReportAudit.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SensorsReportAudit.API.dll"]