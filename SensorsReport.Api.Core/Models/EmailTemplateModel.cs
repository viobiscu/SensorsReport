using System.Text.Json.Serialization;

namespace SensorsReport;

public class EmailTemplateModel : EntityModel
{
    [JsonPropertyName("subject")]
    public ValuePropertyModel<string>? Subject { get; set; }
    [JsonPropertyName("body")]
    public ValuePropertyModel<string>? Body { get; set; }
}

public class SmsTemplateModel : EntityModel
{
    [JsonPropertyName("message")]
    public ValuePropertyModel<string>? Message { get; set; }
}