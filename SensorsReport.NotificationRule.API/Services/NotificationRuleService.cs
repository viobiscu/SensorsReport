using SensorsReport.OrionLD;

namespace SensorsReport.NotificationRule.API;

public class NotificationRuleService : INotificationRuleService
{
    private readonly IServiceProvider service;
    private readonly ITenantRetriever tenantRetriever;
    private IOrionLdService? _orionService;
    public IOrionLdService orionService
    {
        get
        {
            if (_orionService == null)
                _orionService = service.GetService<IOrionLdService>() ?? throw new InvalidOperationException("OrionLD service is not registered.");

            var tenantInfo = tenantRetriever.CurrentTenantInfo;
            if (tenantInfo != null)
                _orionService.SetTenant(tenantInfo.Tenant);

            return _orionService;
        }
    }

    public NotificationRuleService(IServiceProvider service, ITenantRetriever tenantRetriever)
    {
        this.service = service;
        this.tenantRetriever = tenantRetriever;
    }

    public async Task<List<NotificationRuleModel>> GetAsync(int offset = 0, int limit = 100)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));

        return await this.orionService.GetEntitiesAsync<List<NotificationRuleModel>>(
            offset: offset,
            limit: limit,
            type: "NotificationRule"
        ) ?? [];
    }

    public async Task<NotificationRuleModel?> GetAsync(string notificationRuleId)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (string.IsNullOrWhiteSpace(notificationRuleId))
            throw new ArgumentException("Notification rule ID cannot be null or empty.", nameof(notificationRuleId));

        return await this.orionService.GetEntityByIdAsync<NotificationRuleModel>(
            entityId: notificationRuleId
        ) ?? null;
    }

    public async Task<NotificationRuleModel> PostAsync(NotificationRuleModel notificationRule)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (notificationRule == null)
            throw new ArgumentNullException(nameof(notificationRule), "Notification rule model cannot be null.");
        var response = await this.orionService.CreateEntityAsync(notificationRule);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to create notification rule: {response.ReasonPhrase}");
        return notificationRule;
    }

    public async Task<NotificationRuleModel> PutAsync(string notificationRuleId, NotificationRuleModel notificationRule)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (string.IsNullOrWhiteSpace(notificationRuleId))
            throw new ArgumentException("Notification rule ID cannot be null or empty.", nameof(notificationRuleId));
        if (notificationRule == null)
            throw new ArgumentNullException(nameof(notificationRule), "Alarm model cannot be null.");
        var response = await this.orionService.UpdateEntityAsync(notificationRuleId, notificationRule);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to update notification rule: {response.ReasonPhrase}");
        return notificationRule;
    }

    public async Task DeleteAsync(string notificationRuleId)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (string.IsNullOrWhiteSpace(notificationRuleId))
            throw new ArgumentException("Notification rule ID cannot be null or empty.", nameof(notificationRuleId));
        var response = await this.orionService.DeleteEntityAsync(notificationRuleId);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to delete notification rule: {response.ReasonPhrase}");
    }
}

public interface INotificationRuleService
{
    Task<List<NotificationRuleModel>> GetAsync(int offset = 0, int limit = 100);
    Task<NotificationRuleModel?> GetAsync(string notificationRuleId);
    Task<NotificationRuleModel> PostAsync(NotificationRuleModel notificationRule);
    Task<NotificationRuleModel> PutAsync(string notificationRuleId, NotificationRuleModel notificationRule);
    Task DeleteAsync(string notificationRuleId);
}

public class NotificationRuleModel : EntityModel
{

}