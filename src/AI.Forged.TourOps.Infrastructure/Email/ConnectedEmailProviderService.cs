using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Models.EmailIntegrations;
using AI.Forged.TourOps.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AI.Forged.TourOps.Infrastructure.Email;

public sealed class ConnectedEmailProviderService(
    IEmailIntegrationRepository emailIntegrationRepository,
    IEmailConnectionSecretProtector secretProtector,
    IEmailIntegrationProviderResolver providerResolver,
    ICurrentUserContext currentUserContext,
    ILogger<ConnectedEmailProviderService> logger) : IEmailProviderService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<string> SendDraftAsync(EmailDraft draft, CancellationToken cancellationToken = default)
    {
        var recipient = draft.EmailThread?.SupplierEmail;
        if (string.IsNullOrWhiteSpace(recipient))
        {
            logger.LogInformation("Falling back to log-only email send for draft {DraftId}. No thread recipient was available.", draft.Id);
            return $"logged-{draft.Id:N}";
        }

        var ownerUserId = currentUserContext.GetRequiredUserId();
        var connection = await emailIntegrationRepository.GetDefaultSendConnectionAsync(ownerUserId, cancellationToken);
        if (connection is null)
        {
            logger.LogInformation("Falling back to log-only email send for draft {DraftId}. No active default email integration exists.", draft.Id);
            return $"logged-{draft.Id:N}";
        }

        var provider = providerResolver.GetRequiredProvider(connection.ProviderType) as IEmailSendProvider;
        if (provider is null)
        {
            logger.LogInformation("Falling back to log-only email send for draft {DraftId}. Provider {ProviderType} has no send capability.", draft.Id, connection.ProviderType);
            return $"logged-{draft.Id:N}";
        }

        var settings = string.IsNullOrWhiteSpace(connection.ConnectionSettingsJson)
            ? new EmailConnectionResolvedSettings()
            : JsonSerializer.Deserialize<EmailConnectionResolvedSettings>(connection.ConnectionSettingsJson, JsonOptions) ?? new EmailConnectionResolvedSettings();
        var secrets = ResolveSecrets(connection.EncryptedCredentialsJson);

        var result = await provider.SendAsync(connection, settings, secrets, new EmailProviderSendRequest
        {
            FromAddress = settings.FromAddressOverride ?? connection.MailboxAddress,
            Subject = draft.Subject,
            BodyText = draft.Body,
            ToAddresses = [recipient]
        }, cancellationToken);

        if (result.UpdatedSecrets is not null)
        {
            connection.EncryptedCredentialsJson = ProtectSecrets(result.UpdatedSecrets);
        }

        connection.LastSuccessfulSendAt = DateTime.UtcNow;
        connection.LastError = null;
        connection.AccessTokenExpiresAt = result.AccessTokenExpiresAt ?? connection.AccessTokenExpiresAt;
        connection.UpdatedAt = DateTime.UtcNow;
        await emailIntegrationRepository.UpdateConnectionAsync(connection, cancellationToken);

        return result.ProviderMessageId;
    }

    private EmailConnectionResolvedSecrets ResolveSecrets(string? encryptedSecrets)
    {
        if (string.IsNullOrWhiteSpace(encryptedSecrets))
        {
            return new EmailConnectionResolvedSecrets();
        }

        var plaintext = secretProtector.Unprotect(encryptedSecrets);
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

    private string? ProtectSecrets(EmailConnectionResolvedSecrets secrets)
    {
        var envelope = new EmailConnectionSecretEnvelope
        {
            AccessToken = secrets.AccessToken,
            RefreshToken = secrets.RefreshToken,
            IncomingPassword = secrets.IncomingPassword,
            OutgoingPassword = secrets.OutgoingPassword,
            ApiKey = secrets.ApiKey,
            TokenType = secrets.TokenType,
            Scope = secrets.Scope
        };

        return secretProtector.Protect(JsonSerializer.Serialize(envelope, JsonOptions));
    }
}
