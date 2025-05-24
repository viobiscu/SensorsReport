using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

//APYKey entities are stored in the Orion database
// as such the TenantID header need to be set in the request to Null

public class OrionldAPIKey : Orionld
{
    public async Task<string> CreateAPIKeyEntityAsync(string APIKey, string tenantId)
    {
        this.TenantID = string.Empty;
        var entity = new
        {
            id = $"urn:ngsi-ld:APIKey:{APIKey}",
            type = "APIKey",
            APIKey = new
            {
                type = "Property",
                value = APIKey
            },
            TenantID = new
            {
                type = "Property",
                value = tenantId
            }
        };

        return await CreateEntityAsync(entity);
    }

    public async Task<string> GetTenantIdbyAPIKeyAsync(string entityId)
    {
        this.TenantID = string.Empty;
        var entity = await GetEntityAsync(entityId);
        if (entity == null)
        {
            return string.Empty;
        }
        var jsonEntity = JObject.Parse(entity);
        var tenantId = jsonEntity["TenantID"]?.Value<string>();
        if (string.IsNullOrEmpty(tenantId))
        {
            return string.Empty;
        }
        return tenantId;
    }

    public async Task<string> RetrieveAPIKeyEntityAsync(string entityId)
    {
        this.TenantID = string.Empty;
        return await GetEntityAsync(entityId);
    }
    public async Task<string> DeleteAPIKeyEntityAsync(string id)
    {
        this.TenantID = string.Empty;
        string entityId = $"urn:ngsi-ld:APIKey:{id}";
        return await DeleteEntityAsync(entityId);
    }
}
