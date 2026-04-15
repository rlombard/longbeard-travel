using AI.Forged.TourOps.Application.Models.EmailIntegrations;

namespace AI.Forged.TourOps.Api.Models;

public static class EmailIntegrationMappings
{
    public static CreateEmailProviderConnectionModel ToModel(this CreateEmailProviderConnectionRequest request) => new()
    {
        ConnectionName = request.ConnectionName,
        ProviderType = request.ProviderType,
        AuthMethod = request.AuthMethod,
        MailboxAddress = request.MailboxAddress,
        DisplayName = request.DisplayName,
        AllowSend = request.AllowSend,
        AllowSync = request.AllowSync,
        IsDefaultConnection = request.IsDefaultConnection,
        Settings = request.Settings.ToModel(),
        Secrets = request.Secrets.ToModel()
    };

    public static StartEmailProviderOAuthModel ToModel(this StartEmailProviderOAuthRequest request) => new()
    {
        ConnectionName = request.ConnectionName,
        ProviderType = request.ProviderType,
        MailboxAddress = request.MailboxAddress,
        DisplayName = request.DisplayName,
        AllowSend = request.AllowSend,
        AllowSync = request.AllowSync,
        IsDefaultConnection = request.IsDefaultConnection,
        ReturnUrl = request.ReturnUrl,
        Settings = request.Settings.ToModel()
    };

    public static EmailConnectionSettingsInputModel ToModel(this EmailConnectionSettingsRequest request) => new()
    {
        IncomingHost = request.IncomingHost,
        IncomingPort = request.IncomingPort,
        IncomingUseSsl = request.IncomingUseSsl,
        IncomingUsername = request.IncomingUsername,
        IncomingFolder = request.IncomingFolder,
        OutgoingHost = request.OutgoingHost,
        OutgoingPort = request.OutgoingPort,
        OutgoingUseSsl = request.OutgoingUseSsl,
        OutgoingUsername = request.OutgoingUsername,
        FromAddressOverride = request.FromAddressOverride,
        ReplyToAddress = request.ReplyToAddress,
        MicrosoftTenantId = request.MicrosoftTenantId,
        SendGridFromAddress = request.SendGridFromAddress,
        SyncIntervalMinutes = request.SyncIntervalMinutes
    };

    public static EmailConnectionSecretInputModel ToModel(this EmailConnectionSecretsRequest request) => new()
    {
        IncomingPassword = request.IncomingPassword,
        OutgoingPassword = request.OutgoingPassword,
        ApiKey = request.ApiKey
    };

    public static SendConnectedEmailMessageModel ToModel(this SendConnectedEmailMessageRequest request) => new()
    {
        Subject = request.Subject,
        BodyText = request.BodyText,
        BodyHtml = request.BodyHtml,
        ToAddresses = request.ToAddresses,
        CcAddresses = request.CcAddresses,
        BccAddresses = request.BccAddresses,
        ReplyToAddress = request.ReplyToAddress,
        Attachments = request.Attachments.Select(x => new EmailAttachmentInputModel
        {
            FileName = x.FileName,
            ContentType = x.ContentType,
            ContentBase64 = x.ContentBase64
        }).ToList()
    };

    public static EmailProviderConnectionListItemResponse ToResponse(this EmailProviderConnectionListItemModel model) => new()
    {
        Id = model.Id,
        ConnectionName = model.ConnectionName,
        ProviderType = model.ProviderType,
        AuthMethod = model.AuthMethod,
        Status = model.Status,
        MailboxAddress = model.MailboxAddress,
        DisplayName = model.DisplayName,
        AllowSend = model.AllowSend,
        AllowSync = model.AllowSync,
        IsDefaultConnection = model.IsDefaultConnection,
        LastSyncedAt = model.LastSyncedAt,
        LastSuccessfulSendAt = model.LastSuccessfulSendAt,
        LastTestedAt = model.LastTestedAt,
        LastError = model.LastError,
        CreatedAt = model.CreatedAt,
        UpdatedAt = model.UpdatedAt
    };

