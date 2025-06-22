using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class OrionldSeeds :  Orionld
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();
    private const string SeedsEntityId = "urn:ngsi-ld:Seeds:1";
    private const string SeedsEntityType = "Seeds";

    public OrionldSeeds() : base()
    {
    }

    public async Task<string> GenerateNewIdAsync(string entityType)
    {
        // Fetch the current counter value for the specified entity type
        long currentCounter = await GetCurrentCounterAsync(entityType);

        // Increment the counter
        long newCounter = currentCounter + 1;

        // Update the counter value in Orion-LD
        await UpdateCounterAsync(entityType, newCounter);

        // Generate the new ID
        string newId = $"urn:ngsi-ld:{entityType}:{newCounter}";
        return newId;
    }

    private async Task<long> GetCurrentCounterAsync(string entityType)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/entities/{SeedsEntityId}/attrs/{entityType}";
        var response = await _client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var attribute = JObject.Parse(content);
            return attribute["value"].Value<long>();
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // If the attribute does not exist, add the attribute with a default value
            var result = AddAttributeToEntityAsync(SeedsEntityId, entityType, "1");
            return 1;
        }
        else
        {
            throw new Exception("Failed to fetch current counter from Orion-LD: " + await response.Content.ReadAsStringAsync());
        }
    }
    private async Task<string> AddAttributeToEntityAsync(string entityId, string attributeName, object attributeValue)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/entities/{entityId}";
        var attribute = new JObject
        {
            ["id"] = entityId,
            ["type"] = SeedsEntityType,
            [attributeName] = new JObject
            {
                ["type"] = "Property",
                ["value"] = JToken.FromObject(attributeValue)
            },
            ["@context"] = new JArray
            {
                ConfigProgram.Jsonld,
                "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld"
            },
        };

        var content = new StringContent(attribute.ToString(), Encoding.UTF8, "application/ld+json");

        Logger.Trace(url);
        Logger.Trace(attribute.ToString());
        var response = await _client.PatchAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to add attribute to entity in Orion-LD: " + await response.Content.ReadAsStringAsync());
        }
        return await response.Content.ReadAsStringAsync();
    }

    private async Task UpdateCounterAsync(string entityType, long newCounter)
    {
        string url = $"{_orionUrl}/ngsi-ld/v1/entities/{SeedsEntityId}/attrs/{entityType}";
        var attribute = new JObject
        {
            ["type"] = "Property",
            ["value"] = newCounter
        };
        var content = new StringContent(attribute.ToString(), Encoding.UTF8, "application/json");
        var response = await _client.PatchAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to update counter in Orion-LD: " + await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<bool> CreateAndSaveNewSeedsEntityAsync()
    {
        // Create the new Seeds entity
        var newEntity = new JObject
        {
            ["id"] = SeedsEntityId,
            ["type"] = SeedsEntityType,
            ["name"] = new JObject
            {
                ["type"] = "Property",
                ["value"] = "New Seeds Entity"
            },
            ["@context"] = new JArray
            {
                "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld"
            }
        };

        // Save the new entity to Orion-LD
        string url = $"{_orionUrl}/ngsi-ld/v1/entities/";
        var content = new StringContent(newEntity.ToString(), Encoding.UTF8, "application/ld+json");
        var response = await _client.PostAsync(url, content);

        // Verify that the entity has been created successfully
        if (response.IsSuccessStatusCode)
        {
            Logger.Info($"Successfully created new Seeds entity with ID: {SeedsEntityId}");
            return true;
        }
        else
        {
            Logger.Error($"Failed to create new Seeds entity: {await response.Content.ReadAsStringAsync()}");
            return false;
        }
    }
}
