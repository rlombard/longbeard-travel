using AI.Forged.TourOps.Application.Models.EmailIntegrations;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Interfaces.Email;

public interface IEmailIntegrationProvider
{
    EmailIntegrationProviderType ProviderType { get; }
    bool SupportsOAuth { get; }
    bool SupportsSend { get; }
    bool SupportsSync { get; }
    bool SupportsWebhook { get; }
}

public interface IEmailOAuthProvider : IEmailIntegrationProvider
{
    Task<EmailOAuthStartResultModel> StartAuthorizationAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings settings, CancellationToken cancellationToken = default);
    Task<EmailAuthorizationCompletionResult> CompleteAuthorizationAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings settings, EmailConnectionResolvedSecrets secrets, string code, CancellationToken cancellationToken = default);
}

public interface IEmailSendProvider : IEmailIntegrationProvider
{
    Task TestSendCapabilityAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings settings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default);
    Task<EmailProviderSendResult> SendAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings settings, EmailConnectionResolvedSecrets secrets, EmailProviderSendRequest request, CancellationToken cancellationToken = default);
}

public interface IEmailSyncProvider : IEmailIntegrationProvider
{
    Task TestSyncCapabilityAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings settings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default);
    Task<EmailSyncEnvelope> SyncAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings settings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default);
}

public interface IEmailWebhookProvider : IEmailIntegrationProvider
{
    Task<EmailWebhookProcessingResultModel> HandleWebhookAsync(string? validationToken, string payloadJson, CancellationToken cancellationToken = default);
}
