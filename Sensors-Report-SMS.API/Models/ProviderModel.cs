using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SensorsReport;

namespace Sensors_Report_SMS.API.Models;

public class ProviderModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [ExcludeFromRequest]
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [ExcludeFromRequest]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [ValidateNever]
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    [ValidateNever]
    [ExcludeFromRequest]
    public ProviderStatusEnum Status { get; set; } = ProviderStatusEnum.Active;
}
