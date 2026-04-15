using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IEmailRepository
{
    Task<EmailThread> AddThreadAsync(EmailThread thread, CancellationToken cancellationToken = default);
    Task<EmailMessage> AddMessageAsync(EmailMessage message, CancellationToken cancellationToken = default);
    Task<EmailDraft> AddDraftAsync(EmailDraft draft, CancellationToken cancellationToken = default);
    Task<EmailThread?> GetThreadByIdAsync(Guid threadId, CancellationToken cancellationToken = default);
    Task<EmailThread?> GetThreadByExternalThreadIdAsync(string externalThreadId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailThread>> GetThreadsAsync(Guid? bookingId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailThread>> GetThreadsByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailThread>> GetThreadsPendingAutomationAsync(int take, CancellationToken cancellationToken = default);
    Task<EmailMessage?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task<EmailDraft?> GetDraftByIdAsync(Guid draftId, CancellationToken cancellationToken = default);
    Task UpdateThreadAsync(EmailThread thread, CancellationToken cancellationToken = default);
    Task UpdateMessageAsync(EmailMessage message, CancellationToken cancellationToken = default);
    Task UpdateDraftAsync(EmailDraft draft, CancellationToken cancellationToken = default);
}
