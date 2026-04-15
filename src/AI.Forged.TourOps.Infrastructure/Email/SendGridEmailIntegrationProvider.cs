using System.Net.Http.Headers;
using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Models.EmailIntegrations;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Email;

public sealed class SendGridEmailIntegrationProvider(
    HttpClient httpClient,
    IOptions<SendGridEmailProviderSettings> settings) : IEmailSendProvider
{
    public EmailIntegrationProviderType ProviderType => EmailIntegrationProviderType.SendGrid;
    public bool SupportsOAuth => false;
    public bool SupportsSend => true;
    public bool SupportsSync => false;
    public bool SupportsWebhook => false;

    public async Task TestSendCapabilityAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secrets.ApiKey))
        {
            throw new InvalidOperationException("SendGrid API key is missing.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{settings.Value.BaseUrl.TrimEnd('/')}/user/account");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secrets.ApiKey);
        using var _ = await HttpEmailProviderUtilities.SendJsonAsync(httpClient, request, cancellationToken);
    }

    public async Task<EmailProviderSendResult> SendAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings connectionSettings, EmailConnectionResolvedSecrets secrets, EmailProviderSendRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secrets.ApiKey))
        {
            throw new InvalidOperationException("SendGrid API key is missing.");
        }

        var fromAddress = connectionSettings.SendGridFromAddress ?? request.FromAddress;

        using var sendRequest = new HttpRequestMessage(HttpMethod.Post, $"{settings.Value.BaseUrl.TrimEnd('/')}/mail/send");
        sendRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secrets.ApiKey);
        sendRequest.Content = HttpEmailProviderUtilities.JsonBody(new
        {
            from = new { email = fromAddress },
            personalizations = new[]
            {
                new
                {
                    to = request.ToAddresses.Select(x => new { email = x }).ToArray(),
                    cc = request.CcAddresses.Select(x => new { email = x }).ToArray(),
                    bcc = request.BccAddresses.Select(x => new { email = x }).ToArray()
                }
            },
            reply_to = string.IsNullOrWhiteSpace(request.ReplyToAddress) ? null : new { email = request.ReplyToAddress },
            subject = request.Subject,
            content = new[]
            {
                string.IsNullOrWhiteSpace(request.BodyText) ? null : new { type = "text/plain", value = request.BodyText },
                string.IsNullOrWhiteSpace(request.BodyHtml) ? null : new { type = "text/html", value = request.BodyHtml }
            }.Where(x => x is not null).ToArray(),
            attachments = request.Attachments.Select(x => new
            {
                filename = x.FileName,
                type = x.ContentType,
                content = x.ContentBase64
            }).ToArray()
        });

        using var response = await httpClient.SendAsync(sendRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(HttpEmailProviderUtilities.BuildErrorMessage(response.StatusCode, body));
        }

        var messageId = response.Headers.TryGetValues("X-Message-Id", out var values)
            ? values.FirstOrDefault() ?? Guid.NewGuid().ToString("N")
            : Guid.NewGuid().ToString("N");

        return new EmailProviderSendResult
        {
            ProviderMessageId = messageId,
            MetadataJson = JsonSerializer.Serialize(new { provider = "SendGrid" })
        };
    }
}
