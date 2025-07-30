using SensorsReport.OrionLD;

namespace SensorsReport.LogRule.Consumer;

public class LogRuleService : ILogRuleService
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

    public LogRuleService(IServiceProvider service, ITenantRetriever tenantRetriever)
    {
        this.service = service;
        this.tenantRetriever = tenantRetriever;
    }

    public async Task<List<LogRuleModel>> GetAsync(int offset = 0, int limit = 100)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));

        return await this.orionService.GetEntitiesAsync<List<LogRuleModel>>(
            offset: offset,
            limit: limit,
            type: "LogRule"
        ) ?? [];
    }

    public async Task<LogRuleModel?> GetAsync(string logRuleId)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (string.IsNullOrWhiteSpace(logRuleId))
            throw new ArgumentException("Log rule ID cannot be null or empty.", nameof(logRuleId));

        return await this.orionService.GetEntityByIdAsync<LogRuleModel>(
            entityId: logRuleId
        ) ?? null;
    }

    public async Task<LogRuleModel> PostAsync(LogRuleModel logRule)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (logRule == null)
            throw new ArgumentNullException(nameof(logRule), "Log rule model cannot be null.");
        var response = await this.orionService.CreateEntityAsync(logRule);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to create log rule: {response.ReasonPhrase}");
        return logRule;
    }

    public async Task<LogRuleModel> PutAsync(string logRuleId, LogRuleModel logRule)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (string.IsNullOrWhiteSpace(logRuleId))
            throw new ArgumentException("Log rule ID cannot be null or empty.", nameof(logRuleId));
        if (logRule == null)
            throw new ArgumentNullException(nameof(logRule), "Log model cannot be null.");
        var response = await this.orionService.UpdateEntityAsync(logRuleId, logRule);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to update log rule: {response.ReasonPhrase}");
        return logRule;
    }

    public async Task DeleteAsync(string logRuleId)
    {
        ArgumentNullException.ThrowIfNull(orionService, nameof(orionService));
        ArgumentNullException.ThrowIfNull(tenantRetriever, nameof(tenantRetriever));
        if (string.IsNullOrWhiteSpace(logRuleId))
            throw new ArgumentException("Log rule ID cannot be null or empty.", nameof(logRuleId));
        var response = await this.orionService.DeleteEntityAsync(logRuleId);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to delete log rule: {response.ReasonPhrase}");
    }
}

public interface ILogRuleService
{
    Task<List<LogRuleModel>> GetAsync(int offset = 0, int limit = 100);
    Task<LogRuleModel?> GetAsync(string logRuleId);
    Task<LogRuleModel> PostAsync(LogRuleModel logRule);
    Task<LogRuleModel> PutAsync(string logRuleId, LogRuleModel logRule);
    Task DeleteAsync(string logRuleId);
}
