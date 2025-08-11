using SensorsReport.Api.Core.MassTransit;
using SensorsReport.OrionLD;
using System.Text.Json;

namespace SensorsReport.NotificationRule.Consumer;

public class MessageService(ILogger<MessageService> logger, IEventBus eventBus) : IMessageService
{
    public async Task SendSms(IOrionLdService orionLdService, IEnumerable<UserModel> users, string smsTemplateKey, TenantInfo? tenant, Dictionary<string, string> parameters)
    {
        foreach (var user in users)
        {
            if (user.Mobile?.Value is null)
            {
                logger.LogWarning("User {UserId} does not have a mobile number", user.Id);
                continue;
            }
            var smsTemplate = await orionLdService.GetEntityByIdAsync<SmsTemplateModel>(smsTemplateKey);

            var userParameters = new Dictionary<string, string>(parameters)
            {
                { "UserName", $"{user.FirstName?.Value} {user.LastName?.Value}" }
            };

            var body = TemplateHelper.FormatString(smsTemplate?.Message?.Value ?? DefaultSmsMessage, parameters);
            body = TemplateHelper.FormatString(body, userParameters);

            logger.LogInformation("Creating SMS to {ToMobile} with body: {Body}", user.Mobile.Value, body);
            await eventBus.PublishAsync(new CreateSmsCommand
            {
                Message = body,
                PhoneNumber = user.Mobile.Value,
                Tenant = tenant?.Tenant,
                MessageType = "Alarm"
            });
        }
    }

    public async Task SendEmail(IOrionLdService orionLdService, IEnumerable<UserModel> users, string emailTemplateKey, TenantInfo? tenant, Dictionary<string, string> parameters)
    {
        foreach (var user in users)
        {
            if (user.Email?.Value is null)
            {
                logger.LogWarning("User {UserId} does not have an email address", user.Id);
                continue;
            }

            var emailTemplate = await orionLdService.GetEntityByIdAsync<EmailTemplateModel>(emailTemplateKey);
            var userParameters = new Dictionary<string, string>(parameters)
            {
                { "UserName", $"{user.FirstName?.Value} {user.LastName?.Value}" }
            };

            var subject = TemplateHelper.FormatString(UnescapeJsonString(emailTemplate?.Subject?.Value ?? DefaultEmailSubject), parameters);
            subject = TemplateHelper.FormatString(subject, userParameters);
            var body = TemplateHelper.FormatString(UnescapeJsonString(emailTemplate?.Body?.Value ?? DefaultEmailBody), parameters);
            body = TemplateHelper.FormatString(body, userParameters);

            logger.LogInformation("Creating email to {ToEmail} with subject: {Subject}", user.Email.Value, subject);
            await eventBus.PublishAsync(new CreateEmailCommand
            {
                ToEmail = user.Email?.Value,
                ToName = $"{user.FirstName?.Value} {user.LastName?.Value}",
                Subject = subject,
                BodyHtml = body,
                Tenant = tenant?.Tenant
            });
        }
    }

    private string UnescapeJsonString(string jsonEscapedString)
    {
        if (string.IsNullOrEmpty(jsonEscapedString))
            return jsonEscapedString;

        try
        {
            string wrappedJson = $"\"{jsonEscapedString}\"";
            return JsonSerializer.Deserialize<string>(wrappedJson)!;
        }
        catch
        {
            return jsonEscapedString
                .Replace("\\\"", "\"")      // Unescape quotes
                .Replace("\\\\", "\\")      // Unescape backslashes
                .Replace("\\n", "\n")       // Unescape newlines
                .Replace("\\r", "\r")       // Unescape carriage returns
                .Replace("\\t", "\t")       // Unescape tabs
                .Replace("\\/", "/");
        }
    }

    private const string DefaultSmsMessage = "{{AlarmDescription}} {{SensorId}} {{SensorName}} {{SensorLocation}} {{AlarmType}} {{AttributeValue}} {{AttributeUnit}}";
    private const string DefaultEmailSubject = "SensorsReport - Alarm Notification";
    private const string DefaultEmailBody = """
        Dear {{UserName}},</br>
        The following alarm has occurred:</br>
        Alarm Description: {{AlarmDescription}}</br>
        Sensor Location: {{SensorLocation}}</br>
        Sensor Name: {{SensorName}}</br>
        Sensor ID: {{SensorId}}</br>
        Current Value: {{AttributeValue}} {{AttributeUnit}}</br>
        </br>
        Regards,</br>
        http://www.sensorsreport.com
    """;
}
