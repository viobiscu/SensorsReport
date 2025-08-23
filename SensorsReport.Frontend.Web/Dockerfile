FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        fontconfig \
        libc6-dev \
        libgdiplus \
        libx11-dev \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN apt-get update -yq && \
    apt-get upgrade -yq \
    && apt-get install -yq curl git nano
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && apt-get install -yq nodejs 
RUN npm install -g npm

WORKDIR /src
COPY ["SensorsReport.Frontend.Web/SensorsReport.Frontend.Web.csproj", "SensorsReport.Frontend.Web/"]
COPY ["Directory.Build.targets", "."]
RUN --mount=type=secret,id=sensorsreport_secrets \
    set -eu; \
    tmp=/tmp/sr.env; \
    tr -d '\r' < /run/secrets/sensorsreport_secrets > "$tmp"; \
    set -a; . "$tmp"; set +a; \
    : "${NUGETSERENITYUSERNAME:?NUGETSERENITYUSERNAME missing}"; \
    : "${NUGETSERENITYPASSWORD:?NUGETSERENITYPASSWORD missing}"; \
    dotnet nuget add source -n serenity.is https://packages.serenity.is/v3/index.json -u "$NUGETSERENITYUSERNAME" -p "$NUGETSERENITYPASSWORD" --store-password-in-clear-text; \
    dotnet restore "SensorsReport.Frontend.Web/SensorsReport.Frontend.Web.csproj"

COPY . .
WORKDIR "/src/SensorsReport.Frontend.Web"
RUN dotnet build "SensorsReport.Frontend.Web.csproj" -c Release

FROM build AS publish
WORKDIR "/src/SensorsReport.Frontend.Web"
RUN dotnet publish "SensorsReport.Frontend.Web.csproj" --no-build -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["sh","-lc","fc-cache -f -v && dotnet SensorsReport.Frontend.Web.dll"]

