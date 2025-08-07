using SensorsReport.SMS.API.Models;

namespace SensorsReport.SMS.API.Repositories;

public interface IProviderRepository
{
    Task<ProviderModel> CreateAsync(ProviderModel provider);
    Task<ProviderModel?> GetByNameAsync(string providerId);
    Task<ProviderModel?> SetProviderStatusAsync(string providerName, ProviderStatusEnum status);
    Task<ProviderModel?> UpdateLastSeenAsync(string providerName);
    Task<ProviderModel?> GetByStatusAsync(ProviderStatusEnum status);
    Task<List<ProviderModel>> GetProvidersByCountryCode(string countryCode);
}
