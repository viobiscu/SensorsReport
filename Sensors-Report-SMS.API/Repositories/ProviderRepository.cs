using System;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Sensors_Report_SMS.API.Models;

namespace Sensors_Report_SMS.API.Repositories;

public interface IProviderRepository
{
    Task<ProviderModel> CreateAsync(ProviderModel provider);
    Task<ProviderModel?> GetByNameAsync(string providerId);
    Task<ProviderModel?> SetProviderStatusAsync(string providerName, ProviderStatusEnum status);
    Task<ProviderModel?> UpdateLastSeenAsync(string providerName);
    Task<ProviderModel?> GetByStatusAsync(ProviderStatusEnum status);
}

public class ProviderRepository : IProviderRepository
{

    private readonly AppConfiguration appConfig;
    private readonly MongoClient mongoClient;
    private readonly ILogger<ProviderRepository> logger;

    private IMongoDatabase Database => mongoClient.GetDatabase(appConfig.DatabaseName);
    private IMongoCollection<ProviderModel> Collection => Database.GetCollection<ProviderModel>(appConfig.ProviderCollectionName);

    public ProviderRepository(ILogger<ProviderRepository> logger, IOptions<AppConfiguration> appConfig)
    {
        ArgumentNullException.ThrowIfNull(appConfig);
        ArgumentNullException.ThrowIfNull(appConfig.Value.ConnectionString, nameof(appConfig.Value.ConnectionString));
        ArgumentNullException.ThrowIfNull(appConfig.Value.ProviderCollectionName, nameof(appConfig.Value.ProviderCollectionName));
        ArgumentNullException.ThrowIfNull(appConfig.Value.DatabaseName, nameof(appConfig.Value.DatabaseName));

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        this.appConfig = appConfig.Value ?? throw new ArgumentNullException(nameof(appConfig.Value), "AppConfiguration cannot be null");
        this.mongoClient = new MongoClient(this.appConfig.ConnectionString);
    }

    public async Task<ProviderModel?> GetByStatusAsync(ProviderStatusEnum status)
    {
        var filter = Builders<ProviderModel>.Filter.Eq(p => p.Status, status);
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<ProviderModel> CreateAsync(ProviderModel provider)
    {
        ArgumentNullException.ThrowIfNull(provider, nameof(provider));
        var existingProvider = await GetByNameAsync(provider.Name);
        if (existingProvider != null)
        {
            logger.LogInformation("Provider with name {ProviderName} already exists, updating last seen.", provider.Name);
            return await UpdateLastSeenAsync(provider.Name) ?? throw new InvalidOperationException("Failed to update last seen for existing provider.");
        }

        provider.Id = ObjectId.GenerateNewId().ToString();
        provider.Status = ProviderStatusEnum.Active; // Default status
        provider.LastSeen = DateTime.UtcNow;
        if (string.IsNullOrEmpty(provider.Name))
            throw new ArgumentException("Provider Name cannot be null or empty", nameof(provider.Name));
        await Collection.InsertOneAsync(provider);
        logger.LogInformation("Created new provider: {ProviderName}", provider.Name);
        return provider;
    }

    public async Task<ProviderModel?> GetByNameAsync(string providerName)
    {
        if (string.IsNullOrEmpty(providerName))
            throw new ArgumentException("Provider Name cannot be null or empty", nameof(providerName));

        var filter = Builders<ProviderModel>.Filter.Eq(p => p.Name, providerName);
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<ProviderModel?> UpdateLastSeenAsync(string providerName)
    {
        if (string.IsNullOrEmpty(providerName))
            throw new ArgumentException("Provider Name cannot be null or empty", nameof(providerName));

        var filter = Builders<ProviderModel>.Filter.Eq(p => p.Name, providerName);
        var update = Builders<ProviderModel>.Update.Set(p => p.LastSeen, DateTime.UtcNow)
            .Set(p => p.Status, ProviderStatusEnum.Active);

        var result = await Collection.FindOneAndUpdateAsync(filter, update);
        if (result == null)
        {
            return await CreateAsync(new ProviderModel
            {
                Name = providerName,
                LastSeen = DateTime.UtcNow
            });
        }

        logger.LogInformation("Updated last seen for provider: {ProviderName}", result.Name);
        return result;
    }

    public async Task<ProviderModel?> SetProviderStatusAsync(string providerName, ProviderStatusEnum status)
    {
        if (string.IsNullOrEmpty(providerName))
            throw new ArgumentException("Provider Name cannot be null or empty", nameof(providerName));

        var filter = Builders<ProviderModel>.Filter.Eq(p => p.Name, providerName);
        var update = Builders<ProviderModel>.Update.Set(p => p.Status, status)
            .Set(p => p.LastSeen, DateTime.UtcNow);

        return await Collection.FindOneAndUpdateAsync(filter, update);
    }
}
