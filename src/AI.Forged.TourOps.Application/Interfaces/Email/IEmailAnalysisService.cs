using AI.Forged.TourOps.Application.Models.Operations;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces.Email;

public interface IEmailAnalysisService
{
    Task<EmailSignalExtraction> AnalyzeThreadAsync(Guid emailThreadId, CancellationToken cancellationToken = default);
    Task<EmailSignalExtraction> SummarizeEmailAsync(EmailThread thread, EmailMessage message, CancellationToken cancellationToken = default);
    Task<EmailSignalExtraction> ClassifyEmailAsync(EmailThread thread, EmailMessage message, CancellationToken cancellationToken = default);
    Task<EmailDraft> SuggestReplyAsync(Guid emailThreadId, CancellationToken cancellationToken = default);
    Task<EmailSignalExtraction> ExtractBookingSignalsAsync(EmailThread thread, EmailMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SuggestedTaskCandidate>> SuggestTasksFromEmailAsync(Guid emailThreadId, CancellationToken cancellationToken = default);
}
