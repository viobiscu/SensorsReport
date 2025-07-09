using System.Text.Json;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SensorsReport.Api.Core.Helpers;

namespace Sensors_Report_Email.Consumer;

public class EmailRepository : IEmailRepository
{
    private readonly AppConfiguration appConfig;
    private readonly MongoClient mongoClient;
    private readonly ILogger<EmailRepository> logger;

    private IMongoDatabase Database => mongoClient.GetDatabase(appConfig.DatabaseName);
    private IMongoCollection<EmailModel> Collection => Database.GetCollection<EmailModel>(appConfig.EmailCollectionName);

    public EmailRepository(ILogger<EmailRepository> logger, IOptions<AppConfiguration> appConfig)
    {
        ArgumentNullException.ThrowIfNull(appConfig);
        ArgumentNullException.ThrowIfNull(appConfig.Value.ConnectionString, nameof(appConfig.Value.ConnectionString));
        ArgumentNullException.ThrowIfNull(appConfig.Value.EmailCollectionName, nameof(appConfig.Value.EmailCollectionName));
        ArgumentNullException.ThrowIfNull(appConfig.Value.DatabaseName, nameof(appConfig.Value.DatabaseName));

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        this.appConfig = appConfig.Value ?? throw new ArgumentNullException(nameof(appConfig.Value), "AppConfiguration cannot be null");
        this.mongoClient = new MongoClient(this.appConfig.ConnectionString);
    }

    public async Task<EmailModel> CreateAsync(EmailModel email)
    {
        logger.LogInformation("Creating new email record.");
        if (email == null)
            throw new ArgumentNullException(nameof(email), "Email model cannot be null");

        email.Id = ObjectId.GenerateNewId().ToString();
        email.CreatedAt = DateTime.UtcNow;
        email.LastUpdatedAt = DateTime.UtcNow;
        await Collection.InsertOneAsync(email);

        logger.LogInformation("Created new email record with ID: {Id}", email.Id);
        return email;
    }

