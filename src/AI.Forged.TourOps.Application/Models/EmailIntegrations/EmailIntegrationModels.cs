using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Models.EmailIntegrations;

public class EmailProviderConnectionListItemModel
{
    public Guid Id { get; init; }
    public string ConnectionName { get; init; } = string.Empty;
    public EmailIntegrationProviderType ProviderType { get; init; }
    public EmailIntegrationAuthMethod AuthMethod { get; init; }
    public EmailIntegrationStatus Status { get; init; }
    public string MailboxAddress { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public bool AllowSend { get; init; }
    public bool AllowSync { get; init; }
    public bool IsDefaultConnection { get; init; }
    public DateTime? LastSyncedAt { get; init; }
    public DateTime? LastSuccessfulSendAt { get; init; }
    public DateTime? LastTestedAt { get; init; }
    public string? LastError { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class EmailProviderConnectionModel : EmailProviderConnectionListItemModel
{
    public EmailConnectionCapabilitiesModel Capabilities { get; init; } = new();
    public EmailConnectionSettingsModel Settings { get; init; } = new();
    public EmailConnectionSyncStateModel SyncState { get; init; } = new();
    public EmailConnectionWebhookStateModel WebhookState { get; init; } = new();
}

public sealed class EmailConnectionCapabilitiesModel
{
    public bool SupportsOAuth { get; init; }
    public bool SupportsSend { get; init; }
    public bool SupportsSync { get; init; }
    public bool SupportsWebhook { get; init; }
}

public sealed class EmailConnectionSettingsModel
{
    public string? IncomingHost { get; init; }
    public int? IncomingPort { get; init; }
    public bool? IncomingUseSsl { get; init; }
    public string? IncomingUsername { get; init; }
    public string? IncomingFolder { get; init; }
    public string? OutgoingHost { get; init; }
    public int? OutgoingPort { get; init; }
    public bool? OutgoingUseSsl { get; init; }
    public string? OutgoingUsername { get; init; }
    public string? FromAddressOverride { get; init; }
    public string? ReplyToAddress { get; init; }
    public string? MicrosoftTenantId { get; init; }
    public string? SendGridFromAddress { get; init; }
    public int? SyncIntervalMinutes { get; init; }
    public string? SyncMode { get; init; }
}

public sealed class EmailConnectionSyncStateModel
{
    public string? CursorPreview { get; init; }
    public DateTime? LastSyncedAt { get; init; }
    public DateTime? NextSyncAt { get; init; }
}

public sealed class EmailConnectionWebhookStateModel
{
    public string? SubscriptionId { get; init; }
    public DateTime? SubscriptionExpiresAt { get; init; }
}

public sealed class CreateEmailProviderConnectionModel
{
    public string ConnectionName { get; init; } = string.Empty;
    public EmailIntegrationProviderType ProviderType { get; init; }
    public EmailIntegrationAuthMethod AuthMethod { get; init; }
    public string MailboxAddress { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public bool AllowSend { get; init; }
    public bool AllowSync { get; init; }
    public bool IsDefaultConnection { get; init; }
    public EmailConnectionSettingsInputModel Settings { get; init; } = new();
    public EmailConnectionSecretInputModel Secrets { get; init; } = new();
}

public sealed class StartEmailProviderOAuthModel
{
    public string ConnectionName { get; init; } = string.Empty;
    public EmailIntegrationProviderType ProviderType { get; init; }
    public string MailboxAddress { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public bool AllowSend { get; init; }
    public bool AllowSync { get; init; }
    public bool IsDefaultConnection { get; init; }
    public string? ReturnUrl { get; init; }
    public EmailConnectionSettingsInputModel Settings { get; init; } = new();
}

public sealed class EmailConnectionSettingsInputModel
{
    public string? IncomingHost { get; init; }
    public int? IncomingPort { get; init; }
    public bool? IncomingUseSsl { get; init; }
    public string? IncomingUsername { get; init; }
    public string? IncomingFolder { get; init; }
    public string? OutgoingHost { get; init; }
    public int? OutgoingPort { get; init; }
    public bool? OutgoingUseSsl { get; init; }
    public string? OutgoingUsername { get; init; }
    public string? FromAddressOverride { get; init; }
    public string? ReplyToAddress { get; init; }
    public string? MicrosoftTenantId { get; init; }
    public string? SendGridFromAddress { get; init; }
    public int? SyncIntervalMinutes { get; init; }
}

public sealed class EmailConnectionSecretInputModel
{
    public string? IncomingPassword { get; init; }
    public string? OutgoingPassword { get; init; }
    public string? ApiKey { get; init; }
}

public sealed class EmailOAuthStartResultModel
{
    public Guid ConnectionId { get; init; }
    public string AuthorizationUrl { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
}

public sealed class CompleteEmailOAuthCallbackModel
{
    public EmailIntegrationProviderType ProviderType { get; init; }
    public string State { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Error { get; init; }
    public string? ErrorDescription { get; init; }
}

public sealed class EmailOAuthCallbackResultModel
{
    public Guid ConnectionId { get; init; }
    public string? RedirectUrl { get; init; }
    public EmailIntegrationStatus Status { get; init; }
    public string MailboxAddress { get; init; } = string.Empty;
}

public sealed class SendConnectedEmailMessageModel
{
    public string Subject { get; init; } = string.Empty;
    public string? BodyText { get; init; }
    public string? BodyHtml { get; init; }
    public IReadOnlyList<string> ToAddresses { get; init; } = [];
    public IReadOnlyList<string> CcAddresses { get; init; } = [];
    public IReadOnlyList<string> BccAddresses { get; init; } = [];
    public string? ReplyToAddress { get; init; }
    public IReadOnlyList<EmailAttachmentInputModel> Attachments { get; init; } = [];
}

public sealed class EmailAttachmentInputModel
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/octet-stream";
    public string ContentBase64 { get; init; } = string.Empty;
}

public sealed class EmailSendResultModel
{
    public Guid ConnectionId { get; init; }
    public string ProviderMessageId { get; init; } = string.Empty;
    public string? ProviderThreadId { get; init; }
    public string FromAddress { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
    public string? MetadataJson { get; init; }
}

public sealed class EmailSyncResultModel
{
    public Guid ConnectionId { get; init; }
    public int MessagesProcessed { get; init; }
    public int MessagesImported { get; init; }
    public int MessagesSkipped { get; init; }
    public DateTime SyncedAt { get; init; }
    public string? CursorPreview { get; init; }
}

public sealed class EmailConnectionTestResultModel
{
    public Guid ConnectionId { get; init; }
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime TestedAt { get; init; }
}

public sealed class DisconnectEmailConnectionResultModel
{
    public Guid ConnectionId { get; init; }
    public EmailIntegrationStatus Status { get; init; }
}

public sealed class EmailWebhookProcessingResultModel
{
    public bool Handled { get; init; }
    public string? ValidationResponse { get; init; }
    public IReadOnlyList<Guid> ConnectionIdsMarkedForSync { get; init; } = [];
}

public sealed class EmailConnectionSecretEnvelope
{
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public string? IncomingPassword { get; init; }
    public string? OutgoingPassword { get; init; }
    public string? ApiKey { get; init; }
    public string? TokenType { get; init; }
    public string? Scope { get; init; }
}

public sealed class EmailConnectionResolvedSecrets
{
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public string? IncomingPassword { get; init; }
    public string? OutgoingPassword { get; init; }
    public string? ApiKey { get; init; }
    public string? TokenType { get; init; }
    public string? Scope { get; init; }
}

public sealed class EmailSyncEnvelope
{
    public IReadOnlyList<EmailSyncedMessageModel> Messages { get; init; } = [];
    public string? NextCursorJson { get; init; }
    public EmailConnectionResolvedSecrets? UpdatedSecrets { get; init; }
    public DateTime? AccessTokenExpiresAt { get; init; }
}

public sealed class EmailSyncedMessageModel
{
    public string ProviderMessageId { get; init; } = string.Empty;
    public string? ProviderThreadId { get; init; }
    public string FolderName { get; init; } = "Inbox";
    public string Subject { get; init; } = string.Empty;
    public string BodyText { get; init; } = string.Empty;
    public string? BodyHtml { get; init; }
    public string Sender { get; init; } = string.Empty;
    public string Recipients { get; init; } = string.Empty;
    public DateTime SentAtUtc { get; init; }
}

public sealed class EmailProviderSendRequest
{
    public string FromAddress { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string? BodyText { get; init; }
    public string? BodyHtml { get; init; }
    public IReadOnlyList<string> ToAddresses { get; init; } = [];
    public IReadOnlyList<string> CcAddresses { get; init; } = [];
    public IReadOnlyList<string> BccAddresses { get; init; } = [];
    public string? ReplyToAddress { get; init; }
    public IReadOnlyList<EmailAttachmentInputModel> Attachments { get; init; } = [];
}

public sealed class EmailProviderSendResult
{
    public string ProviderMessageId { get; init; } = string.Empty;
    public string? ProviderThreadId { get; init; }
    public string? MetadataJson { get; init; }
    public EmailConnectionResolvedSecrets? UpdatedSecrets { get; init; }
    public DateTime? AccessTokenExpiresAt { get; init; }
}

public sealed class EmailAuthorizationCompletionResult
{
    public string MailboxAddress { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? ExternalAccountId { get; init; }
    public EmailConnectionResolvedSecrets Secrets { get; init; } = new();
    public DateTime? AccessTokenExpiresAt { get; init; }
}

public sealed class EmailConnectionResolvedSettings
{
    public string? IncomingHost { get; init; }
    public int? IncomingPort { get; init; }
    public bool IncomingUseSsl { get; init; }
    public string? IncomingUsername { get; init; }
    public string IncomingFolder { get; init; } = "Inbox";
    public string? OutgoingHost { get; init; }
    public int? OutgoingPort { get; init; }
    public bool OutgoingUseSsl { get; init; }
    public string? OutgoingUsername { get; init; }
    public string? FromAddressOverride { get; init; }
    public string? ReplyToAddress { get; init; }
    public string? MicrosoftTenantId { get; init; }
    public string? SendGridFromAddress { get; init; }
    public int SyncIntervalMinutes { get; init; } = 15;
}