    public static EmailProviderConnectionResponse ToResponse(this EmailProviderConnectionModel model) => new()
    {
        Id = model.Id,
        ConnectionName = model.ConnectionName,
        ProviderType = model.ProviderType,
        AuthMethod = model.AuthMethod,
        Status = model.Status,
        MailboxAddress = model.MailboxAddress,
        DisplayName = model.DisplayName,
        AllowSend = model.AllowSend,
        AllowSync = model.AllowSync,
        IsDefaultConnection = model.IsDefaultConnection,
        LastSyncedAt = model.LastSyncedAt,
        LastSuccessfulSendAt = model.LastSuccessfulSendAt,
        LastTestedAt = model.LastTestedAt,
        LastError = model.LastError,
        CreatedAt = model.CreatedAt,
        UpdatedAt = model.UpdatedAt,
        Capabilities = new EmailConnectionCapabilitiesResponse
        {
            SupportsOAuth = model.Capabilities.SupportsOAuth,
            SupportsSend = model.Capabilities.SupportsSend,
            SupportsSync = model.Capabilities.SupportsSync,
            SupportsWebhook = model.Capabilities.SupportsWebhook
        },
        Settings = new EmailConnectionSettingsResponse
        {
            IncomingHost = model.Settings.IncomingHost,
            IncomingPort = model.Settings.IncomingPort,
            IncomingUseSsl = model.Settings.IncomingUseSsl,
            IncomingUsername = model.Settings.IncomingUsername,
            IncomingFolder = model.Settings.IncomingFolder,
            OutgoingHost = model.Settings.OutgoingHost,
            OutgoingPort = model.Settings.OutgoingPort,
            OutgoingUseSsl = model.Settings.OutgoingUseSsl,
            OutgoingUsername = model.Settings.OutgoingUsername,
            FromAddressOverride = model.Settings.FromAddressOverride,
            ReplyToAddress = model.Settings.ReplyToAddress,
            MicrosoftTenantId = model.Settings.MicrosoftTenantId,
            SendGridFromAddress = model.Settings.SendGridFromAddress,
            SyncIntervalMinutes = model.Settings.SyncIntervalMinutes
        },
        SyncState = new EmailConnectionSyncStateResponse
        {
            CursorPreview = model.SyncState.CursorPreview,
            LastSyncedAt = model.SyncState.LastSyncedAt,
            NextSyncAt = model.SyncState.NextSyncAt
        },
        WebhookState = new EmailConnectionWebhookStateResponse
        {
            SubscriptionId = model.WebhookState.SubscriptionId,
            SubscriptionExpiresAt = model.WebhookState.SubscriptionExpiresAt
        }
    };

    public static EmailOAuthStartResponse ToResponse(this EmailOAuthStartResultModel model) => new()
    {
        ConnectionId = model.ConnectionId,
        AuthorizationUrl = model.AuthorizationUrl,
        State = model.State
    };

    public static EmailOAuthCallbackResponse ToResponse(this EmailOAuthCallbackResultModel model) => new()
    {
        ConnectionId = model.ConnectionId,
        Status = model.Status,
        MailboxAddress = model.MailboxAddress,
        RedirectUrl = model.RedirectUrl
    };

    public static EmailConnectionTestResponse ToResponse(this EmailConnectionTestResultModel model) => new()
    {
        ConnectionId = model.ConnectionId,
        Success = model.Success,
        Message = model.Message,
        TestedAt = model.TestedAt
    };

    public static EmailSyncResponse ToResponse(this EmailSyncResultModel model) => new()
    {
        ConnectionId = model.ConnectionId,
        MessagesProcessed = model.MessagesProcessed,
        MessagesImported = model.MessagesImported,
        MessagesSkipped = model.MessagesSkipped,
        SyncedAt = model.SyncedAt,
        CursorPreview = model.CursorPreview
    };

    public static EmailSendResponse ToResponse(this EmailSendResultModel model) => new()
    {
        ConnectionId = model.ConnectionId,
        ProviderMessageId = model.ProviderMessageId,
        ProviderThreadId = model.ProviderThreadId,
        FromAddress = model.FromAddress,
        SentAt = model.SentAt,
        MetadataJson = model.MetadataJson
    };

    public static DisconnectEmailConnectionResponse ToResponse(this DisconnectEmailConnectionResultModel model) => new()
    {
        ConnectionId = model.ConnectionId,
        Status = model.Status
    };

    public static EmailWebhookResponse ToResponse(this EmailWebhookProcessingResultModel model) => new()
    {
        Handled = model.Handled,
        ConnectionIdsMarkedForSync = model.ConnectionIdsMarkedForSync.ToList()
    };
}
