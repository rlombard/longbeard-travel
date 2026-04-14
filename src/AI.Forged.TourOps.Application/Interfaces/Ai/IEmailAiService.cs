using AI.Forged.TourOps.Application.Models.Operations;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces.Ai;

public interface IEmailAiService
{
    Task<EmailSignalExtraction> AnalyzeThreadWithAifAsync(Guid emailThreadId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SuggestedTaskCandidate>> GenerateTasksFromThreadAsync(Guid emailThreadId, CancellationToken cancellationToken = default);
    Task<EmailDraft> GenerateReplyFromThreadAsync(Guid emailThreadId, CancellationToken cancellationToken = default);
}
