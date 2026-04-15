using AI.Forged.TourOps.Application.Models.EmailIntegrations;

namespace AI.Forged.TourOps.Application.Interfaces.Email;

public interface IEmailIntegrationService
{
    Task<IReadOnlyList<EmailProviderConnectionListItemModel>> GetConnectionsAsync(CancellationToken cancellationToken = default);
    Task<EmailProviderConnectionModel?> GetConnectionAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task<EmailProviderConnectionModel> CreateConnectionAsync(CreateEmailProviderConnectionModel model, CancellationToken cancellationToken = default);
    Task<EmailOAuthStartResultModel> StartOAuthAsync(StartEmailProviderOAuthModel model, CancellationToken cancellationToken = default);
    Task<EmailOAuthCallbackResultModel> CompleteOAuthAsync(CompleteEmailOAuthCallbackModel model, CancellationToken cancellationToken = default);
    Task<EmailConnectionTestResultModel> TestConnectionAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task<EmailSyncResultModel> SyncConnectionAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task<EmailSendResultModel> SendMessageAsync(Guid connectionId, SendConnectedEmailMessageModel model, CancellationToken cancellationToken = default);
    Task<DisconnectEmailConnectionResultModel> DisconnectAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task<EmailWebhookProcessingResultModel> HandleMicrosoftGraphWebhookAsync(string? validationToken, string payloadJson, CancellationToken cancellationToken = default);
    Task<int> RunDueSyncAsync(CancellationToken cancellationToken = default);
}
