using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class EmailProviderConnection
{
    public Guid Id { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public string ConnectionName { get; set; } = string.Empty;
    public EmailIntegrationProviderType ProviderType { get; set; }
    public EmailIntegrationAuthMethod AuthMethod { get; set; }
    public EmailIntegrationStatus Status { get; set; }
    public string MailboxAddress { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? ExternalAccountId { get; set; }
    public bool AllowSend { get; set; }
    public bool AllowSync { get; set; }
    public bool IsDefaultConnection { get; set; }
    public string? ConnectionSettingsJson { get; set; }
    public string? EncryptedCredentialsJson { get; set; }
    public DateTime? AccessTokenExpiresAt { get; set; }
    public string? OAuthState { get; set; }
    public DateTime? OAuthStateExpiresAt { get; set; }
    public string? OAuthReturnUrl { get; set; }
    public string? SyncCursorJson { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public DateTime? NextSyncAt { get; set; }
    public DateTime? LastSuccessfulSendAt { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public string? LastError { get; set; }
    public string? WebhookSubscriptionId { get; set; }
    public DateTime? WebhookSubscriptionExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<EmailProviderMessageLink> MessageLinks { get; set; } = new List<EmailProviderMessageLink>();
}
