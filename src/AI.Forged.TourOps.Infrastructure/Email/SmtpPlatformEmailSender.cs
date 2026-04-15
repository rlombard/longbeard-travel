using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Infrastructure.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AI.Forged.TourOps.Infrastructure.Email;

public sealed class SmtpPlatformEmailSender(IOptions<SignupEmailSettings> settings) : IPlatformEmailSender
{
    public async Task SendSignupVerificationAsync(string toEmail, string organizationName, string verificationLink, CancellationToken cancellationToken = default)
    {
        var config = settings.Value;
        if (string.IsNullOrWhiteSpace(config.SmtpHost))
        {
            throw new InvalidOperationException("Signup SMTP host is not configured.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(config.FromDisplayName, config.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = $"Verify your {organizationName} TourOps signup";
        message.Body = new TextPart("plain")
        {
            Text = $"Verify your email to continue signup:\n\n{verificationLink}\n"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            config.SmtpHost,
            config.SmtpPort,
            config.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(config.Username))
        {
            await client.AuthenticateAsync(config.Username, config.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
