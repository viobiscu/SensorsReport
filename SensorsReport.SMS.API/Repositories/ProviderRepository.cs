using System;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SensorsReport.SMS.API.Models;

namespace SensorsReport.SMS.API.Repositories;

public class ProviderRepository : IProviderRepository
{

    private readonly ProviderMongoDbConnectionOptions mongoDbConfig;
    private readonly MongoClient mongoClient;
    private readonly ILogger<ProviderRepository> logger;

    private IMongoDatabase Database => mongoClient.GetDatabase(mongoDbConfig.DatabaseName);
    private IMongoCollection<ProviderModel> Collection => Database.GetCollection<ProviderModel>(mongoDbConfig.CollectionName);

    public ProviderRepository(ILogger<ProviderRepository> logger, IOptions<ProviderMongoDbConnectionOptions> appConfigOptions)
    {
        ArgumentNullException.ThrowIfNull(appConfigOptions);
        ArgumentNullException.ThrowIfNull(appConfigOptions.Value.ConnectionString, nameof(appConfigOptions.Value.ConnectionString));
        ArgumentNullException.ThrowIfNull(appConfigOptions.Value.CollectionName, nameof(appConfigOptions.Value.CollectionName));
        ArgumentNullException.ThrowIfNull(appConfigOptions.Value.DatabaseName, nameof(appConfigOptions.Value.DatabaseName));

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        this.mongoDbConfig = appConfigOptions.Value ?? throw new ArgumentNullException(nameof(appConfigOptions.Value), "AppConfiguration cannot be null");
        this.mongoClient = new MongoClient(this.mongoDbConfig.ConnectionString);
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

    public async Task<List<ProviderModel>> GetProvidersByCountryCode(string countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
            throw new ArgumentException("Country code cannot be null or empty", nameof(countryCode));
        var filter = Builders<ProviderModel>.Filter.AnyEq(p => p.SupportedCountryCodes, countryCode);
        return await Collection.Find(filter).ToListAsync();
    }
}
