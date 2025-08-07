using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SensorsReport.SMS.API.Models;
using System.Text.Json;

namespace SensorsReport.SMS.API.Repositories;

public class SmsRepository : ISmsRepository
{
    private readonly SmsMongoDbConnectionOptions smsDbConfig;
    private readonly MongoClient mongoClient;
    private readonly ILogger<SmsRepository> logger;

    private IMongoDatabase Database => mongoClient.GetDatabase(smsDbConfig.DatabaseName);
    private IMongoCollection<SmsModel> Collection => Database.GetCollection<SmsModel>(smsDbConfig.CollectionName);

    public SmsRepository(ILogger<SmsRepository> logger, IOptions<SmsMongoDbConnectionOptions> smsMongoDbOptions)
    {
        ArgumentNullException.ThrowIfNull(smsMongoDbOptions);
        ArgumentNullException.ThrowIfNull(smsMongoDbOptions.Value.ConnectionString, nameof(smsMongoDbOptions.Value.ConnectionString));
        ArgumentNullException.ThrowIfNull(smsMongoDbOptions.Value.CollectionName, nameof(smsMongoDbOptions.Value.CollectionName));
        ArgumentNullException.ThrowIfNull(smsMongoDbOptions.Value.DatabaseName, nameof(smsMongoDbOptions.Value.DatabaseName));

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        this.smsDbConfig = smsMongoDbOptions.Value ?? throw new ArgumentNullException(nameof(smsMongoDbOptions.Value), "AppConfiguration cannot be null");
        this.mongoClient = new MongoClient(this.smsDbConfig.ConnectionString);
    }

    public async Task<SmsModel?> GetNextAsync(string provider, string[]? countryCodes = null, string? tenant = null)
    {
        logger.LogInformation("Retrieving next SMS record for provider: {Provider}, status: {Status}", provider, SmsStatusEnum.Pending);
        if (string.IsNullOrEmpty(provider))
            throw new ArgumentException("Provider cannot be null or empty", nameof(provider));

        var filter = Builders<SmsModel>.Filter.Eq(s => s.Status, SmsStatusEnum.Pending);

        if (!string.IsNullOrEmpty(tenant))
        {
            filter &= Builders<SmsModel>.Filter.Eq(s => s.Tenant, tenant);
        }

        if (countryCodes?.Length > 0)
        {
            filter &= Builders<SmsModel>.Filter.In(s => s.CountryCode, countryCodes);
            filter &= Builders<SmsModel>.Filter.Or(Builders<SmsModel>.Filter.Eq(s => s.Provider, provider),
                Builders<SmsModel>.Filter.Eq(s => s.Provider, null));
        }
        else
        {
            filter &= Builders<SmsModel>.Filter.Eq(s => s.Provider, provider);
        }

        var result = await Collection.Find(filter).FirstOrDefaultAsync();
        if (result == null)
        {
            logger.LogWarning("No SMS records found for provider: {Provider}, status: {Status}", provider, SmsStatusEnum.Pending);
            return null;
        }

        result.Status = SmsStatusEnum.Entrusted; // Update status to Entrusted
        result.SentAt = DateTime.UtcNow; // Set SentAt to current time
        result.RetryCount++; // Increment retry count
        var update = Builders<SmsModel>.Update
            .Set(s => s.Status, result.Status)
            .Set(s => s.SentAt, result.SentAt)
            .Set(s => s.RetryCount, result.RetryCount);
        await Collection.UpdateOneAsync(filter, update);

        logger.LogInformation("Retrieved SMS record with ID: {Id} for provider: {Provider}, status: {Status}, tenant: {Tenant}", result.Id, provider, SmsStatusEnum.Pending, result.Tenant);
        return result;
    }

