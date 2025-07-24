using SensorsReport.OrionLD;

namespace SensorsReport.AlarmRule.Consumer;

public class AlarmRuleService : IAlarmRuleService
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

    public AlarmRuleService(IServiceProvider service, ITenantRetriever tenantRetriever)
    {
        this.service = service;
        this.tenantRetriever = tenantRetriever;
    }

    public async Task<List<AlarmRuleModel>> GetAsync(int offset = 0, int limit = 100)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));

        return await this.orionService.GetEntitiesAsync<List<AlarmRuleModel>>(
            offset: offset,
            limit: limit,
            type: "AlarmRule"
        ) ?? [];
    }

    public async Task<AlarmRuleModel?> GetAsync(string alarmRuleId)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (string.IsNullOrWhiteSpace(alarmRuleId))
            throw new ArgumentException("Alarm rule ID cannot be null or empty.", nameof(alarmRuleId));

        return await this.orionService.GetEntityByIdAsync<AlarmRuleModel>(
            entityId: alarmRuleId
        ) ?? null;
    }

    public async Task<AlarmRuleModel> PostAsync(AlarmRuleModel alarmRule)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (alarmRule == null)
            throw new ArgumentNullException(nameof(alarmRule), "Alarm rule model cannot be null.");
        var response = await this.orionService.CreateEntityAsync(alarmRule);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to create alarm rule: {response.ReasonPhrase}");
        return alarmRule;
    }

    public async Task<AlarmRuleModel> PutAsync(string alarmRuleId, AlarmRuleModel alarmRule)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (string.IsNullOrWhiteSpace(alarmRuleId))
            throw new ArgumentException("Alarm rule ID cannot be null or empty.", nameof(alarmRuleId));
        if (alarmRule == null)
            throw new ArgumentNullException(nameof(alarmRule), "Alarm model cannot be null.");
        var response = await this.orionService.UpdateEntityAsync(alarmRuleId, alarmRule);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to update alarm rule: {response.ReasonPhrase}");
        return alarmRule;
    }

    public async Task DeleteAsync(string alarmRuleId)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (string.IsNullOrWhiteSpace(alarmRuleId))
            throw new ArgumentException("Alarm rule ID cannot be null or empty.", nameof(alarmRuleId));
        var response = await this.orionService.DeleteEntityAsync(alarmRuleId);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to delete alarm rule: {response.ReasonPhrase}");
    }
}

public interface IAlarmRuleService
{
    Task<List<AlarmRuleModel>> GetAsync(int offset = 0, int limit = 100);
    Task<AlarmRuleModel?> GetAsync(string alarmRuleId);
    Task<AlarmRuleModel> PostAsync(AlarmRuleModel alarmRule);
    Task<AlarmRuleModel> PutAsync(string alarmRuleId, AlarmRuleModel alarmRule);
    Task DeleteAsync(string alarmRuleId);
}

public class AlarmRuleModel : EntityModel
{

}