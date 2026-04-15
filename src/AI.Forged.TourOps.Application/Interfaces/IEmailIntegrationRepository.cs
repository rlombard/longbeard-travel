using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IEmailIntegrationRepository
{
    Task<EmailProviderConnection> AddConnectionAsync(EmailProviderConnection connection, CancellationToken cancellationToken = default);
    Task<EmailProviderConnection?> GetConnectionByIdAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailProviderConnection>> GetConnectionsByOwnerAsync(string ownerUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailProviderConnection>> GetDueForSyncAsync(DateTime utcNow, CancellationToken cancellationToken = default);
    Task<EmailProviderConnection?> GetByOAuthStateAsync(string oauthState, CancellationToken cancellationToken = default);
    Task<EmailProviderConnection?> GetDefaultSendConnectionAsync(string ownerUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailProviderConnection>> GetByWebhookSubscriptionIdsAsync(IEnumerable<string> subscriptionIds, CancellationToken cancellationToken = default);
    Task UpdateConnectionAsync(EmailProviderConnection connection, CancellationToken cancellationToken = default);
    Task<EmailProviderMessageLink> AddMessageLinkAsync(EmailProviderMessageLink link, CancellationToken cancellationToken = default);
    Task<EmailProviderMessageLink?> GetMessageLinkAsync(Guid connectionId, string providerMessageId, CancellationToken cancellationToken = default);
}
