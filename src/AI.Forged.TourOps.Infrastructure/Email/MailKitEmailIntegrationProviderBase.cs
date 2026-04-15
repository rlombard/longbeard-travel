using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Models.EmailIntegrations;
using AI.Forged.TourOps.Domain.Entities;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MimeKit;

namespace AI.Forged.TourOps.Infrastructure.Email;

public abstract class MailKitEmailIntegrationProviderBase : IEmailSendProvider
{
    public abstract AI.Forged.TourOps.Domain.Enums.EmailIntegrationProviderType ProviderType { get; }
    public bool SupportsOAuth => false;
    public bool SupportsSend => true;
    public virtual bool SupportsSync => false;
    public bool SupportsWebhook => false;

    public virtual Task TestSyncCapabilityAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException($"Provider '{ProviderType}' does not support sync.");

    public virtual Task<EmailSyncEnvelope> SyncAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException($"Provider '{ProviderType}' does not support sync.");

    public async Task TestSendCapabilityAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(
            connectionSettings.OutgoingHost ?? throw new InvalidOperationException("SMTP host is required."),
            connectionSettings.OutgoingPort ?? throw new InvalidOperationException("SMTP port is required."),
            connectionSettings.OutgoingUseSsl,
            cancellationToken);
        await client.AuthenticateAsync(
            connectionSettings.OutgoingUsername ?? throw new InvalidOperationException("SMTP username is required."),
            secrets.OutgoingPassword ?? secrets.IncomingPassword ?? throw new InvalidOperationException("SMTP password is required."),
            cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    public async Task<EmailProviderSendResult> SendAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, EmailProviderSendRequest request, CancellationToken cancellationToken = default)
    {
        var message = EmailMimeFactory.Create(request);

        using var client = new SmtpClient();
        await client.ConnectAsync(
            connectionSettings.OutgoingHost ?? throw new InvalidOperationException("SMTP host is required."),
            connectionSettings.OutgoingPort ?? throw new InvalidOperationException("SMTP port is required."),
            connectionSettings.OutgoingUseSsl,
            cancellationToken);
        await client.AuthenticateAsync(
            connectionSettings.OutgoingUsername ?? throw new InvalidOperationException("SMTP username is required."),
            secrets.OutgoingPassword ?? secrets.IncomingPassword ?? throw new InvalidOperationException("SMTP password is required."),
            cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        return new EmailProviderSendResult
        {
            ProviderMessageId = message.MessageId,
            MetadataJson = $"{{\"provider\":\"{ProviderType}\"}}"
        };
    }

    protected async Task<EmailSyncEnvelope> SyncImapAsync(EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, DateTime? lastSyncedAt, CancellationToken cancellationToken)
    {
        using var client = new ImapClient();
        await client.ConnectAsync(
            connectionSettings.IncomingHost ?? throw new InvalidOperationException("IMAP host is required."),
            connectionSettings.IncomingPort ?? throw new InvalidOperationException("IMAP port is required."),
            connectionSettings.IncomingUseSsl,
            cancellationToken);
        await client.AuthenticateAsync(
            connectionSettings.IncomingUsername ?? throw new InvalidOperationException("IMAP username is required."),
            secrets.IncomingPassword ?? throw new InvalidOperationException("IMAP password is required."),
            cancellationToken);

        var folder = await client.GetFolderAsync(connectionSettings.IncomingFolder, cancellationToken);
        await folder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

        var query = lastSyncedAt.HasValue
            ? SearchQuery.DeliveredAfter(lastSyncedAt.Value.AddMinutes(-5))
            : SearchQuery.All;
        var uids = await folder.SearchAsync(query, cancellationToken);
        var ordered = uids.TakeLast(50).ToList();
        var messages = new List<EmailSyncedMessageModel>();

        foreach (var uid in ordered)
        {
            var message = await folder.GetMessageAsync(uid, cancellationToken);
            messages.Add(new EmailSyncedMessageModel
            {
                ProviderMessageId = uid.Id.ToString(),
                ProviderThreadId = message.InReplyTo ?? message.MessageId,
                FolderName = connectionSettings.IncomingFolder,
                Subject = message.Subject ?? string.Empty,
                BodyText = message.TextBody ?? message.HtmlBody ?? string.Empty,
                BodyHtml = message.HtmlBody,
                Sender = message.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty,
                Recipients = string.Join(';', message.To.Mailboxes.Select(x => x.Address)),
                SentAtUtc = message.Date.UtcDateTime
            });
        }

        await client.DisconnectAsync(true, cancellationToken);

        return new EmailSyncEnvelope
        {
            Messages = messages,
            NextCursorJson = $"{{\"lastSyncedAtUtc\":\"{DateTime.UtcNow:O}\"}}"
        };
    }
}
