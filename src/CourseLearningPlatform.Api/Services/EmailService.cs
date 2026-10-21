using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CourseLearningPlatform.Api.Services;

public sealed class EmailService(IConfiguration configuration, ILogger<EmailService> logger)
{
    public async Task SendAsync(string to, string subject, string body)
    {
        var host = configuration["Smtp:Host"];
        var from = configuration["Smtp:From"];
        var username = configuration["Smtp:Username"];
        var password = configuration["Smtp:Password"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from) ||
            string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Email was not sent because SMTP settings are missing.");
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(from));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, configuration.GetValue<int>("Smtp:Port", 587), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not send email to {EmailAddress}.", to);
        }
    }
}
