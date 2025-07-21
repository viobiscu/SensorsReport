using System.Text.Json;
using SensorsReport.SMS.API.Models;

namespace SensorsReport.SMS.API.Repositories;

public interface ISmsRepository
{
    Task<SmsModel?> GetNextAsync(string tenant);
    Task<List<SmsModel>> GetAsync(string? tenant, string? fromDate, string? toDate, int limit = 100, int offset = 0, SmsStatusEnum? status = null);
    Task<SmsModel?> GetAsync(string id, string tenant);
    Task<SmsModel> CreateAsync(SmsModel sms);
    Task<bool> DeleteAsync(string id, string tenant);
    Task<SmsModel> PatchAsync(string id, JsonElement patchDoc, string tenant);
    Task<SmsModel> UpdateAsync(string id, SmsModel sms);
}
