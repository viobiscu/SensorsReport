using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


class OrionldSensorEntity 
{
    protected JObject? _jsonEntity = null;
    protected static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

    public EntityType entityType { get; set; }
    public async Task<string> TryGetEntityType(string payload)
    {
        try
        {
            _jsonEntity = JObject.Parse(payload);
            string entityType = _jsonEntity["type"]?.Value<string>() ?? "";
            switch (entityType.ToUpper())
            {
                case "TG8":
                case "TG8W":
                    this.entityType = EntityType.TG8W;
                    break;
                case "TG8I":
                    this.entityType = EntityType.TG8I;
                    break;
                case "WiSensor":
                    this.entityType = EntityType.WiSensor;
                    break;
                case "BlueTooth":
                    this.entityType = EntityType.BlueTooth;
                    break;
                case "TRS":
                    this.entityType = EntityType.TRS;
                    break;
                default:
                    this.entityType = EntityType.Unknow;
                    Logger.Warn($"Unknown entity type: {entityType}");
                    return $"Unknown entity type: {entityType}";
            }
        }
        catch (JsonException jsonEx)
        {
            Logger.Error($"JSON parsing error: {jsonEx.Message}");
            return $"JSON parsing error: {jsonEx.Message}";
        }
        catch (Exception ex)
        {
            Logger.Error($"Error processing message: {ex.Message}");
            return $"Error processing message: {ex.Message}";
        }

        return "OK";
    }

}
