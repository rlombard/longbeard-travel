using System.Net.Http.Headers;
using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Models.EmailIntegrations;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Email;

public sealed class MicrosoftGraphEmailIntegrationProvider(
    HttpClient httpClient,
    IOptions<MicrosoftEmailProviderSettings> settings) : IEmailOAuthProvider, IEmailSendProvider, IEmailSyncProvider
{
    public EmailIntegrationProviderType ProviderType => EmailIntegrationProviderType.Microsoft365;
    public bool SupportsOAuth => true;
    public bool SupportsSend => true;
    public bool SupportsSync => true;
    public bool SupportsWebhook => false;

    public Task<EmailOAuthStartResultModel> StartAuthorizationAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        var tenantId = connectionSettings.MicrosoftTenantId ?? settings.Value.DefaultTenantId;
        var query = new Dictionary<string, string>
        {
            ["client_id"] = settings.Value.ClientId!,
            ["response_type"] = "code",
            ["redirect_uri"] = settings.Value.RedirectUri,
            ["response_mode"] = "query",
            ["scope"] = settings.Value.Scope,
            ["state"] = connection.OAuthState ?? throw new InvalidOperationException("OAuth state is missing.")
        };

        var authUrl = $"https://login.microsoftonline.com/{Uri.EscapeDataString(tenantId)}/oauth2/v2.0/authorize?{string.Join("&", query.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"))}";

        return Task.FromResult(new EmailOAuthStartResultModel
        {
            ConnectionId = connection.Id,
            AuthorizationUrl = authUrl,
            State = connection.OAuthState!
        });
    }

    public async Task<EmailAuthorizationCompletionResult> CompleteAuthorizationAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, string code, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        var token = await ExchangeCodeAsync(connectionSettings.MicrosoftTenantId, code, cancellationToken);
        var profile = await GetProfileAsync(token.AccessToken, cancellationToken);

        return new EmailAuthorizationCompletionResult
        {
            MailboxAddress = profile.MailboxAddress,
            DisplayName = profile.DisplayName,
            ExternalAccountId = profile.ExternalAccountId,
            Secrets = new EmailConnectionResolvedSecrets
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                TokenType = token.TokenType,
                Scope = token.Scope
            },
            AccessTokenExpiresAt = token.ExpiresAtUtc
        };
    }

    public async Task TestSendCapabilityAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default)
    {
        var token = await EnsureAccessTokenAsync(connection, connectionSettings, secrets, cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{settings.Value.BaseUrl.TrimEnd('/')}/me?$select=id,mail,userPrincipalName");
        HttpEmailProviderUtilities.ApplyBearer(request, token.AccessToken, token.TokenType);
        using var _ = await HttpEmailProviderUtilities.SendJsonAsync(httpClient, request, cancellationToken);
    }

    public async Task<EmailProviderSendResult> SendAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, EmailProviderSendRequest request, CancellationToken cancellationToken = default)
    {
        var token = await EnsureAccessTokenAsync(connection, connectionSettings, secrets, cancellationToken);

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, $"{settings.Value.BaseUrl.TrimEnd('/')}/me/messages");
        HttpEmailProviderUtilities.ApplyBearer(createRequest, token.AccessToken, token.TokenType);
        createRequest.Content = HttpEmailProviderUtilities.JsonBody(new
        {
            subject = request.Subject,
            body = new
            {
                contentType = string.IsNullOrWhiteSpace(request.BodyHtml) ? "Text" : "HTML",
                content = request.BodyHtml ?? request.BodyText ?? string.Empty
            },
            toRecipients = request.ToAddresses.Select(ToRecipient).ToArray(),
            ccRecipients = request.CcAddresses.Select(ToRecipient).ToArray(),
            bccRecipients = request.BccAddresses.Select(ToRecipient).ToArray(),
            replyTo = string.IsNullOrWhiteSpace(request.ReplyToAddress) ? Array.Empty<object>() : new[] { ToRecipient(request.ReplyToAddress!) }
        });

        using var draftJson = await HttpEmailProviderUtilities.SendJsonAsync(httpClient, createRequest, cancellationToken);
        var draftRoot = draftJson.RootElement;
        var messageId = draftRoot.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Microsoft Graph draft id was missing.");
        var conversationId = draftRoot.TryGetProperty("conversationId", out var conversationElement)
            ? conversationElement.GetString()
            : null;

        using var sendRequest = new HttpRequestMessage(HttpMethod.Post, $"{settings.Value.BaseUrl.TrimEnd('/')}/me/messages/{Uri.EscapeDataString(messageId)}/send");
        HttpEmailProviderUtilities.ApplyBearer(sendRequest, token.AccessToken, token.TokenType);
        using var sendResponse = await httpClient.SendAsync(sendRequest, cancellationToken);
        if (!sendResponse.IsSuccessStatusCode)
        {
            var errorBody = await sendResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(HttpEmailProviderUtilities.BuildErrorMessage(sendResponse.StatusCode, errorBody));
        }

        return new EmailProviderSendResult
        {
            ProviderMessageId = messageId,
            ProviderThreadId = conversationId,
            MetadataJson = JsonSerializer.Serialize(new { provider = "MicrosoftGraph", conversationId }),
            UpdatedSecrets = token.ToResolvedSecrets(),
            AccessTokenExpiresAt = token.ExpiresAtUtc
        };
    }

    public async Task TestSyncCapabilityAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default)
    {
        var token = await EnsureAccessTokenAsync(connection, connectionSettings, secrets, cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{settings.Value.BaseUrl.TrimEnd('/')}/me/mailFolders/inbox?$select=id,displayName");
        HttpEmailProviderUtilities.ApplyBearer(request, token.AccessToken, token.TokenType);
        using var _ = await HttpEmailProviderUtilities.SendJsonAsync(httpClient, request, cancellationToken);
    }

    public async Task<EmailSyncEnvelope> SyncAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default)
    {
        var token = await EnsureAccessTokenAsync(connection, connectionSettings, secrets, cancellationToken);
        var lastSyncedAt = connection.LastSyncedAt?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(-14);
        var filter = Uri.EscapeDataString($"receivedDateTime ge {lastSyncedAt:O}");
        var path = $"{settings.Value.BaseUrl.TrimEnd('/')}/me/mailFolders/inbox/messages?$top=50&$orderby=receivedDateTime asc&$filter={filter}&$select=id,conversationId,subject,body,bodyPreview,from,toRecipients,ccRecipients,receivedDateTime";

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.TryAddWithoutValidation("Prefer", "outlook.body-content-type=\"text\"");
        HttpEmailProviderUtilities.ApplyBearer(request, token.AccessToken, token.TokenType);
        using var json = await HttpEmailProviderUtilities.SendJsonAsync(httpClient, request, cancellationToken);

        var messages = new List<EmailSyncedMessageModel>();
        if (json.RootElement.TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                var bodyText = item.TryGetProperty("bodyPreview", out var preview) ? preview.GetString() ?? string.Empty : string.Empty;
                var bodyHtml = item.TryGetProperty("body", out var body) && body.ValueKind == JsonValueKind.Object && body.TryGetProperty("content", out var content)
                    ? content.GetString()
                    : null;

                messages.Add(new EmailSyncedMessageModel
                {
                    ProviderMessageId = item.GetProperty("id").GetString() ?? throw new InvalidOperationException("Microsoft message id missing."),
                    ProviderThreadId = item.TryGetProperty("conversationId", out var conversationId) ? conversationId.GetString() : null,
                    FolderName = connectionSettings.IncomingFolder,
                    Subject = item.TryGetProperty("subject", out var subject) ? subject.GetString() ?? string.Empty : string.Empty,
                    BodyText = bodyText,
                    BodyHtml = bodyHtml,
                    Sender = item.TryGetProperty("from", out var from)
                        ? from.GetProperty("emailAddress").GetProperty("address").GetString() ?? string.Empty
                        : string.Empty,
                    Recipients = JoinRecipients(item, "toRecipients", "ccRecipients"),
                    SentAtUtc = item.TryGetProperty("receivedDateTime", out var sentAt)
                        ? DateTime.Parse(sentAt.GetString() ?? DateTime.UtcNow.ToString("O")).ToUniversalTime()
                        : DateTime.UtcNow
                });
            }
        }

        return new EmailSyncEnvelope
        {
            Messages = messages,
            NextCursorJson = JsonSerializer.Serialize(new { lastSyncedAtUtc = DateTime.UtcNow }),
            UpdatedSecrets = token.ToResolvedSecrets(),
            AccessTokenExpiresAt = token.ExpiresAtUtc
        };
    }

    private async Task<MicrosoftTokenEnvelope> EnsureAccessTokenAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(secrets.AccessToken)
            && connection.AccessTokenExpiresAt.HasValue
            && connection.AccessTokenExpiresAt > DateTime.UtcNow.AddMinutes(2))
        {
            return new MicrosoftTokenEnvelope(secrets.AccessToken!, secrets.RefreshToken, secrets.TokenType ?? "Bearer", secrets.Scope, connection.AccessTokenExpiresAt);
        }

        if (string.IsNullOrWhiteSpace(secrets.RefreshToken))
        {
            throw new InvalidOperationException("Microsoft refresh token is missing. Reconnect the mailbox.");
        }

        return await RefreshTokenAsync(connectionSettings.MicrosoftTenantId, secrets.RefreshToken!, cancellationToken);
    }

    private async Task<MicrosoftTokenEnvelope> ExchangeCodeAsync(string? tenantIdOverride, string code, CancellationToken cancellationToken)
    {
        var tenantId = tenantIdOverride ?? settings.Value.DefaultTenantId;
        using var response = await httpClient.PostAsync(
            $"https://login.microsoftonline.com/{Uri.EscapeDataString(tenantId)}/oauth2/v2.0/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = settings.Value.ClientId!,
                ["client_secret"] = settings.Value.ClientSecret!,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = settings.Value.RedirectUri,
                ["scope"] = settings.Value.Scope
            }),
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(HttpEmailProviderUtilities.BuildErrorMessage(response.StatusCode, body));
        }

        using var json = JsonDocument.Parse(body);
        return ParseTokenEnvelope(json.RootElement);
    }

    private async Task<MicrosoftTokenEnvelope> RefreshTokenAsync(string? tenantIdOverride, string refreshToken, CancellationToken cancellationToken)
    {
        var tenantId = tenantIdOverride ?? settings.Value.DefaultTenantId;
        using var response = await httpClient.PostAsync(
            $"https://login.microsoftonline.com/{Uri.EscapeDataString(tenantId)}/oauth2/v2.0/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = settings.Value.ClientId!,
                ["client_secret"] = settings.Value.ClientSecret!,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token",
                ["redirect_uri"] = settings.Value.RedirectUri,
                ["scope"] = settings.Value.Scope
            }),
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(HttpEmailProviderUtilities.BuildErrorMessage(response.StatusCode, body));
        }

        using var json = JsonDocument.Parse(body);
        return ParseTokenEnvelope(json.RootElement);
    }

    private async Task<MicrosoftProfileEnvelope> GetProfileAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{settings.Value.BaseUrl.TrimEnd('/')}/me?$select=id,displayName,mail,userPrincipalName");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var json = await HttpEmailProviderUtilities.SendJsonAsync(httpClient, request, cancellationToken);
        var root = json.RootElement;

        return new MicrosoftProfileEnvelope(
            root.TryGetProperty("mail", out var mail) && !string.IsNullOrWhiteSpace(mail.GetString())
                ? mail.GetString()!
                : root.GetProperty("userPrincipalName").GetString() ?? throw new InvalidOperationException("Microsoft mailbox address missing."),
            root.TryGetProperty("displayName", out var displayName) ? displayName.GetString() : null,
            root.GetProperty("id").GetString());
    }

    private static MicrosoftTokenEnvelope ParseTokenEnvelope(JsonElement root) =>
        new(
            root.GetProperty("access_token").GetString() ?? throw new InvalidOperationException("Microsoft access token missing."),
            root.TryGetProperty("refresh_token", out var refreshToken) ? refreshToken.GetString() : null,
            root.TryGetProperty("token_type", out var tokenType) ? tokenType.GetString() ?? "Bearer" : "Bearer",
            root.TryGetProperty("scope", out var scope) ? scope.GetString() : null,
            HttpEmailProviderUtilities.ReadExpiresAt(root));

    private static object ToRecipient(string address) => new
    {
        emailAddress = new
        {
            address
        }
    };

    private static string JoinRecipients(JsonElement item, params string[] propertyNames)
    {
        var recipients = new List<string>();

        foreach (var propertyName in propertyNames)
        {
            if (!item.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            recipients.AddRange(property.EnumerateArray()
                .Select(x => x.GetProperty("emailAddress").GetProperty("address").GetString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!));
        }

        return string.Join(';', recipients);
    }

    private void EnsureEnabled()
    {
        if (!settings.Value.Enabled || string.IsNullOrWhiteSpace(settings.Value.ClientId) || string.IsNullOrWhiteSpace(settings.Value.ClientSecret))
        {
            throw new InvalidOperationException("Microsoft email integration settings are not configured.");
        }
    }

    private sealed record MicrosoftTokenEnvelope(string AccessToken, string? RefreshToken, string TokenType, string? Scope, DateTime? ExpiresAtUtc)
    {
        public EmailConnectionResolvedSecrets ToResolvedSecrets() => new()
        {
            AccessToken = AccessToken,
            RefreshToken = RefreshToken,
            TokenType = TokenType,
            Scope = Scope
        };
    }

    private sealed record MicrosoftProfileEnvelope(string MailboxAddress, string? DisplayName, string? ExternalAccountId);
}