    public async Task<List<SmsModel>> GetAsync(string? tenant, string? fromDate, string? toDate, int limit = 100, int offset = 0, SmsStatusEnum? status = null, string? countryCode = null)
    {
        logger.LogInformation("Retrieving SMS records for tenant: {Tenant}, from: {FromDate}, to: {ToDate}, limit: {Limit}, offset: {Offset}",
            tenant, fromDate, toDate, limit, offset);

        var filter = Builders<SmsModel>.Filter.Empty;
        if (!string.IsNullOrEmpty(tenant))
        {
            filter &= Builders<SmsModel>.Filter.Eq(s => s.Tenant, tenant);
        }

        if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var from))
        {
            filter &= Builders<SmsModel>.Filter.Gte(s => s.Timestamp, from);
        }

        if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var to))
        {
            filter &= Builders<SmsModel>.Filter.Lte(s => s.Timestamp, to);
        }

        if (!string.IsNullOrEmpty(countryCode))
        {
            filter &= Builders<SmsModel>.Filter.Eq(s => s.CountryCode, countryCode);
        }

        if (status.HasValue)
        {
            filter &= Builders<SmsModel>.Filter.Eq(s => s.Status, status.Value);
        }

        var query = Collection.Find(filter);

        query = query.Limit(limit);
        query = query.Skip(offset);

        var result = await query.ToListAsync();

        logger.LogInformation("Retrieved {Count} SMS records for tenant: {Tenant}", result.Count, tenant);
        return result;
    }

    public async Task<SmsModel?> GetAsync(string id, string tenant)
    {
        logger.LogInformation("Retrieving SMS record with ID: {Id} for tenant: {Tenant}", id, tenant);
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty", nameof(id));

        if (string.IsNullOrEmpty(tenant))
            throw new ArgumentException("Tenant cannot be null or empty", nameof(tenant));

        var filter = Builders<SmsModel>.Filter.And(
            Builders<SmsModel>.Filter.Eq(s => s.Id, id),
            Builders<SmsModel>.Filter.Eq(s => s.Tenant, tenant)
        );

        var result = await Collection.Find(filter).FirstOrDefaultAsync();
        if (result == null)
        {
            logger.LogWarning("SMS record with ID: {Id} not found for tenant: {Tenant}", id, tenant);
            return null;
        }
        logger.LogInformation("Retrieved SMS record with ID: {Id} for tenant: {Tenant}", id, tenant);
        return result;
    }

    public async Task<SmsModel> CreateAsync(SmsModel sms, string tenant)
    {
        logger.LogInformation("Creating new SMS record for tenant: {Tenant}", sms.Tenant);
        if (sms == null)
            throw new ArgumentNullException(nameof(sms), "SMS model cannot be null");

        if (string.IsNullOrEmpty(sms.Tenant))
            throw new ArgumentException("Tenant cannot be null or empty", nameof(sms.Tenant));

        sms.Id = ObjectId.GenerateNewId().ToString();
        sms.Tenant = tenant;
        if (string.IsNullOrEmpty(sms.CountryCode))
            sms.CountryCode = PhoneNumberHelper.GetCountryCode(sms.PhoneNumber);

        await Collection.InsertOneAsync(sms);
        logger.LogInformation("Created new SMS record with ID: {Id} for tenant: {Tenant}", sms.Id, sms.Tenant);
        return sms;
    }

    public async Task<SmsModel> UpdateAsync(string id, SmsModel sms, string tenant)
    {
        logger.LogInformation("Updating SMS record with ID: {Id}", id);
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty", nameof(id));

        if (sms == null)
            throw new ArgumentNullException(nameof(sms), "SMS model cannot be null");

        if (string.IsNullOrEmpty(sms.CountryCode))
            sms.CountryCode = PhoneNumberHelper.GetCountryCode(sms.PhoneNumber);

        var filter = Builders<SmsModel>.Filter.And(Builders<SmsModel>.Filter.Eq(s => s.Id, id),
                     Builders<SmsModel>.Filter.Eq(s => s.Tenant, tenant));
        var update = Builders<SmsModel>.Update
            .Set(s => s.PhoneNumber, sms.PhoneNumber)
            .Set(s => s.Message, sms.Message)
            .Set(s => s.Status, sms.Status)
            .Set(s => s.SentAt, sms.SentAt)
            .Set(s => s.CountryCode, sms.CountryCode)
            .Set(s => s.Provider, sms.Provider)
            .Set(s => s.TrackingId, sms.TrackingId)
            .Set(s => s.MessageType, sms.MessageType)
            .Set(s => s.CustomData, sms.CustomData)
            .Set(s => s.RetryCount, sms.RetryCount);

        var result = await Collection.FindOneAndUpdateAsync(filter, update) ?? throw new Exception($"SMS with ID {id} not found");

        result = await Collection.Find(filter).FirstOrDefaultAsync();
        if (result == null)
            throw new Exception($"SMS with ID {id} not found after update");
        logger.LogInformation("Updated SMS record with ID: {Id}", id);
        return result;
    }

    public async Task<bool> DeleteAsync(string id, string tenant)
    {
        logger.LogInformation("Deleting SMS record with ID: {Id} for tenant: {Tenant}", id, tenant);
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty", nameof(id));

        if (string.IsNullOrEmpty(tenant))
            throw new ArgumentException("Tenant cannot be null or empty", nameof(tenant));

        var filter = Builders<SmsModel>.Filter.And(
            Builders<SmsModel>.Filter.Eq(s => s.Id, id),
            Builders<SmsModel>.Filter.Eq(s => s.Tenant, tenant)
        );

        var result = await Collection.DeleteOneAsync(filter);
        if (result.DeletedCount == 0)
        {
            logger.LogWarning("SMS record with ID: {Id} not found for tenant: {Tenant}", id, tenant);
            return false;
        }

        logger.LogInformation("Deleted SMS record with ID: {Id} for tenant: {Tenant}", id, tenant);
        return result.DeletedCount > 0;
    }

    public async Task<SmsModel> PatchAsync(string id, JsonElement patchDoc, string tenant)
    {
        logger.LogInformation("Patching SMS record with ID: {Id} for tenant: {Tenant}", id, tenant);
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty", nameof(id));

        if (string.IsNullOrEmpty(tenant))
            throw new ArgumentException("Tenant cannot be null or empty", nameof(tenant));

        var sms = await GetAsync(id, tenant) ?? throw new Exception($"SMS with ID {id} not found");
        sms.PatchModel(patchDoc);

        logger.LogInformation("Applying patch to SMS record with ID: {Id} for tenant: {Tenant}", id, tenant);
        return await UpdateAsync(id, sms, tenant);
    }
}
