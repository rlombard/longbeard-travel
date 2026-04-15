using System.Text;
using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Models.EmailIntegrations;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AI.Forged.TourOps.Infrastructure.Email;

public sealed class GoogleGmailEmailIntegrationProvider(
    HttpClient httpClient,
    IOptions<GoogleEmailProviderSettings> settings) : IEmailOAuthProvider, IEmailSendProvider, IEmailSyncProvider
{
    public EmailIntegrationProviderType ProviderType => EmailIntegrationProviderType.Gmail;
    public bool SupportsOAuth => true;
    public bool SupportsSend => true;
    public bool SupportsSync => true;
    public bool SupportsWebhook => false;

    public Task<EmailOAuthStartResultModel> StartAuthorizationAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();

        var query = new Dictionary<string, string>
        {
            ["client_id"] = settings.Value.ClientId!,
            ["redirect_uri"] = settings.Value.RedirectUri,
            ["response_type"] = "code",
            ["access_type"] = "offline",
            ["prompt"] = "consent",
            ["scope"] = settings.Value.Scope,
            ["state"] = connection.OAuthState ?? throw new InvalidOperationException("OAuth state is missing.")
        };

        var authUrl = $"{settings.Value.AuthorizationUrl}?{string.Join("&", query.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"))}";

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

        var token = await ExchangeCodeAsync(code, cancellationToken);
        var profile = await GetProfileAsync(token.AccessToken, cancellationToken);

        return new EmailAuthorizationCompletionResult
        {
            MailboxAddress = profile.MailboxAddress,
            DisplayName = connection.DisplayName,
            ExternalAccountId = profile.HistoryId,
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
        var token = await EnsureAccessTokenAsync(connection, secrets, cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{settings.Value.GmailApiBaseUrl.TrimEnd('/')}/users/me/profile");
        HttpEmailProviderUtilities.ApplyBearer(request, token.AccessToken, token.TokenType);
        using var _ = await HttpEmailProviderUtilities.SendJsonAsync(httpClient, request, cancellationToken);
    }

    public async Task<EmailProviderSendResult> SendAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, EmailProviderSendRequest request, CancellationToken cancellationToken = default)
    {
        var token = await EnsureAccessTokenAsync(connection, secrets, cancellationToken);
        var mimeMessage = EmailMimeFactory.Create(request);
        using var stream = new MemoryStream();
        await mimeMessage.WriteToAsync(stream, cancellationToken);
        var raw = HttpEmailProviderUtilities.ToBase64Url(stream.ToArray());

        using var sendRequest = new HttpRequestMessage(HttpMethod.Post, $"{settings.Value.GmailApiBaseUrl.TrimEnd('/')}/users/me/messages/send");
        HttpEmailProviderUtilities.ApplyBearer(sendRequest, token.AccessToken, token.TokenType);
        sendRequest.Content = HttpEmailProviderUtilities.JsonBody(new { raw });

        using var json = await HttpEmailProviderUtilities.SendJsonAsync(httpClient, sendRequest, cancellationToken);
        var root = json.RootElement;

        return new EmailProviderSendResult
        {
            ProviderMessageId = root.GetProperty("id").GetString() ?? throw new InvalidOperationException("Gmail message id missing."),
            ProviderThreadId = root.TryGetProperty("threadId", out var threadId) ? threadId.GetString() : null,
            MetadataJson = JsonSerializer.Serialize(new { provider = "Gmail" }),
            UpdatedSecrets = token.ToResolvedSecrets(),
            AccessTokenExpiresAt = token.ExpiresAtUtc
        };
    }

    public async Task TestSyncCapabilityAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default)
    {
        var token = await EnsureAccessTokenAsync(connection, secrets, cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{settings.Value.GmailApiBaseUrl.TrimEnd('/')}/users/me/profile");
        HttpEmailProviderUtilities.ApplyBearer(request, token.AccessToken, token.TokenType);
        using var _ = await HttpEmailProviderUtilities.SendJsonAsync(httpClient, request, cancellationToken);
    }

    public async Task<EmailSyncEnvelope> SyncAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default)
    {
        var token = await EnsureAccessTokenAsync(connection, secrets, cancellationToken);
        var lastSyncedAt = connection.LastSyncedAt?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(-14);
        var query = Uri.EscapeDataString($"after:{new DateTimeOffset(lastSyncedAt).ToUnixTimeSeconds()} in:inbox");

        using var listRequest = new HttpRequestMessage(HttpMethod.Get, $"{settings.Value.GmailApiBaseUrl.TrimEnd('/')}/users/me/messages?q={query}&maxResults=50");
        HttpEmailProviderUtilities.ApplyBearer(listRequest, token.AccessToken, token.TokenType);
        using var listJson = await HttpEmailProviderUtilities.SendJsonAsync(httpClient, listRequest, cancellationToken);

        var messages = new List<EmailSyncedMessageModel>();
        if (listJson.RootElement.TryGetProperty("messages", out var messageRefs) && messageRefs.ValueKind == JsonValueKind.Array)
        {
            foreach (var messageRef in messageRefs.EnumerateArray())
            {
                var id = messageRef.GetProperty("id").GetString();
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                using var detailRequest = new HttpRequestMessage(HttpMethod.Get, $"{settings.Value.GmailApiBaseUrl.TrimEnd('/')}/users/me/messages/{Uri.EscapeDataString(id)}?format=full");
                HttpEmailProviderUtilities.ApplyBearer(detailRequest, token.AccessToken, token.TokenType);
                using var detailJson = await HttpEmailProviderUtilities.SendJsonAsync(httpClient, detailRequest, cancellationToken);
                messages.Add(ToSyncedMessage(detailJson.RootElement, connectionSettings.IncomingFolder));
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

    private async Task<GoogleTokenEnvelope> EnsureAccessTokenAsync(EmailProviderConnection connection, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(secrets.AccessToken)
            && connection.AccessTokenExpiresAt.HasValue
            && connection.AccessTokenExpiresAt > DateTime.UtcNow.AddMinutes(2))
        {
            return new GoogleTokenEnvelope(secrets.AccessToken!, secrets.RefreshToken, secrets.TokenType ?? "Bearer", secrets.Scope, connection.AccessTokenExpiresAt);
        }

        if (string.IsNullOrWhiteSpace(secrets.RefreshToken))
        {
            throw new InvalidOperationException("Google refresh token is missing. Reconnect the mailbox.");
        }

        return await RefreshTokenAsync(secrets.RefreshToken!, cancellationToken);
    }

    private async Task<GoogleTokenEnvelope> ExchangeCodeAsync(string code, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsync(
            settings.Value.TokenUrl,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = settings.Value.ClientId!,
                ["client_secret"] = settings.Value.ClientSecret!,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = settings.Value.RedirectUri
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

    private async Task<GoogleTokenEnvelope> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsync(
            settings.Value.TokenUrl,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = settings.Value.ClientId!,
                ["client_secret"] = settings.Value.ClientSecret!,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token"
            }),
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(HttpEmailProviderUtilities.BuildErrorMessage(response.StatusCode, body));
        }

        using var json = JsonDocument.Parse(body);
        var token = ParseTokenEnvelope(json.RootElement);
        return token with { RefreshToken = refreshToken };
    }

    private async Task<GoogleProfileEnvelope> GetProfileAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{settings.Value.GmailApiBaseUrl.TrimEnd('/')}/users/me/profile");
        HttpEmailProviderUtilities.ApplyBearer(request, accessToken);
        using var json = await HttpEmailProviderUtilities.SendJsonAsync(httpClient, request, cancellationToken);
        var root = json.RootElement;

        return new GoogleProfileEnvelope(
            root.GetProperty("emailAddress").GetString() ?? throw new InvalidOperationException("Gmail mailbox address missing."),
            root.TryGetProperty("historyId", out var historyId) ? historyId.GetString() : null);
    }

    private static GoogleTokenEnvelope ParseTokenEnvelope(JsonElement root) =>
        new(
            root.GetProperty("access_token").GetString() ?? throw new InvalidOperationException("Google access token missing."),
            root.TryGetProperty("refresh_token", out var refreshToken) ? refreshToken.GetString() : null,
            root.TryGetProperty("token_type", out var tokenType) ? tokenType.GetString() ?? "Bearer" : "Bearer",
            root.TryGetProperty("scope", out var scope) ? scope.GetString() : null,
            HttpEmailProviderUtilities.ReadExpiresAt(root));

    private static EmailSyncedMessageModel ToSyncedMessage(JsonElement root, string folderName)
    {
        var payload = root.TryGetProperty("payload", out var payloadElement) ? payloadElement : default;
        var headers = payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty("headers", out var headersElement)
            ? headersElement.EnumerateArray()
                .Where(x => x.TryGetProperty("name", out _) && x.TryGetProperty("value", out _))
                .ToDictionary(
                    x => x.GetProperty("name").GetString() ?? string.Empty,
                    x => x.GetProperty("value").GetString() ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var textBody = ExtractBody(payload, "text/plain");
        if (string.IsNullOrWhiteSpace(textBody) && root.TryGetProperty("snippet", out var snippet))
        {
            textBody = snippet.GetString();
        }
        var htmlBody = ExtractBody(payload, "text/html");
        var internalDate = root.TryGetProperty("internalDate", out var internalDateElement)
            ? DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(internalDateElement.GetString() ?? "0")).UtcDateTime
            : DateTime.UtcNow;

        return new EmailSyncedMessageModel
        {
            ProviderMessageId = root.GetProperty("id").GetString() ?? throw new InvalidOperationException("Gmail message id missing."),
            ProviderThreadId = root.TryGetProperty("threadId", out var threadId) ? threadId.GetString() : null,
            FolderName = folderName,
            Subject = headers.TryGetValue("Subject", out var subject) ? subject : string.Empty,
            BodyText = textBody ?? string.Empty,
            BodyHtml = htmlBody,
            Sender = headers.TryGetValue("From", out var from) ? from : string.Empty,
            Recipients = string.Join(';', new[] { headers.TryGetValue("To", out var to) ? to : null, headers.TryGetValue("Cc", out var cc) ? cc : null }.Where(x => !string.IsNullOrWhiteSpace(x))),
            SentAtUtc = internalDate
        };
    }

    private static string? ExtractBody(JsonElement payload, string mimeType)
    {
        if (payload.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (payload.TryGetProperty("mimeType", out var type) && string.Equals(type.GetString(), mimeType, StringComparison.OrdinalIgnoreCase))
        {
            return DecodeBody(payload);
        }

        if (!payload.TryGetProperty("parts", out var parts) || parts.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var part in parts.EnumerateArray())
        {
            var nested = ExtractBody(part, mimeType);
            if (!string.IsNullOrWhiteSpace(nested))
            {
                return nested;
            }
        }

        return null;
    }

    private static string? DecodeBody(JsonElement part)
    {
        if (!part.TryGetProperty("body", out var body) || !body.TryGetProperty("data", out var dataElement))
        {
            return null;
        }

        var value = dataElement.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Replace('-', '+').Replace('_', '/');
        var padding = 4 - normalized.Length % 4;
        if (padding is > 0 and < 4)
        {
            normalized = normalized.PadRight(normalized.Length + padding, '=');
        }

        return Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
    }

    private void EnsureEnabled()
    {
        if (!settings.Value.Enabled || string.IsNullOrWhiteSpace(settings.Value.ClientId) || string.IsNullOrWhiteSpace(settings.Value.ClientSecret))
        {
            throw new InvalidOperationException("Google email integration settings are not configured.");
        }
    }

    private sealed record GoogleTokenEnvelope(string AccessToken, string? RefreshToken, string TokenType, string? Scope, DateTime? ExpiresAtUtc)
    {
        public EmailConnectionResolvedSecrets ToResolvedSecrets() => new()
        {
            AccessToken = AccessToken,
            RefreshToken = RefreshToken,
            TokenType = TokenType,
            Scope = Scope
        };
    }

    private sealed record GoogleProfileEnvelope(string MailboxAddress, string? HistoryId);
}
