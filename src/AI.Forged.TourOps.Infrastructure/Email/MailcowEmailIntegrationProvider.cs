using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Models.EmailIntegrations;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Infrastructure.Email;

public sealed class MailcowEmailIntegrationProvider : MailKitEmailIntegrationProviderBase, IEmailSyncProvider
{
    public override EmailIntegrationProviderType ProviderType => EmailIntegrationProviderType.Mailcow;
    public override bool SupportsSync => true;

    public override async Task TestSyncCapabilityAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default) =>
        _ = await SyncImapAsync(connectionSettings, secrets, connection.LastSyncedAt, cancellationToken);

    public override Task<EmailSyncEnvelope> SyncAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default) =>
        SyncImapAsync(connectionSettings, secrets, connection.LastSyncedAt, cancellationToken);
}
