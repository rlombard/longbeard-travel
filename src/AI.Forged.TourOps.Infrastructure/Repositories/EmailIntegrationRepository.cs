using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public sealed class EmailIntegrationRepository(AppDbContext dbContext) : IEmailIntegrationRepository
{
    public async Task<EmailProviderConnection> AddConnectionAsync(EmailProviderConnection connection, CancellationToken cancellationToken = default)
    {
        dbContext.EmailProviderConnections.Add(connection);
        await dbContext.SaveChangesAsync(cancellationToken);
        return connection;
    }

    public async Task<EmailProviderConnection?> GetConnectionByIdAsync(Guid connectionId, CancellationToken cancellationToken = default) =>
        await dbContext.EmailProviderConnections
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == connectionId, cancellationToken);

    public async Task<IReadOnlyList<EmailProviderConnection>> GetConnectionsByOwnerAsync(string ownerUserId, CancellationToken cancellationToken = default) =>
        await dbContext.EmailProviderConnections
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EmailProviderConnection>> GetDueForSyncAsync(DateTime utcNow, CancellationToken cancellationToken = default) =>
        await dbContext.EmailProviderConnections
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.AllowSync
                && x.Status == EmailIntegrationStatus.Active
                && x.NextSyncAt.HasValue
                && x.NextSyncAt <= utcNow)
            .OrderBy(x => x.NextSyncAt)
            .Take(25)
            .ToListAsync(cancellationToken);

    public async Task<EmailProviderConnection?> GetByOAuthStateAsync(string oauthState, CancellationToken cancellationToken = default) =>
        await dbContext.EmailProviderConnections
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.OAuthState == oauthState, cancellationToken);

    public async Task<EmailProviderConnection?> GetDefaultSendConnectionAsync(string ownerUserId, CancellationToken cancellationToken = default) =>
        await dbContext.EmailProviderConnections
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId && x.AllowSend && x.Status == EmailIntegrationStatus.Active)
            .OrderByDescending(x => x.IsDefaultConnection)
            .ThenByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<EmailProviderConnection>> GetByWebhookSubscriptionIdsAsync(IEnumerable<string> subscriptionIds, CancellationToken cancellationToken = default)
    {
        var ids = subscriptionIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        return await dbContext.EmailProviderConnections
            .IgnoreQueryFilters()
            .Where(x => x.WebhookSubscriptionId != null && ids.Contains(x.WebhookSubscriptionId))
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateConnectionAsync(EmailProviderConnection connection, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.EmailProviderConnections.FirstOrDefaultAsync(x => x.Id == connection.Id, cancellationToken)
            ?? throw new InvalidOperationException("Email provider connection not found.");

        existing.ConnectionName = connection.ConnectionName;
        existing.ProviderType = connection.ProviderType;
        existing.AuthMethod = connection.AuthMethod;
        existing.Status = connection.Status;
        existing.MailboxAddress = connection.MailboxAddress;
        existing.DisplayName = connection.DisplayName;
        existing.ExternalAccountId = connection.ExternalAccountId;
        existing.AllowSend = connection.AllowSend;
        existing.AllowSync = connection.AllowSync;
        existing.IsDefaultConnection = connection.IsDefaultConnection;
        existing.ConnectionSettingsJson = connection.ConnectionSettingsJson;
        existing.EncryptedCredentialsJson = connection.EncryptedCredentialsJson;
        existing.AccessTokenExpiresAt = connection.AccessTokenExpiresAt;
        existing.OAuthState = connection.OAuthState;
        existing.OAuthStateExpiresAt = connection.OAuthStateExpiresAt;
        existing.OAuthReturnUrl = connection.OAuthReturnUrl;
        existing.SyncCursorJson = connection.SyncCursorJson;
        existing.LastSyncedAt = connection.LastSyncedAt;
        existing.NextSyncAt = connection.NextSyncAt;
        existing.LastSuccessfulSendAt = connection.LastSuccessfulSendAt;
        existing.LastTestedAt = connection.LastTestedAt;
        existing.LastError = connection.LastError;
        existing.WebhookSubscriptionId = connection.WebhookSubscriptionId;
        existing.WebhookSubscriptionExpiresAt = connection.WebhookSubscriptionExpiresAt;
        existing.UpdatedAt = connection.UpdatedAt;

        if (connection.IsDefaultConnection)
        {
            var siblings = await dbContext.EmailProviderConnections
                .Where(x => x.OwnerUserId == connection.OwnerUserId && x.Id != connection.Id && x.IsDefaultConnection)
                .ToListAsync(cancellationToken);

            foreach (var sibling in siblings)
            {
                sibling.IsDefaultConnection = false;
                sibling.UpdatedAt = connection.UpdatedAt;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmailProviderMessageLink> AddMessageLinkAsync(EmailProviderMessageLink link, CancellationToken cancellationToken = default)
    {
        dbContext.EmailProviderMessageLinks.Add(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        return link;
    }

    public async Task<EmailProviderMessageLink?> GetMessageLinkAsync(Guid connectionId, string providerMessageId, CancellationToken cancellationToken = default) =>
        await dbContext.EmailProviderMessageLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmailProviderConnectionId == connectionId && x.ProviderMessageId == providerMessageId, cancellationToken);
}
