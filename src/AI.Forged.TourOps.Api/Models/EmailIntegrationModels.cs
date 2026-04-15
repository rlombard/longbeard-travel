using System.Text.Json.Serialization;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Api.Models;

public sealed class CreateEmailProviderConnectionRequest
{
    public string ConnectionName { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailIntegrationProviderType ProviderType { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailIntegrationAuthMethod AuthMethod { get; set; }
    public string MailboxAddress { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool AllowSend { get; set; }
    public bool AllowSync { get; set; }
    public bool IsDefaultConnection { get; set; }
    public EmailConnectionSettingsRequest Settings { get; set; } = new();
    public EmailConnectionSecretsRequest Secrets { get; set; } = new();
}

public sealed class StartEmailProviderOAuthRequest
{
    public string ConnectionName { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailIntegrationProviderType ProviderType { get; set; }
    public string MailboxAddress { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool AllowSend { get; set; }
    public bool AllowSync { get; set; }
    public bool IsDefaultConnection { get; set; }
    public string? ReturnUrl { get; set; }
    public EmailConnectionSettingsRequest Settings { get; set; } = new();
}

public sealed class EmailConnectionSettingsRequest
{
    public string? IncomingHost { get; set; }
    public int? IncomingPort { get; set; }
    public bool? IncomingUseSsl { get; set; }
    public string? IncomingUsername { get; set; }
    public string? IncomingFolder { get; set; }
    public string? OutgoingHost { get; set; }
    public int? OutgoingPort { get; set; }
    public bool? OutgoingUseSsl { get; set; }
    public string? OutgoingUsername { get; set; }
    public string? FromAddressOverride { get; set; }
    public string? ReplyToAddress { get; set; }
    public string? MicrosoftTenantId { get; set; }
    public string? SendGridFromAddress { get; set; }
    public int? SyncIntervalMinutes { get; set; }
}

public sealed class EmailConnectionSecretsRequest
{
    public string? IncomingPassword { get; set; }
    public string? OutgoingPassword { get; set; }
    public string? ApiKey { get; set; }
}

public sealed class SendConnectedEmailMessageRequest
{
    public string Subject { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public string? BodyHtml { get; set; }
    public List<string> ToAddresses { get; set; } = [];
    public List<string> CcAddresses { get; set; } = [];
    public List<string> BccAddresses { get; set; } = [];
    public string? ReplyToAddress { get; set; }
    public List<EmailAttachmentRequest> Attachments { get; set; } = [];
}

public sealed class EmailAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public string ContentBase64 { get; set; } = string.Empty;
}

public class EmailProviderConnectionListItemResponse
{
    public Guid Id { get; set; }
    public string ConnectionName { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailIntegrationProviderType ProviderType { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailIntegrationAuthMethod AuthMethod { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailIntegrationStatus Status { get; set; }
    public string MailboxAddress { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool AllowSend { get; set; }
    public bool AllowSync { get; set; }
    public bool IsDefaultConnection { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public DateTime? LastSuccessfulSendAt { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class EmailProviderConnectionResponse : EmailProviderConnectionListItemResponse
{
    public EmailConnectionCapabilitiesResponse Capabilities { get; set; } = new();
    public EmailConnectionSettingsResponse Settings { get; set; } = new();
    public EmailConnectionSyncStateResponse SyncState { get; set; } = new();
    public EmailConnectionWebhookStateResponse WebhookState { get; set; } = new();
}

public sealed class EmailConnectionCapabilitiesResponse
{
    public bool SupportsOAuth { get; set; }
    public bool SupportsSend { get; set; }
    public bool SupportsSync { get; set; }
    public bool SupportsWebhook { get; set; }
}

public sealed class EmailConnectionSettingsResponse
{
    public string? IncomingHost { get; set; }
    public int? IncomingPort { get; set; }
    public bool? IncomingUseSsl { get; set; }
    public string? IncomingUsername { get; set; }
    public string? IncomingFolder { get; set; }
    public string? OutgoingHost { get; set; }
    public int? OutgoingPort { get; set; }
    public bool? OutgoingUseSsl { get; set; }
    public string? OutgoingUsername { get; set; }
    public string? FromAddressOverride { get; set; }
    public string? ReplyToAddress { get; set; }
    public string? MicrosoftTenantId { get; set; }
    public string? SendGridFromAddress { get; set; }
    public int? SyncIntervalMinutes { get; set; }
}

public sealed class EmailConnectionSyncStateResponse
{
    public string? CursorPreview { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public DateTime? NextSyncAt { get; set; }
}

public sealed class EmailConnectionWebhookStateResponse
{
    public string? SubscriptionId { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }
}

public sealed class EmailOAuthStartResponse
{
    public Guid ConnectionId { get; set; }
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}

public sealed class EmailOAuthCallbackResponse
{
    public Guid ConnectionId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailIntegrationStatus Status { get; set; }
    public string MailboxAddress { get; set; } = string.Empty;
    public string? RedirectUrl { get; set; }
}

public sealed class EmailConnectionTestResponse
{
    public Guid ConnectionId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime TestedAt { get; set; }
}

public sealed class EmailSyncResponse
{
    public Guid ConnectionId { get; set; }
    public int MessagesProcessed { get; set; }
    public int MessagesImported { get; set; }
    public int MessagesSkipped { get; set; }
    public DateTime SyncedAt { get; set; }
    public string? CursorPreview { get; set; }
}

public sealed class EmailSendResponse
{
    public Guid ConnectionId { get; set; }
    public string ProviderMessageId { get; set; } = string.Empty;
    public string? ProviderThreadId { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string? MetadataJson { get; set; }
}

public sealed class DisconnectEmailConnectionResponse
{
    public Guid ConnectionId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailIntegrationStatus Status { get; set; }
}

public sealed class EmailWebhookResponse
{
    public bool Handled { get; set; }
    public List<Guid> ConnectionIdsMarkedForSync { get; set; } = [];
}
