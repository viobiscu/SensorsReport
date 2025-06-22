using Newtonsoft.Json.Linq;
using NLog;


namespace OrionldClient
{
    internal class TG8 : ISensorEntity
    {
        protected JObject? _jsonEntity = null;

        protected static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();
        public override bool IsValid()
        {
            if (string.IsNullOrEmpty(this.Id))
            {
                Logger.Error("Program shall not reach this point!");
                return false;
            }

            return true;
        }
        public override async Task<string> CommitToOrion()
        {
            if (!this.IsValid())
                return "Invalid entity";

#pragma warning disable CS8602, CS8604 // Possible null reference argument.
                // TG8 is registred in Orion-LD and the payload is HearBeat
            if ( this.HeartBeat)
            {
                // there are two cases:

                // 1. the payload is HearBeat and the entity is registred in Orion-LD
                var result = await base.PatchEntityAttributeAsync(this.Id, "online", this.Online);
                if (result.Contains("error"))
                {
                    Logger.Error($"Error updating entity: {result}");
                }
                else if (result.StartsWith("OK:"))
                {
                    Logger.Info($"Entity {this.Id} , Tenant:  {this.TenantID} , Online:  {this.Online}");
                    return result;
                }
                // 2. the payload is HearBeat and the entity is not registred in Orion-LD
                result = await base.CreateEntityAsync(_jsonEntity);
                if (result.StartsWith("OK:"))
                {
                    // the entity is created in Orion-LD
                    Logger.Info($"Entity created: {this.Id}");
                    return result;
                }
                this.TenantID = result;

            }
            // TG8 is registred in Orion-LD and the payload is not HearBeat
            if (!this.HeartBeat)
            {
                object[] update = new JObject[] { _jsonEntity };
                var result = await base.BatchUpsertEntitiesAsync(update);
                if (result.Contains("error"))
                {
                    Logger.Error($"Error updating entity:{this.Id}, {result}");
                }
                else
                {
                    Logger.Info($"Entity {this.Id} updated, Tenant:  {this.TenantID}");
                }
                    return result;
            }
            Logger.Error("Program shall not reach this point!");
            return "Invalid entity";
#pragma warning restore CS8602, CS8604 // Possible null reference argument.
        }
        public override async Task<string> Consume(string payload)
        {
            _jsonEntity = JObject.Parse(payload);
            if (_jsonEntity != null && _jsonEntity["id"] != null)
            {
                this.Id = _jsonEntity["id"]?.Value<string>() ?? "";
            }
            else
            {
                return "No Id";
            }
            //check if attribute online exist
            if (_jsonEntity != null && _jsonEntity["online"] != null)
            {
                this.HeartBeat = true;
                // check if property "value" exist
#pragma warning disable CS8600, CS8602, CS8604 // Converting null literal or possible null value to non-nullable type.
                JToken onlineToken = _jsonEntity["online"];
                if (onlineToken.Type == JTokenType.Object && onlineToken["value"] != null && onlineToken["value"].Type == JTokenType.Boolean)
                {
                    this.Online = onlineToken["value"].Value<bool>();
                }
                else if (onlineToken.Type == JTokenType.Boolean)
                {
                    this.Online = onlineToken.Value<bool>();
                }
            }

            JToken apyKeyToken = _jsonEntity["APIKey"];
            if (apyKeyToken != null && apyKeyToken.Type == JTokenType.Object && apyKeyToken["value"] != null && apyKeyToken["value"].Type == JTokenType.String)
            {
                this.APIKey = apyKeyToken["value"].Value<String>();
            }
            if (this.APIKey == "0")
            {
                this.TenantID = string.Empty;
                return "OK";
            }

            string tenantId;
            Logger.Trace($"APIKey: {this.APIKey}");
            Program.tenantCache.TryGetTenantId(this.APIKey, out tenantId);
            Logger.Trace($"TenantId: {tenantId}");
            if (tenantId == null || tenantId == string.Empty)
            {
                Logger.Trace($"TenantId not found in cache");
            }
            else
            {
                Logger.Trace($"TenantId found in cache");
            }
            if (tenantId == null)
            {
                Orionld orionld = new Orionld();
                var results = await orionld.GetEntityAsync($"urn:ngsi-ld:APIKey:{APIKey}");
                if (results != null)
                {
                    _jsonEntity = JObject.Parse(results);
                    if (_jsonEntity != null && _jsonEntity["TenantID"] != null)
                    {
                        JToken tenantidToken = _jsonEntity["TenantID"];
                        if (tenantidToken.Type == JTokenType.Object && tenantidToken["value"] != null && tenantidToken["value"].Type == JTokenType.String)
                        {
                            tenantId = tenantidToken["value"].Value<string>();
                        }
                        if (tenantId != null)
                        {
                            Program.tenantCache.AddOrUpdate(this.APIKey, tenantId);
                            Logger.Trace($"TenantId added to catch: {tenantId}");
                        }
                    }
                    else
                    {
                        return "No TenantID";
                    }
                }

            }
            this.TenantID = tenantId;
#pragma warning restore CS8600, CS8602, CS8604
            return "OK";

        }
    }
}

