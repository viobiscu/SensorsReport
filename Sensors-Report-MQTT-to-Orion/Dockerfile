FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Set environment variables
ENV DOTNET_USE_POLLING_FILE_WATCHER="false"
ENV DOTNET_MODIFIABLE_ASSEMBLIES="debug"
ENV SENSORSREPORTMQTT_TO_ORION_MQTTHOST="mqtt.sensorsreport.com"
ENV SENSORSREPORTMQTT_TO_ORION_MQTTPORT="1883"
ENV SENSORSREPORTMQTT_TO_ORION_MQTTTOPIC="Synchro/#"
ENV SENSORSREPORTMQTT_TO_ORION_ORIONURL="http://192.168.0.77:1026"
ENV SENSORSREPORTMQTT_TO_ORION_JSONLD="http://ngsi-ld.sensorsreport.net/synchro-context.jsonld"
ENV DOTNET_USE_POLLING_FILE_WATCHER="false"

# Add labels for metadata
LABEL maintainer="viobiscu"
LABEL name="viobiscu/sensorsreport-mqtt-to-orion"
LABEL version="1.0"
LABEL description="SensorsReport MQTT to Orion service"

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore project files
COPY ["SensorsReportMQTT-to-Orion.csproj", "./"]
RUN dotnet restore "SensorsReportMQTT-to-Orion.csproj"

# Copy source code
COPY . .
WORKDIR "/src"
RUN dotnet build "SensorsReportMQTT-to-Orion.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SensorsReportMQTT-to-Orion.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy NLog configuration
COPY --from=publish /app/publish/nlog.config .

ENTRYPOINT ["dotnet", "SensorsReportMQTT-to-Orion.dll"]