using System.Text.Json;
using SensorsReport.SMS.API.Models;

namespace SensorsReport.SMS.API.Repositories;

public interface ISmsRepository
{
    Task<SmsModel?> GetNextAsync(string provider, string[]? countryCodes = null, string? tenant = null);
    Task<List<SmsModel>> GetAsync(string? tenant, string? fromDate, string? toDate, int limit = 100, int offset = 0, SmsStatusEnum? status = null, string? countryCode = null);
    Task<SmsModel?> GetAsync(string id, string tenant);
    Task<SmsModel> CreateAsync(SmsModel sms, string tenant);
    Task<SmsModel> UpdateAsync(string id, SmsModel sms, string tenant);
    Task<bool> DeleteAsync(string id, string tenant);
    Task<SmsModel> PatchAsync(string id, JsonElement patchDoc, string tenant);
}
