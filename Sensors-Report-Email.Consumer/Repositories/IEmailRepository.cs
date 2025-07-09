using System.Text.Json;

namespace Sensors_Report_Email.Consumer;

public interface IEmailRepository
{
    Task<EmailModel> CreateAsync(EmailModel email);
    Task<bool> DeleteAsync(string id);
    Task<List<EmailModel>> GetAllAsync(string? fromDate, string? toDate, int limit = 100, int offset = 0, EmailStatusEnum? status = null);
    Task<EmailModel?> GetAsync(string id);
    Task<EmailModel> PatchAsync(string id, JsonElement patchDoc);
    Task<EmailModel> UpdateAsync(string id, EmailModel email);
    Task<EmailModel> UpdateStatusAsync(string id, EmailStatusEnum status, string? errorMessage = null);
    Task<List<EmailModel>> GetReconciliationEmails();
    Task<EmailModel> ClaimForProcessingAsync(string emailId);
}