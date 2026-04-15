using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Models.EmailIntegrations;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services.Email;

public sealed class EmailIntegrationService(
    IEmailIntegrationRepository emailIntegrationRepository,
    IEmailRepository emailRepository,
    ICurrentUserContext currentUserContext,
    IEmailConnectionSecretProtector secretProtector,
    IEmailIntegrationProviderResolver providerResolver,
    ILicensePolicyService? licensePolicyService = null,
    IUsageMeteringService? usageMeteringService = null,
    IAuditService? auditService = null,
    ITenantExecutionContextAccessor? tenantExecutionContextAccessor = null) : IEmailIntegrationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<EmailProviderConnectionListItemModel>> GetConnectionsAsync(CancellationToken cancellationToken = default)
    {
        var ownerUserId = currentUserContext.GetRequiredUserId();
        var connections = await emailIntegrationRepository.GetConnectionsByOwnerAsync(ownerUserId, cancellationToken);
        return connections.Select(ToListModel).ToList();
    }

    public async Task<EmailProviderConnectionModel?> GetConnectionAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        var connection = await GetOwnedConnectionAsync(connectionId, cancellationToken, throwIfMissing: false);
        return connection is null ? null : ToModel(connection);
    }

    public async Task<EmailProviderConnectionModel> CreateConnectionAsync(CreateEmailProviderConnectionModel model, CancellationToken cancellationToken = default)
    {
        if (licensePolicyService is not null)
        {
            await licensePolicyService.AssertAllowedAsync("email.integrations.manage", cancellationToken);
        }

        ValidateManualCreateModel(model);

        var connection = new EmailProviderConnection
        {
            Id = Guid.NewGuid(),
            OwnerUserId = currentUserContext.GetRequiredUserId(),
            ConnectionName = NormalizeRequired(model.ConnectionName, "Connection name is required.", 200),
            ProviderType = model.ProviderType,
            AuthMethod = model.AuthMethod,
            Status = EmailIntegrationStatus.Active,
            MailboxAddress = NormalizeRequired(model.MailboxAddress, "Mailbox address is required.", 256),
            DisplayName = NormalizeOptional(model.DisplayName, 256),
            AllowSend = model.AllowSend,
            AllowSync = model.AllowSync,
            IsDefaultConnection = model.IsDefaultConnection,
            ConnectionSettingsJson = Serialize(ToResolvedSettings(model.Settings)),
            EncryptedCredentialsJson = ProtectSecrets(new EmailConnectionSecretEnvelope
            {
                IncomingPassword = NormalizeSecret(model.Secrets.IncomingPassword),
                OutgoingPassword = NormalizeSecret(model.Secrets.OutgoingPassword),
                ApiKey = NormalizeSecret(model.Secrets.ApiKey)
            }),
            NextSyncAt = model.AllowSync ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await emailIntegrationRepository.AddConnectionAsync(connection, cancellationToken);
        if (auditService is not null)
        {
            await auditService.WriteAsync(new AI.Forged.TourOps.Application.Models.Platform.AuditEventCreateModel
            {
                ScopeType = "EmailIntegration",
                Action = "EmailConnectionCreated",
                Result = "Success",
                TargetEntityType = nameof(EmailProviderConnection),
                TargetEntityId = connection.Id
            }, cancellationToken);
        }

        return ToModel(connection);
    }

    public async Task<EmailOAuthStartResultModel> StartOAuthAsync(StartEmailProviderOAuthModel model, CancellationToken cancellationToken = default)
    {
        if (licensePolicyService is not null)
        {
            await licensePolicyService.AssertAllowedAsync("email.integrations.manage", cancellationToken);
        }

        ValidateOAuthStartModel(model);

        var ownerUserId = currentUserContext.GetRequiredUserId();
        var state = Guid.NewGuid().ToString("N");
        var connection = new EmailProviderConnection
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            ConnectionName = NormalizeRequired(model.ConnectionName, "Connection name is required.", 200),
            ProviderType = model.ProviderType,
            AuthMethod = EmailIntegrationAuthMethod.OAuth2,
            Status = EmailIntegrationStatus.PendingAuthorization,
            MailboxAddress = NormalizeRequired(model.MailboxAddress, "Mailbox address is required.", 256),
            DisplayName = NormalizeOptional(model.DisplayName, 256),
            AllowSend = model.AllowSend,
            AllowSync = model.AllowSync,
            IsDefaultConnection = model.IsDefaultConnection,
            ConnectionSettingsJson = Serialize(ToResolvedSettings(model.Settings)),
            OAuthState = state,
            OAuthStateExpiresAt = DateTime.UtcNow.AddMinutes(20),
            OAuthReturnUrl = NormalizeOptional(model.ReturnUrl, 2000),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await emailIntegrationRepository.AddConnectionAsync(connection, cancellationToken);
        if (auditService is not null)
        {
            await auditService.WriteAsync(new AI.Forged.TourOps.Application.Models.Platform.AuditEventCreateModel
            {
                ScopeType = "EmailIntegration",
                Action = "EmailOAuthStarted",
                Result = "Pending",
                TargetEntityType = nameof(EmailProviderConnection),
                TargetEntityId = connection.Id
            }, cancellationToken);
        }

        var provider = GetOAuthProvider(model.ProviderType);
        return await provider.StartAuthorizationAsync(connection, ReadSettings(connection), cancellationToken);
    }

    public async Task<EmailOAuthCallbackResultModel> CompleteOAuthAsync(CompleteEmailOAuthCallbackModel model, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(model.Error))
        {
            throw new InvalidOperationException(model.ErrorDescription ?? model.Error);
        }

        var connection = await emailIntegrationRepository.GetByOAuthStateAsync(model.State, cancellationToken)
            ?? throw new InvalidOperationException("OAuth state could not be resolved.");
        using var tenantScope = tenantExecutionContextAccessor is not null && connection.TenantId != Guid.Empty
            ? tenantExecutionContextAccessor.BeginTenantScope(connection.TenantId)
            : null;

        if (connection.ProviderType != model.ProviderType)
        {
            throw new InvalidOperationException("OAuth provider type does not match the pending connection.");
        }

        if (!connection.OAuthStateExpiresAt.HasValue || connection.OAuthStateExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("OAuth state has expired.");
        }

        var provider = GetOAuthProvider(connection.ProviderType);
        var completion = await provider.CompleteAuthorizationAsync(
            connection,
            ReadSettings(connection),
            ReadSecrets(connection),
            model.Code,
            cancellationToken);

        connection.MailboxAddress = completion.MailboxAddress;
        connection.DisplayName = completion.DisplayName ?? connection.DisplayName;
        connection.ExternalAccountId = completion.ExternalAccountId;
        connection.EncryptedCredentialsJson = ProtectSecrets(ToEnvelope(completion.Secrets));
        connection.AccessTokenExpiresAt = completion.AccessTokenExpiresAt;
        connection.OAuthState = null;
        connection.OAuthStateExpiresAt = null;
        connection.Status = EmailIntegrationStatus.Active;
        connection.LastError = null;
        connection.NextSyncAt = connection.AllowSync ? DateTime.UtcNow : null;
        connection.UpdatedAt = DateTime.UtcNow;

        await emailIntegrationRepository.UpdateConnectionAsync(connection, cancellationToken);
        if (auditService is not null)
        {
            await auditService.WriteAsync(new AI.Forged.TourOps.Application.Models.Platform.AuditEventCreateModel
            {
                ScopeType = "EmailIntegration",
                Action = "EmailOAuthCompleted",
                Result = "Success",
                TargetEntityType = nameof(EmailProviderConnection),
                TargetEntityId = connection.Id
            }, cancellationToken);
        }

        return new EmailOAuthCallbackResultModel
        {
            ConnectionId = connection.Id,
            RedirectUrl = connection.OAuthReturnUrl,
            Status = connection.Status,
            MailboxAddress = connection.MailboxAddress
        };
    }

    public async Task<EmailConnectionTestResultModel> TestConnectionAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        var connection = await GetOwnedConnectionAsync(connectionId, cancellationToken) ?? throw new InvalidOperationException("Email provider connection not found.");
        await TestConnectionInternalAsync(connection, cancellationToken);

        return new EmailConnectionTestResultModel
        {
            ConnectionId = connection.Id,
            Success = connection.Status == EmailIntegrationStatus.Active,
            Message = connection.LastError is null ? "Connection test succeeded." : connection.LastError,
            TestedAt = connection.LastTestedAt ?? DateTime.UtcNow
        };
    }

    public async Task<EmailSyncResultModel> SyncConnectionAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        var connection = await GetOwnedConnectionAsync(connectionId, cancellationToken) ?? throw new InvalidOperationException("Email provider connection not found.");
        using var tenantScope = tenantExecutionContextAccessor is not null && connection.TenantId != Guid.Empty
            ? tenantExecutionContextAccessor.BeginTenantScope(connection.TenantId)
            : null;
        if (licensePolicyService is not null)
        {
            await licensePolicyService.AssertAllowedAsync("email.sync", cancellationToken);
        }

        var result = await SyncConnectionInternalAsync(connection, cancellationToken);
        if (usageMeteringService is not null)
        {
            await usageMeteringService.RecordAsync(new AI.Forged.TourOps.Application.Models.Platform.MeterUsageModel
            {
                Category = "Email",
                MetricKey = "email.sync.monthly",
                Quantity = 1,
                Unit = "sync",
                Source = "EmailIntegrationService",
                ReferenceEntityType = nameof(EmailProviderConnection),
                ReferenceEntityId = connection.Id
            }, cancellationToken);
        }

        return result;
    }

    public async Task<EmailSendResultModel> SendMessageAsync(Guid connectionId, SendConnectedEmailMessageModel model, CancellationToken cancellationToken = default)
    {
        var connection = await GetOwnedConnectionAsync(connectionId, cancellationToken) ?? throw new InvalidOperationException("Email provider connection not found.");
        using var tenantScope = tenantExecutionContextAccessor is not null && connection.TenantId != Guid.Empty
            ? tenantExecutionContextAccessor.BeginTenantScope(connection.TenantId)
            : null;
        if (licensePolicyService is not null)
        {
            await licensePolicyService.AssertAllowedAsync("email.send", cancellationToken);
        }

        var result = await SendThroughConnectionAsync(connection, model, cancellationToken);
        if (usageMeteringService is not null)
        {
            await usageMeteringService.RecordAsync(new AI.Forged.TourOps.Application.Models.Platform.MeterUsageModel
            {
                Category = "Email",
                MetricKey = "email.sends.monthly",
                Quantity = 1,
                Unit = "message",
                Source = "EmailIntegrationService",
                ReferenceEntityType = nameof(EmailProviderConnection),
                ReferenceEntityId = connection.Id
            }, cancellationToken);
        }

        return result;
    }

    public async Task<DisconnectEmailConnectionResultModel> DisconnectAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        var connection = await GetOwnedConnectionAsync(connectionId, cancellationToken) ?? throw new InvalidOperationException("Email provider connection not found.");

        connection.Status = EmailIntegrationStatus.Revoked;
        connection.EncryptedCredentialsJson = null;
        connection.OAuthState = null;
        connection.OAuthStateExpiresAt = null;
        connection.WebhookSubscriptionId = null;
        connection.WebhookSubscriptionExpiresAt = null;
        connection.NextSyncAt = null;
        connection.LastError = null;
        connection.UpdatedAt = DateTime.UtcNow;

        await emailIntegrationRepository.UpdateConnectionAsync(connection, cancellationToken);
        if (auditService is not null)
        {
            await auditService.WriteAsync(new AI.Forged.TourOps.Application.Models.Platform.AuditEventCreateModel
            {
                ScopeType = "EmailIntegration",
                Action = "EmailConnectionDisconnected",
                Result = "Success",
                TargetEntityType = nameof(EmailProviderConnection),
                TargetEntityId = connection.Id
            }, cancellationToken);
        }

        return new DisconnectEmailConnectionResultModel
        {
            ConnectionId = connection.Id,
            Status = connection.Status
        };
    }

    public async Task<EmailWebhookProcessingResultModel> HandleMicrosoftGraphWebhookAsync(string? validationToken, string payloadJson, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(validationToken))
        {
            return new EmailWebhookProcessingResultModel
            {
                Handled = true,
                ValidationResponse = validationToken
            };
        }

        using var document = JsonDocument.Parse(payloadJson);
        var subscriptionIds = document.RootElement.TryGetProperty("value", out var values) && values.ValueKind == JsonValueKind.Array
            ? values.EnumerateArray()
                .Select(x => x.TryGetProperty("subscriptionId", out var id) ? id.GetString() : null)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList()
            : [];

        var connections = await emailIntegrationRepository.GetByWebhookSubscriptionIdsAsync(subscriptionIds, cancellationToken);
        foreach (var connection in connections)
        {
            connection.NextSyncAt = DateTime.UtcNow;
            connection.UpdatedAt = DateTime.UtcNow;
            await emailIntegrationRepository.UpdateConnectionAsync(connection, cancellationToken);
        }

        return new EmailWebhookProcessingResultModel
        {
            Handled = connections.Count > 0,
            ConnectionIdsMarkedForSync = connections.Select(x => x.Id).ToList()
        };
    }

    public async Task<int> RunDueSyncAsync(CancellationToken cancellationToken = default)
    {
        var dueConnections = await emailIntegrationRepository.GetDueForSyncAsync(DateTime.UtcNow, cancellationToken);
        var processed = 0;

        foreach (var connection in dueConnections)
        {
            try
            {
                using var tenantScope = tenantExecutionContextAccessor is not null && connection.TenantId != Guid.Empty
                    ? tenantExecutionContextAccessor.BeginTenantScope(connection.TenantId)
                    : null;
                await SyncConnectionInternalAsync(connection, cancellationToken);
                processed++;
            }
            catch
            {
                processed++;
            }
        }

        return processed;
    }

    internal async Task<EmailSendResultModel> SendWithDefaultConnectionAsync(string ownerUserId, SendConnectedEmailMessageModel model, CancellationToken cancellationToken = default)
    {
        var connection = await emailIntegrationRepository.GetDefaultSendConnectionAsync(ownerUserId, cancellationToken);
        if (connection is null)
        {
            throw new InvalidOperationException("No active default email connection is configured.");
        }

        using var tenantScope = tenantExecutionContextAccessor is not null && connection.TenantId != Guid.Empty
            ? tenantExecutionContextAccessor.BeginTenantScope(connection.TenantId)
            : null;
        if (licensePolicyService is not null)
        {
            await licensePolicyService.AssertAllowedAsync("email.send", cancellationToken);
        }

        return await SendThroughConnectionAsync(connection, model, cancellationToken);
    }

    private async Task TestConnectionInternalAsync(EmailProviderConnection connection, CancellationToken cancellationToken)
    {
        var provider = providerResolver.GetRequiredProvider(connection.ProviderType);
        var settings = ReadSettings(connection);
        var secrets = ReadSecrets(connection);

        try
        {
            if (provider is IEmailSendProvider sendProvider && connection.AllowSend)
            {
                await sendProvider.TestSendCapabilityAsync(connection, settings, secrets, cancellationToken);
            }

            if (provider is IEmailSyncProvider syncProvider && connection.AllowSync)
            {
                await syncProvider.TestSyncCapabilityAsync(connection, settings, secrets, cancellationToken);
            }

            connection.Status = EmailIntegrationStatus.Active;
            connection.LastError = null;
        }
        catch (Exception ex)
        {
            connection.Status = EmailIntegrationStatus.Error;
            connection.LastError = ex.Message;
            throw;
        }
        finally
        {
            connection.LastTestedAt = DateTime.UtcNow;
            connection.UpdatedAt = DateTime.UtcNow;
            await emailIntegrationRepository.UpdateConnectionAsync(connection, cancellationToken);
        }
    }

    private async Task<EmailSyncResultModel> SyncConnectionInternalAsync(EmailProviderConnection connection, CancellationToken cancellationToken)
    {
        if (!connection.AllowSync)
        {
            throw new InvalidOperationException("Connection is not configured for sync.");
        }

        var provider = providerResolver.GetRequiredProvider(connection.ProviderType) as IEmailSyncProvider
            ?? throw new InvalidOperationException($"Provider '{connection.ProviderType}' does not support sync.");
        var settings = ReadSettings(connection);
        var secrets = ReadSecrets(connection);
        var now = DateTime.UtcNow;
        var processed = 0;
        var imported = 0;
        var skipped = 0;

        try
        {
            var syncEnvelope = await provider.SyncAsync(connection, settings, secrets, cancellationToken);
            processed = syncEnvelope.Messages.Count;

            foreach (var message in syncEnvelope.Messages)
            {
                var existingLink = await emailIntegrationRepository.GetMessageLinkAsync(connection.Id, message.ProviderMessageId, cancellationToken);
                if (existingLink is not null)
                {
                    skipped++;
                    continue;
                }

                var externalThreadKey = BuildExternalThreadKey(connection.Id, message.ProviderThreadId ?? message.ProviderMessageId);
                var thread = await emailRepository.GetThreadByExternalThreadIdAsync(externalThreadKey, cancellationToken);

                if (thread is null)
                {
                    thread = new EmailThread
                    {
                        Id = Guid.NewGuid(),
                        Subject = Truncate(message.Subject, 512),
                        SupplierEmail = Truncate(message.Sender, 256),
                        ExternalThreadId = externalThreadKey,
                        CreatedAt = now,
                        LastMessageAt = message.SentAtUtc
                    };

                    await emailRepository.AddThreadAsync(thread, cancellationToken);
                }

                var emailMessage = new EmailMessage
                {
                    Id = Guid.NewGuid(),
                    EmailThreadId = thread.Id,
                    Direction = EmailDirection.Inbound,
                    Subject = Truncate(message.Subject, 512),
                    BodyText = Truncate(message.BodyText, 16000),
                    BodyHtml = Truncate(message.BodyHtml, 32000),
                    Sender = Truncate(message.Sender, 256),
                    Recipients = Truncate(message.Recipients, 2000),
                    SentAt = message.SentAtUtc,
                    RequiresHumanReview = true,
                    CreatedAt = now
                };

                await emailRepository.AddMessageAsync(emailMessage, cancellationToken);
                thread.LastMessageAt = emailMessage.SentAt;
                await emailRepository.UpdateThreadAsync(thread, cancellationToken);

                await emailIntegrationRepository.AddMessageLinkAsync(new EmailProviderMessageLink
                {
                    Id = Guid.NewGuid(),
                    EmailProviderConnectionId = connection.Id,
                    ProviderMessageId = message.ProviderMessageId,
                    ProviderThreadId = message.ProviderThreadId,
                    EmailThreadId = thread.Id,
                    EmailMessageId = emailMessage.Id,
                    FolderName = message.FolderName,
                    ReceivedAt = message.SentAtUtc,
                    CreatedAt = now,
                    UpdatedAt = now
                }, cancellationToken);

                imported++;
            }

            ApplyConnectionRefresh(connection, syncEnvelope.UpdatedSecrets, syncEnvelope.AccessTokenExpiresAt);
            connection.SyncCursorJson = syncEnvelope.NextCursorJson;
            connection.LastSyncedAt = now;
            connection.NextSyncAt = now.AddMinutes(settings.SyncIntervalMinutes);
            connection.LastError = null;
            connection.Status = EmailIntegrationStatus.Active;
            connection.UpdatedAt = now;
            await emailIntegrationRepository.UpdateConnectionAsync(connection, cancellationToken);
        }
        catch (Exception ex)
        {
            connection.LastError = ex.Message;
            connection.Status = EmailIntegrationStatus.NeedsReconnect;
            connection.NextSyncAt = now.AddMinutes(settings.SyncIntervalMinutes);
            connection.UpdatedAt = now;
            await emailIntegrationRepository.UpdateConnectionAsync(connection, cancellationToken);
            throw;
        }

        return new EmailSyncResultModel
        {
            ConnectionId = connection.Id,
            MessagesProcessed = processed,
            MessagesImported = imported,
            MessagesSkipped = skipped,
            SyncedAt = now,
            CursorPreview = PreviewCursor(connection.SyncCursorJson)
        };
    }

    private async Task<EmailSendResultModel> SendThroughConnectionAsync(EmailProviderConnection connection, SendConnectedEmailMessageModel model, CancellationToken cancellationToken)
    {
        if (!connection.AllowSend)
        {
            throw new InvalidOperationException("Connection is not configured for sending.");
        }

        var provider = providerResolver.GetRequiredProvider(connection.ProviderType) as IEmailSendProvider
            ?? throw new InvalidOperationException($"Provider '{connection.ProviderType}' does not support sending.");
        var settings = ReadSettings(connection);
        var secrets = ReadSecrets(connection);

        ValidateSendModel(model);

        var sendResult = await provider.SendAsync(connection, settings, secrets, new EmailProviderSendRequest
        {
            FromAddress = settings.FromAddressOverride ?? connection.MailboxAddress,
            Subject = NormalizeRequired(model.Subject, "Email subject is required.", 512),
            BodyText = NormalizeOptional(model.BodyText, 16000),
            BodyHtml = NormalizeOptional(model.BodyHtml, 32000),
            ToAddresses = NormalizeAddresses(model.ToAddresses, "To"),
            CcAddresses = NormalizeAddresses(model.CcAddresses, "Cc"),
            BccAddresses = NormalizeAddresses(model.BccAddresses, "Bcc"),
            ReplyToAddress = NormalizeOptional(model.ReplyToAddress ?? settings.ReplyToAddress, 256),
            Attachments = model.Attachments
        }, cancellationToken);

        ApplyConnectionRefresh(connection, sendResult.UpdatedSecrets, sendResult.AccessTokenExpiresAt);
        connection.LastSuccessfulSendAt = DateTime.UtcNow;
        connection.LastError = null;
        connection.Status = EmailIntegrationStatus.Active;
        connection.UpdatedAt = DateTime.UtcNow;
        await emailIntegrationRepository.UpdateConnectionAsync(connection, cancellationToken);

        return new EmailSendResultModel
        {
            ConnectionId = connection.Id,
            ProviderMessageId = sendResult.ProviderMessageId,
            ProviderThreadId = sendResult.ProviderThreadId,
            FromAddress = settings.FromAddressOverride ?? connection.MailboxAddress,
            SentAt = connection.LastSuccessfulSendAt.Value,
            MetadataJson = sendResult.MetadataJson
        };
    }

    private async Task<EmailProviderConnection?> GetOwnedConnectionAsync(Guid connectionId, CancellationToken cancellationToken, bool throwIfMissing = true)
    {
        var ownerUserId = currentUserContext.GetRequiredUserId();
        var connection = await emailIntegrationRepository.GetConnectionByIdAsync(connectionId, cancellationToken);

        if (connection is null || !string.Equals(connection.OwnerUserId, ownerUserId, StringComparison.Ordinal))
        {
            if (throwIfMissing)
            {
                throw new InvalidOperationException("Email provider connection not found.");
            }

            return null;
        }

        return connection;
    }

    private static void ValidateSendModel(SendConnectedEmailMessageModel model)
    {
        if (model.ToAddresses.Count == 0)
        {
            throw new InvalidOperationException("At least one recipient is required.");
        }

        if (string.IsNullOrWhiteSpace(model.BodyText) && string.IsNullOrWhiteSpace(model.BodyHtml))
        {
            throw new InvalidOperationException("Email body is required.");
        }
    }

    private void ValidateManualCreateModel(CreateEmailProviderConnectionModel model)
    {
        if (model.AuthMethod == EmailIntegrationAuthMethod.OAuth2)
        {
            throw new InvalidOperationException("OAuth-based providers must use the OAuth start endpoint.");
        }

        ValidateBaseConnectionModel(model.ConnectionName, model.MailboxAddress);
        ValidateResolvedSettings(ToResolvedSettings(model.Settings), model.ProviderType, model.AllowSync, model.AllowSend);

        if (model.AuthMethod == EmailIntegrationAuthMethod.Password
            && string.IsNullOrWhiteSpace(model.Secrets.IncomingPassword)
            && string.IsNullOrWhiteSpace(model.Secrets.OutgoingPassword))
        {
            throw new InvalidOperationException("At least one mailbox password must be supplied.");
        }

        if (model.AuthMethod == EmailIntegrationAuthMethod.ApiKey && string.IsNullOrWhiteSpace(model.Secrets.ApiKey))
        {
            throw new InvalidOperationException("API key is required.");
        }
    }

    private void ValidateOAuthStartModel(StartEmailProviderOAuthModel model)
    {
        ValidateBaseConnectionModel(model.ConnectionName, model.MailboxAddress);
        ValidateResolvedSettings(ToResolvedSettings(model.Settings), model.ProviderType, model.AllowSync, model.AllowSend);

        var provider = providerResolver.GetRequiredProvider(model.ProviderType);
        if (!provider.SupportsOAuth)
        {
            throw new InvalidOperationException($"Provider '{model.ProviderType}' does not support OAuth connection flow.");
        }
    }

    private static void ValidateBaseConnectionModel(string connectionName, string mailboxAddress)
    {
        NormalizeRequired(connectionName, "Connection name is required.", 200);
        NormalizeRequired(mailboxAddress, "Mailbox address is required.", 256);
    }

    private static void ValidateResolvedSettings(EmailConnectionResolvedSettings settings, EmailIntegrationProviderType providerType, bool allowSync, bool allowSend)
    {
        if (allowSync && providerType is EmailIntegrationProviderType.Mailcow or EmailIntegrationProviderType.GenericImapSmtp)
        {
            if (string.IsNullOrWhiteSpace(settings.IncomingHost) || !settings.IncomingPort.HasValue || string.IsNullOrWhiteSpace(settings.IncomingUsername))
            {
                throw new InvalidOperationException("Incoming IMAP settings are required for sync-enabled IMAP connections.");
            }
        }

        if (allowSend && providerType is EmailIntegrationProviderType.Mailcow or EmailIntegrationProviderType.GenericImapSmtp or EmailIntegrationProviderType.SmtpDirect)
        {
            if (string.IsNullOrWhiteSpace(settings.OutgoingHost) || !settings.OutgoingPort.HasValue || string.IsNullOrWhiteSpace(settings.OutgoingUsername))
            {
                throw new InvalidOperationException("Outgoing SMTP settings are required for send-enabled SMTP connections.");
            }
        }
    }

    private static IReadOnlyList<string> NormalizeAddresses(IReadOnlyList<string> addresses, string label) =>
        addresses
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => NormalizeRequired(x, $"{label} address is required.", 256))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string NormalizeRequired(string? value, string message, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(message);
        }

        if (normalized.Length > maxLength)
        {
            throw new InvalidOperationException($"Value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized is { Length: > 0 } && normalized.Length > maxLength)
        {
            throw new InvalidOperationException($"Value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static string? NormalizeSecret(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string BuildExternalThreadKey(Guid connectionId, string providerThreadId) => $"{connectionId:N}:{providerThreadId}";

    private static string? PreviewCursor(string? cursorJson) =>
        string.IsNullOrWhiteSpace(cursorJson)
            ? null
            : cursorJson.Length <= 128
                ? cursorJson
                : cursorJson[..128];

    private static string Truncate(string? value, int maxLength)
    {
        var normalized = value ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string? Serialize<T>(T? value) where T : class =>
        value is null ? null : JsonSerializer.Serialize(value, JsonOptions);

    private static T Deserialize<T>(string? value) where T : class, new() =>
        string.IsNullOrWhiteSpace(value)
            ? new T()
            : JsonSerializer.Deserialize<T>(value, JsonOptions) ?? new T();

    private string? ProtectSecrets(EmailConnectionSecretEnvelope envelope)
    {
        var hasAnySecret = !string.IsNullOrWhiteSpace(envelope.AccessToken)
            || !string.IsNullOrWhiteSpace(envelope.RefreshToken)
            || !string.IsNullOrWhiteSpace(envelope.IncomingPassword)
            || !string.IsNullOrWhiteSpace(envelope.OutgoingPassword)
            || !string.IsNullOrWhiteSpace(envelope.ApiKey);

        if (!hasAnySecret)
        {
            return null;
        }

        return secretProtector.Protect(JsonSerializer.Serialize(envelope, JsonOptions));
    }

    private EmailConnectionResolvedSecrets ReadSecrets(EmailProviderConnection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.EncryptedCredentialsJson))
        {
            return new EmailConnectionResolvedSecrets();
        }

        var plaintext = secretProtector.Unprotect(connection.EncryptedCredentialsJson);
        var envelope = JsonSerializer.Deserialize<EmailConnectionSecretEnvelope>(plaintext, JsonOptions) ?? new EmailConnectionSecretEnvelope();

        return new EmailConnectionResolvedSecrets
        {
            AccessToken = envelope.AccessToken,
            RefreshToken = envelope.RefreshToken,
            IncomingPassword = envelope.IncomingPassword,
            OutgoingPassword = envelope.OutgoingPassword,
            ApiKey = envelope.ApiKey,
            TokenType = envelope.TokenType,
            Scope = envelope.Scope
        };
    }

    private static EmailConnectionResolvedSettings ReadSettings(EmailProviderConnection connection) =>
        Deserialize<EmailConnectionResolvedSettings>(connection.ConnectionSettingsJson);

    private static EmailConnectionSecretEnvelope ToEnvelope(EmailConnectionResolvedSecrets secrets) => new()
    {
        AccessToken = secrets.AccessToken,
        RefreshToken = secrets.RefreshToken,
        IncomingPassword = secrets.IncomingPassword,
        OutgoingPassword = secrets.OutgoingPassword,
        ApiKey = secrets.ApiKey,
        TokenType = secrets.TokenType,
        Scope = secrets.Scope
    };

    private static EmailConnectionResolvedSettings ToResolvedSettings(EmailConnectionSettingsInputModel model) => new()
    {
        IncomingHost = NormalizeOptional(model.IncomingHost, 256),
        IncomingPort = model.IncomingPort,
        IncomingUseSsl = model.IncomingUseSsl ?? true,
        IncomingUsername = NormalizeOptional(model.IncomingUsername, 256),
        IncomingFolder = NormalizeOptional(model.IncomingFolder, 128) ?? "Inbox",
        OutgoingHost = NormalizeOptional(model.OutgoingHost, 256),
        OutgoingPort = model.OutgoingPort,
        OutgoingUseSsl = model.OutgoingUseSsl ?? true,
        OutgoingUsername = NormalizeOptional(model.OutgoingUsername, 256),
        FromAddressOverride = NormalizeOptional(model.FromAddressOverride, 256),
        ReplyToAddress = NormalizeOptional(model.ReplyToAddress, 256),
        MicrosoftTenantId = NormalizeOptional(model.MicrosoftTenantId, 128),
        SendGridFromAddress = NormalizeOptional(model.SendGridFromAddress, 256),
        SyncIntervalMinutes = model.SyncIntervalMinutes is > 0 and <= 1440 ? model.SyncIntervalMinutes.Value : 15
    };

    private static EmailProviderConnectionListItemModel ToListModel(EmailProviderConnection connection) => new()
    {
        Id = connection.Id,
        ConnectionName = connection.ConnectionName,
        ProviderType = connection.ProviderType,
        AuthMethod = connection.AuthMethod,
        Status = connection.Status,
        MailboxAddress = connection.MailboxAddress,
        DisplayName = connection.DisplayName,
        AllowSend = connection.AllowSend,
        AllowSync = connection.AllowSync,
        IsDefaultConnection = connection.IsDefaultConnection,
        LastSyncedAt = connection.LastSyncedAt,
        LastSuccessfulSendAt = connection.LastSuccessfulSendAt,
        LastTestedAt = connection.LastTestedAt,
        LastError = connection.LastError,
        CreatedAt = connection.CreatedAt,
        UpdatedAt = connection.UpdatedAt
    };

    private EmailProviderConnectionModel ToModel(EmailProviderConnection connection)
    {
        var provider = providerResolver.GetRequiredProvider(connection.ProviderType);
        var settings = ReadSettings(connection);

        return new EmailProviderConnectionModel
        {
            Id = connection.Id,
            ConnectionName = connection.ConnectionName,
            ProviderType = connection.ProviderType,
            AuthMethod = connection.AuthMethod,
            Status = connection.Status,
            MailboxAddress = connection.MailboxAddress,
            DisplayName = connection.DisplayName,
            AllowSend = connection.AllowSend,
            AllowSync = connection.AllowSync,
            IsDefaultConnection = connection.IsDefaultConnection,
            LastSyncedAt = connection.LastSyncedAt,
            LastSuccessfulSendAt = connection.LastSuccessfulSendAt,
            LastTestedAt = connection.LastTestedAt,
            LastError = connection.LastError,
            CreatedAt = connection.CreatedAt,
            UpdatedAt = connection.UpdatedAt,
            Capabilities = new EmailConnectionCapabilitiesModel
            {
                SupportsOAuth = provider.SupportsOAuth,
                SupportsSend = provider.SupportsSend,
                SupportsSync = provider.SupportsSync,
                SupportsWebhook = provider.SupportsWebhook
            },
            Settings = new EmailConnectionSettingsModel
            {
                IncomingHost = settings.IncomingHost,
                IncomingPort = settings.IncomingPort,
                IncomingUseSsl = settings.IncomingUseSsl,
                IncomingUsername = settings.IncomingUsername,
                IncomingFolder = settings.IncomingFolder,
                OutgoingHost = settings.OutgoingHost,
                OutgoingPort = settings.OutgoingPort,
                OutgoingUseSsl = settings.OutgoingUseSsl,
                OutgoingUsername = settings.OutgoingUsername,
                FromAddressOverride = settings.FromAddressOverride,
                ReplyToAddress = settings.ReplyToAddress,
                MicrosoftTenantId = settings.MicrosoftTenantId,
                SendGridFromAddress = settings.SendGridFromAddress,
                SyncIntervalMinutes = settings.SyncIntervalMinutes
            },
            SyncState = new EmailConnectionSyncStateModel
            {
                CursorPreview = PreviewCursor(connection.SyncCursorJson),
                LastSyncedAt = connection.LastSyncedAt,
                NextSyncAt = connection.NextSyncAt
            },
            WebhookState = new EmailConnectionWebhookStateModel
            {
                SubscriptionId = connection.WebhookSubscriptionId,
                SubscriptionExpiresAt = connection.WebhookSubscriptionExpiresAt
            }
        };
    }

    private IEmailOAuthProvider GetOAuthProvider(EmailIntegrationProviderType providerType) =>
        providerResolver.GetRequiredProvider(providerType) as IEmailOAuthProvider
        ?? throw new InvalidOperationException($"Provider '{providerType}' does not support OAuth flows.");

    private void ApplyConnectionRefresh(EmailProviderConnection connection, EmailConnectionResolvedSecrets? updatedSecrets, DateTime? accessTokenExpiresAt)
    {
        if (updatedSecrets is not null)
        {
            connection.EncryptedCredentialsJson = ProtectSecrets(ToEnvelope(updatedSecrets));
        }

        if (accessTokenExpiresAt.HasValue)
        {
            connection.AccessTokenExpiresAt = accessTokenExpiresAt.Value;
        }
    }
}