    public async Task<List<EmailModel>> GetAllAsync(string? fromDate, string? toDate, int limit = 100, int offset = 0, EmailStatusEnum? status = null)
    {
        logger.LogInformation("Retrieving email records from: {FromDate}, to: {ToDate}, limit: {Limit}, offset: {Offset}", fromDate, toDate, limit, offset);

        var filter = Builders<EmailModel>.Filter.Empty;

        if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var from))
        {
            filter &= Builders<EmailModel>.Filter.Gte(e => e.CreatedAt, from);
        }

        if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var to))
        {
            filter &= Builders<EmailModel>.Filter.Lte(e => e.CreatedAt, to);
        }

        if (status.HasValue)
        {
            filter &= Builders<EmailModel>.Filter.Eq(e => e.Status, status.Value);
        }

        var result = await Collection.Find(filter)
            .SortBy(e => e.CreatedAt)
            .Skip(offset)
            .Limit(limit)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} email records.", result.Count);
        return result;
    }

    public async Task<EmailModel?> GetAsync(string id)
    {
        logger.LogInformation("Retrieving email record with ID: {Id}", id);
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty", nameof(id));

        var filter = Builders<EmailModel>.Filter.Eq(e => e.Id, id);
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<EmailModel> UpdateAsync(string id, EmailModel email)
    {
        logger.LogInformation("Updating email record with ID: {Id}", id);
        if (string.IsNullOrEmpty(id) || email == null)
            throw new ArgumentException("ID or email model cannot be null or empty.");

        email.Id = id;
        email.LastUpdatedAt = DateTime.UtcNow;

        var filter = Builders<EmailModel>.Filter.Eq(e => e.Id, id);

        var options = new FindOneAndReplaceOptions<EmailModel>
        {
            ReturnDocument = ReturnDocument.After // Güncellenmiş dokümanı geri döndür
        };

        var updatedEmail = await Collection.FindOneAndReplaceAsync(filter, email, options);

        if (updatedEmail == null)
            throw new Exception($"Email with ID {id} not found.");

        logger.LogInformation("Updated email record with ID: {Id}", id);
        return updatedEmail;
    }

    public async Task<EmailModel> PatchAsync(string id, JsonElement patchDoc)
    {
        logger.LogInformation("Patching email record with ID: {Id}", id);
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty", nameof(id));

        var email = await GetAsync(id) ?? throw new Exception($"Email with ID {id} not found");

        email.PatchModel(patchDoc);

        logger.LogInformation("Applying patch to email record with ID: {Id}", id);
        return await UpdateAsync(id, email);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        logger.LogInformation("Deleting email record with ID: {Id}", id);
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty", nameof(id));

        var email = await GetAsync(id);

        if (email == null)
        {
            logger.LogWarning("Email record with ID: {Id} not found for deletion.", id);
            return false;
        }

        var filter = Builders<EmailModel>.Filter.Eq(e => e.Id, id);
        var result = await Collection.DeleteOneAsync(filter);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    public async Task<EmailModel> UpdateStatusAsync(string id, EmailStatusEnum status, string? errorMessage = null)
    {
        logger.LogInformation("Updating status of email record with ID: {Id} to {Status}", id, status);
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty", nameof(id));

        var update = Builders<EmailModel>.Update
            .Set(e => e.Status, status)
            .Set(e => e.ErrorMessage, errorMessage)
            .Set(e => e.LastUpdatedAt, DateTime.UtcNow);

        if (string.IsNullOrEmpty(errorMessage))
            update.Inc(e => e.RetryCount, 1);

        var filter = Builders<EmailModel>.Filter.Eq(e => e.Id, id);
        var result = Collection.UpdateOne(filter, update);

        var entity = await GetAsync(id);
        if (result.IsAcknowledged && result.ModifiedCount > 0)
        {
            logger.LogInformation("Successfully updated status of email record with ID: {Id} Status: {Status}", id, Enum.GetName(status));
        }
        else
        {
            logger.LogWarning("Failed to update status of email record with ID: {Id} Status: {Status}", id, Enum.GetName(status));
        }

        return entity!;
    }

    public async Task<List<EmailModel>> GetReconciliationEmails()
    {
        logger.LogInformation("Retrieving reconciliation emails.");
        var filter = Builders<EmailModel>.Filter.Eq(e => e.Status, EmailStatusEnum.Pending);
        filter &= Builders<EmailModel>.Filter.Lt(e => e.CreatedAt, DateTime.UtcNow.AddMinutes(-5));

        var result = await Collection.Find(filter)
            .SortByDescending(e => e.CreatedAt)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} reconciliation emails.", result.Count);
        return result;

    }

    public async Task<EmailModel> ClaimForProcessingAsync(string emailId)
    {
        logger.LogInformation("Claiming email for processing with ID: {EmailId}", emailId);
        if (string.IsNullOrEmpty(emailId))
            throw new ArgumentException("Email ID cannot be null or empty", nameof(emailId));

        var filter = Builders<EmailModel>.Filter.Eq(e => e.Id, emailId) &
                     (Builders<EmailModel>.Filter.Eq(e => e.Status, EmailStatusEnum.Queued) |
                      Builders<EmailModel>.Filter.Eq(e => e.Status, EmailStatusEnum.Retry));

        var update = Builders<EmailModel>.Update
            .Set(e => e.Status, EmailStatusEnum.Sending)
            .Set(e => e.LastUpdatedAt, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<EmailModel>
        {
            ReturnDocument = ReturnDocument.After // Return updated document
        };

        var email = await Collection.FindOneAndUpdateAsync(filter, update, options);

        if (email == null)
        {
            logger.LogWarning("No email found with ID: {EmailId} for processing. It might be already claimed or not in the queue.", emailId);
            return email!;
        }

        logger.LogInformation("Claimed email for processing with ID: {EmailId}", emailId);
        return email;
    }
}