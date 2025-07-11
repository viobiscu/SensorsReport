using Microsoft.Extensions.Options;
using MimeKit;

namespace Sensors_Report_Email.Consumer;

public class EmailService : IEmailService
{
    private AppConfiguration AppConfiguration { get; }
    private ILogger<EmailService> Logger { get; }


    public EmailService(IOptions<AppConfiguration> appConfig, ILogger<EmailService> logger)
    {
        ArgumentNullException.ThrowIfNull(appConfig);
        this.AppConfiguration = appConfig.Value ?? throw new ArgumentNullException(nameof(appConfig.Value), "AppConfiguration cannot be null");
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");

    }

    public async Task SendEmailAsync(EmailModel emailModel)
    {
        var replyToAddress = $"{emailModel.Id}+{emailModel.FromEmail}";
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(emailModel.FromName, emailModel.FromEmail));
        message.To.Add(new MailboxAddress(emailModel.ToName, emailModel.ToEmail));
        message.Headers.Add("X-Email-Id", emailModel.Id);
        message.References.Add(emailModel.Id);
        message.ReplyTo.Add(new MailboxAddress(emailModel.FromName, replyToAddress));

        if (!string.IsNullOrEmpty(emailModel.CcEmail))
            message.Cc.Add(new MailboxAddress(emailModel.CcName, emailModel.CcEmail));

        if (!string.IsNullOrEmpty(emailModel.BccEmail))
            message.Bcc.Add(new MailboxAddress(emailModel.BccName, emailModel.BccEmail));

        message.Subject = $"{emailModel.Subject} [Ref:{emailModel.Id}]";
        message.Body = new TextPart("html") { Text = $"{emailModel.BodyHtml} <small style=\"font-size:10px; color:#999;\">Ref:<i>{emailModel.Id}</i></small>" };

        try
        {
            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(AppConfiguration.SmtpServer, AppConfiguration.SmtpPort, AppConfiguration.UseSsl);
            await client.AuthenticateAsync(AppConfiguration.SmtpUsername, AppConfiguration.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            Logger.LogInformation("Email sent successfully to {ToEmail}", emailModel.ToEmail);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to send email to {ToEmail}", emailModel.ToEmail);
            throw;
        }
    }
}
