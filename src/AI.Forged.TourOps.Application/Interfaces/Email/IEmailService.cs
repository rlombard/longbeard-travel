using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Interfaces.Email;

public interface IEmailService
{
    Task<EmailThread> CreateThreadAsync(Guid bookingId, Guid? bookingItemId, string subject, string supplierEmail, string? externalThreadId, CancellationToken cancellationToken = default);
    Task<EmailMessage> AddMessageAsync(Guid emailThreadId, EmailDirection direction, string subject, string bodyText, string? bodyHtml, string sender, string recipients, DateTime sentAt, CancellationToken cancellationToken = default);
    Task<EmailDraft> CreateDraftAsync(Guid? bookingId, Guid? bookingItemId, Guid? emailThreadId, string subject, string body, EmailDraftGeneratedBy generatedBy, string? llmProvider, string? llmModel, string? auditMetadataJson, CancellationToken cancellationToken = default);
    Task<EmailDraft> UpdateDraftAsync(Guid draftId, string subject, string body, CancellationToken cancellationToken = default);
    Task<EmailDraft> ApproveDraftAsync(Guid draftId, CancellationToken cancellationToken = default);
    Task<EmailDraft> SendDraftAsync(Guid draftId, CancellationToken cancellationToken = default);
    Task<EmailThread?> GetThreadAsync(Guid threadId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailThread>> GetThreadsAsync(Guid? bookingId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailThread>> GetThreadsByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
