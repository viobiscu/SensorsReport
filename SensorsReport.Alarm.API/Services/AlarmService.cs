using SensorsReport.OrionLD;

namespace SensorsReport.Alarm.API;

public class AlarmService : IAlarmService
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

    public AlarmService(IServiceProvider service, ITenantRetriever tenantRetriever)
    {
        this.service = service;
        this.tenantRetriever = tenantRetriever;
    }

    public async Task<List<AlarmModel>> GetAsync(int offset = 0, int limit = 100)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));

        return await this.orionService.GetEntitiesAsync<List<AlarmModel>>(
            offset: offset,
            limit: limit,
            type: "Alarm"
        ) ?? [];
    }

    public async Task<AlarmModel?> GetAsync(string alarmId)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (string.IsNullOrWhiteSpace(alarmId))
            throw new ArgumentException("Alarm ID cannot be null or empty.", nameof(alarmId));

        return await this.orionService.GetEntityByIdAsync<AlarmModel>(
            entityId: alarmId
        ) ?? null;
    }

    public async Task<AlarmModel> PostAsync(AlarmModel alarm)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (alarm == null)
            throw new ArgumentNullException(nameof(alarm), "Alarm model cannot be null.");
        var response = await this.orionService.CreateEntityAsync(alarm);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to create alarm: {response.ReasonPhrase}");
        return alarm;
    }

    public async Task<AlarmModel> PutAsync(string alarmId, AlarmModel alarm)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (string.IsNullOrWhiteSpace(alarmId))
            throw new ArgumentException("Alarm ID cannot be null or empty.", nameof(alarmId));
        if (alarm == null)
            throw new ArgumentNullException(nameof(alarm), "Alarm model cannot be null.");
        var response = await this.orionService.UpdateEntityAsync(alarmId, alarm);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to update alarm: {response.ReasonPhrase}");
        return alarm;
    }

    public async Task DeleteAsync(string alarmId)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (string.IsNullOrWhiteSpace(alarmId))
            throw new ArgumentException("Alarm ID cannot be null or empty.", nameof(alarmId));
        var response = await this.orionService.DeleteEntityAsync(alarmId);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to delete alarm: {response.ReasonPhrase}");
    }
}

public interface IAlarmService
{
    Task<List<AlarmModel>> GetAsync(int offset = 0, int limit = 100);
    Task<AlarmModel?> GetAsync(string alarmId);
    Task<AlarmModel> PostAsync(AlarmModel alarm);
    Task<AlarmModel> PutAsync(string alarmId, AlarmModel alarm);
    Task DeleteAsync(string alarmId);
}

public class AlarmModel : EntityModel
{

}