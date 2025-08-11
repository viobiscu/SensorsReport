using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Text.Json;

namespace SensorsReport.AlarmRule.Consumer;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationMongoDbConnectionOptions notificationDbOptions;
    private readonly MongoClient mongoClient;
    private readonly ILogger<NotificationRepository> logger;

    private IMongoDatabase Database => mongoClient.GetDatabase(notificationDbOptions.DatabaseName);
    private IMongoCollection<NotificationMonitorModel> Collection => Database.GetCollection<NotificationMonitorModel>(notificationDbOptions.CollectionName);

    public NotificationRepository(ILogger<NotificationRepository> logger, IOptions<NotificationMongoDbConnectionOptions> notificationDbOptions)
    {
        ArgumentNullException.ThrowIfNull(notificationDbOptions);
        ArgumentNullException.ThrowIfNull(notificationDbOptions.Value.ConnectionString, nameof(notificationDbOptions.Value.ConnectionString));
        ArgumentNullException.ThrowIfNull(notificationDbOptions.Value.CollectionName, nameof(notificationDbOptions.Value.CollectionName));
        ArgumentNullException.ThrowIfNull(notificationDbOptions.Value.DatabaseName, nameof(notificationDbOptions.Value.DatabaseName));

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        this.notificationDbOptions = notificationDbOptions.Value ?? throw new ArgumentNullException(nameof(notificationDbOptions.Value), "NotificationMongoDbConnectionOptions cannot be null");
        this.mongoClient = new MongoClient(this.notificationDbOptions.ConnectionString);
        _ = InitializeCollectionAsync();
    }

    public async Task InitializeCollectionAsync()
    {
        logger.LogInformation("Initializing NotificationMonitor collection.");
        if (string.IsNullOrEmpty(notificationDbOptions.CollectionName))
            throw new ArgumentException("Collection name cannot be null or empty", nameof(notificationDbOptions.CollectionName));
        var existingCollections = await (await Database.ListCollectionNamesAsync()).ToListAsync();
        if (existingCollections.Any(name => name == notificationDbOptions.CollectionName))
        {
            logger.LogInformation("Creating NotificationMonitor collection: {CollectionName}", notificationDbOptions.CollectionName);
            await Database.CreateCollectionAsync(notificationDbOptions.CollectionName);
        }
        else
        {
            logger.LogInformation("NotificationMonitor collection already exists: {CollectionName}", notificationDbOptions.CollectionName);
        }
    }

    public async Task<NotificationMonitorModel> CreateAsync(NotificationMonitorModel notificationModel)
    {
        logger.LogInformation("Creating new NotificationMonitor record.");
        if (notificationModel == null)
            throw new ArgumentNullException(nameof(notificationModel), "NotificationMonitor model cannot be null");

        notificationModel.Id = ObjectId.GenerateNewId().ToString();
        notificationModel.CreatedAt = DateTime.UtcNow;
        notificationModel.LastUpdatedAt = DateTime.UtcNow;
        await Collection.InsertOneAsync(notificationModel);

        logger.LogInformation("Created new NotificationMonitor record with ID: {Id}", notificationModel.Id);
        return notificationModel;
    }

    public async Task<NotificationMonitorModel?> GetAsync(string id)
    {
        logger.LogInformation("Retrieving NotificationMonitor record with ID: {Id}", id);
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty", nameof(id));

        var filter = Builders<NotificationMonitorModel>.Filter.Eq(e => e.Id, id);
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<NotificationMonitorModel> UpdateAsync(string id, NotificationMonitorModel notificationMonitor)
    {
        logger.LogInformation("Updating NotificationMonitor record with ID: {Id}", id);
        if (string.IsNullOrEmpty(id) || notificationMonitor == null)
            throw new ArgumentException("ID or NotificationMonitor model cannot be null or empty.");

        notificationMonitor.Id = id;
        notificationMonitor.LastUpdatedAt = DateTime.UtcNow;

        var filter = Builders<NotificationMonitorModel>.Filter.Eq(e => e.Id, id);

        var options = new FindOneAndReplaceOptions<NotificationMonitorModel>
        {
            ReturnDocument = ReturnDocument.After // Güncellenmiş dokümanı geri döndür
        };

        var updatedNotificationMonitor = await Collection.FindOneAndReplaceAsync(filter, notificationMonitor, options);

        if (updatedNotificationMonitor == null)
            throw new Exception($"NotificationMonitor with ID {id} not found.");

        logger.LogInformation("Updated NotificationMonitor record with ID: {Id}", id);
        return updatedNotificationMonitor;
    }

    public async Task<NotificationMonitorModel> UpdateStatusAsync(string id, NotificationMonitorStatusEnum status, string? errorMessage = null)
    {
        logger.LogInformation("Updating status of NotificationMonitor record with ID: {Id} to {Status}", id, status);
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty", nameof(id));

        var update = Builders<NotificationMonitorModel>.Update
            .Set(e => e.Status, status)
            .Set(e => e.Message, errorMessage)
            .Set(e => e.LastUpdatedAt, DateTime.UtcNow);

        var filter = Builders<NotificationMonitorModel>.Filter.Eq(e => e.Id, id);
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

    public async Task<NotificationMonitorModel?> GetByAlarmForProcessingAsync(string alarmId)
    {
        logger.LogInformation("Retrieving NotificationMonitor record for alarm ID: {AlarmId}", alarmId);
        if (string.IsNullOrEmpty(alarmId))
            throw new ArgumentException("Alarm ID cannot be null or empty", nameof(alarmId));

        var filter = Builders<NotificationMonitorModel>.Filter.Eq(e => e.AlarmId, alarmId);
        filter &= Builders<NotificationMonitorModel>.Filter.Or(
            Builders<NotificationMonitorModel>.Filter.Eq(e => e.Status, NotificationMonitorStatusEnum.Watching),
            Builders<NotificationMonitorModel>.Filter.Eq(e => e.Status, NotificationMonitorStatusEnum.Acknowledged));

        return await Collection.FindOneAndUpdateAsync(filter,
            Builders<NotificationMonitorModel>.Update
                .Set(e => e.Status, NotificationMonitorStatusEnum.Processing)
                .Set(e => e.LastUpdatedAt, DateTime.UtcNow),
            new FindOneAndUpdateOptions<NotificationMonitorModel>
            {
                ReturnDocument = ReturnDocument.Before
            });
    }

    public async Task<List<NotificationMonitorModel>> GetNextNotificationProcessingAsync(int skip = 0, int take = 10)
    {
        logger.LogInformation("Retrieving next NotificationMonitor record for processing.");

        var filter = Builders<NotificationMonitorModel>.Filter.Or(
            Builders<NotificationMonitorModel>.Filter.Eq(e => e.Status, NotificationMonitorStatusEnum.Watching),
            Builders<NotificationMonitorModel>.Filter.Eq(e => e.Status, NotificationMonitorStatusEnum.Acknowledged),
            Builders<NotificationMonitorModel>.Filter.And(Builders<NotificationMonitorModel>.Filter.Eq(e => e.Status, NotificationMonitorStatusEnum.Processing),
            Builders<NotificationMonitorModel>.Filter.Gt(e => e.LastUpdatedAt, DateTime.UtcNow.AddMinutes(-10))));

        var options = new FindOptions<NotificationMonitorModel>
        {
            Sort = Builders<NotificationMonitorModel>.Sort.Ascending(e => e.LastNotificationSentAt),
            Skip = skip,
            Limit = take
        };

        var results = await Collection.FindAsync(filter, options);
        return await results.ToListAsync();
    }
}