using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Ai;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Interfaces.Operations;
using AI.Forged.TourOps.Application.Models.Ai;
using AI.Forged.TourOps.Application.Models.Operations;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services.Email;

public class EmailAiService(
    IEmailRepository emailRepository,
    ITaskSuggestionRepository taskSuggestionRepository,
    IAiForgedService aiForgedService,
    IPdfService pdfService,
    IHumanApprovalService humanApprovalService) : IEmailAnalysisService, IEmailAiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<EmailSignalExtraction> AnalyzeThreadWithAifAsync(Guid emailThreadId, CancellationToken cancellationToken = default) =>
        AnalyzeThreadAsync(emailThreadId, cancellationToken);

    public async Task<EmailSignalExtraction> AnalyzeThreadAsync(Guid emailThreadId, CancellationToken cancellationToken = default)
    {
        var analysis = await AnalyzeThreadInternalAsync(emailThreadId, cancellationToken);
        await PersistAnalysisAsync(analysis.LatestMessage, analysis.Extraction, cancellationToken);
        return analysis.Extraction;
    }

    public Task<EmailSignalExtraction> SummarizeEmailAsync(EmailThread thread, EmailMessage message, CancellationToken cancellationToken = default) =>
        AnalyzeThreadAsync(thread.Id, cancellationToken);

    public Task<EmailSignalExtraction> ClassifyEmailAsync(EmailThread thread, EmailMessage message, CancellationToken cancellationToken = default) =>
        AnalyzeThreadAsync(thread.Id, cancellationToken);

    public Task<EmailSignalExtraction> ExtractBookingSignalsAsync(EmailThread thread, EmailMessage message, CancellationToken cancellationToken = default) =>
        AnalyzeThreadAsync(thread.Id, cancellationToken);

    public async Task<IReadOnlyList<SuggestedTaskCandidate>> SuggestTasksFromEmailAsync(Guid emailThreadId, CancellationToken cancellationToken = default) =>
        await GenerateTasksFromThreadAsync(emailThreadId, cancellationToken);

    public async Task<IReadOnlyList<SuggestedTaskCandidate>> GenerateTasksFromThreadAsync(Guid emailThreadId, CancellationToken cancellationToken = default)
    {
        var analysis = await AnalyzeThreadInternalAsync(emailThreadId, cancellationToken);
        await PersistAnalysisAsync(analysis.LatestMessage, analysis.Extraction, cancellationToken);

        var bookingId = analysis.Thread.BookingId ?? analysis.Thread.BookingItem?.BookingId
            ?? throw new InvalidOperationException("Email thread is not linked to a booking.");

        var existingSuggestions = await taskSuggestionRepository.GetByBookingAsync(bookingId, cancellationToken);
        var pendingKeys = existingSuggestions
            .Where(x => x.State == TaskSuggestionState.PendingReview && x.Source == "AiForgedEmail")
            .Select(x => $"{x.BookingItemId}:{x.Title}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var normalizedTasks = analysis.Result.Tasks
            .Select(x => NormalizeTaskCandidate(x, analysis.Thread))
            .Where(x => !pendingKeys.Contains($"{x.BookingItemId}:{x.Title}"))
            .ToList();

        if (normalizedTasks.Count == 0)
        {
            return [];
        }

        var entities = normalizedTasks.Select(x => new OperationalTaskSuggestion
        {
            Id = Guid.NewGuid(),
            BookingId = x.BookingId,
            BookingItemId = x.BookingItemId,
            Title = x.Title,
            Description = x.Description,
            SuggestedStatus = x.SuggestedStatus,
            SuggestedDueDate = x.SuggestedDueDate?.ToUniversalTime(),
            Reason = x.Reason,
            Confidence = x.Confidence,
            RequiresHumanReview = true,
            State = TaskSuggestionState.PendingReview,
            Source = "AiForgedEmail",
            LlmProvider = analysis.Result.Provider,
            LlmModel = analysis.Result.ProcessingMode,
            AuditMetadataJson = JsonSerializer.Serialize(new
            {
                emailThreadId = analysis.Thread.Id,
                latestMessageId = analysis.LatestMessage.Id,
                analysis.Result.EmailClassification,
                analysis.Result.Summary
            }, JsonOptions),
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await taskSuggestionRepository.AddRangeAsync(entities, cancellationToken);
        return normalizedTasks;
    }

    public Task<EmailDraft> SuggestReplyAsync(Guid emailThreadId, CancellationToken cancellationToken = default) =>
        GenerateReplyFromThreadAsync(emailThreadId, cancellationToken);

    public async Task<EmailDraft> GenerateReplyFromThreadAsync(Guid emailThreadId, CancellationToken cancellationToken = default)
    {
        var analysis = await AnalyzeThreadInternalAsync(emailThreadId, cancellationToken);
        await PersistAnalysisAsync(analysis.LatestMessage, analysis.Extraction, cancellationToken);

        var reply = analysis.Result.SuggestedReply
            ?? throw new InvalidOperationException("AI Forged did not return a suggested reply for this thread.");

        var now = DateTime.UtcNow;
        var entity = new EmailDraft
        {
            Id = Guid.NewGuid(),
            BookingId = analysis.Thread.BookingId,
            BookingItemId = analysis.Thread.BookingItemId,
            EmailThreadId = analysis.Thread.Id,
            Subject = reply.Subject.Trim(),
            Body = reply.Body.Trim(),
            Status = EmailDraftStatus.Draft,
            GeneratedBy = EmailDraftGeneratedBy.AI,
            LlmProvider = analysis.Result.Provider,
            LlmModel = analysis.Result.ProcessingMode,
            AuditMetadataJson = JsonSerializer.Serialize(new
            {
                emailThreadId = analysis.Thread.Id,
                analysis.Result.EmailClassification,
                analysis.Result.Summary,
                generatedAt = now
            }, JsonOptions),
            CreatedAt = now,
            UpdatedAt = now
        };

        await emailRepository.AddDraftAsync(entity, cancellationToken);
        await humanApprovalService.CreateApprovalRequestAsync(
            "ApproveAiForgedDraft",
            nameof(EmailDraft),
            entity.Id,
            entity.AuditMetadataJson,
            cancellationToken);

        return await emailRepository.GetDraftByIdAsync(entity.Id, cancellationToken)
            ?? throw new InvalidOperationException("Email draft not found after AI Forged generation.");
    }

    private async Task<ThreadAiAnalysis> AnalyzeThreadInternalAsync(Guid emailThreadId, CancellationToken cancellationToken)
    {
        var thread = await emailRepository.GetThreadByIdAsync(emailThreadId, cancellationToken)
            ?? throw new InvalidOperationException("Email thread not found.");

        var latestMessage = thread.Messages
            .OrderByDescending(x => x.SentAt)
            .FirstOrDefault(x => x.Direction == EmailDirection.Inbound)
            ?? thread.Messages.OrderByDescending(x => x.SentAt).FirstOrDefault()
            ?? throw new InvalidOperationException("Cannot analyze a thread with no messages.");

        var pdf = await pdfService.GenerateEmailThreadPdfAsync(thread, cancellationToken);
        var result = await aiForgedService.ProcessEmailPdfAsync(pdf, cancellationToken);
        var extraction = MapExtraction(result);

        return new ThreadAiAnalysis(thread, latestMessage, result, extraction);
    }

    private async Task PersistAnalysisAsync(EmailMessage latestMessage, EmailSignalExtraction extraction, CancellationToken cancellationToken)
    {
        latestMessage.AiSummary = extraction.Summary;
        latestMessage.AiClassification = extraction.Classification;
        latestMessage.AiConfidence = extraction.Confidence;
        latestMessage.RequiresHumanReview = extraction.RequiresHumanReview;
        latestMessage.AiExtractedSignalsJson = JsonSerializer.Serialize(extraction, JsonOptions);
        await emailRepository.UpdateMessageAsync(latestMessage, cancellationToken);
    }

    private static SuggestedTaskCandidate NormalizeTaskCandidate(SuggestedTaskCandidate candidate, EmailThread thread)
    {
        var bookingId = thread.BookingId ?? thread.BookingItem?.BookingId
            ?? throw new InvalidOperationException("Email thread is not linked to a booking.");

        return new SuggestedTaskCandidate
        {
            Title = candidate.Title,
            Description = candidate.Description,
            SuggestedStatus = candidate.SuggestedStatus,
            SuggestedDueDate = candidate.SuggestedDueDate,
            BookingId = candidate.BookingId == Guid.Empty ? bookingId : candidate.BookingId,
            BookingItemId = candidate.BookingItemId ?? thread.BookingItemId,
            Reason = candidate.Reason,
            Confidence = ClampConfidence(candidate.Confidence),
            RequiresHumanReview = true
        };
    }

    private static EmailSignalExtraction MapExtraction(AiForgedProcessingResult result) => new()
    {
        Classification = result.EmailClassification,
        Summary = result.Summary,
        Reason = result.Reason,
        Confidence = ClampConfidence(result.Confidence),
        RequiresHumanReview = result.RequiresHumanReview,
        HasConfirmation = result.EmailClassification == EmailClassificationType.ConfirmationReceived,
        HasPricingChange = result.EmailClassification == EmailClassificationType.PricingChanged,
        HasAvailabilityIssue = result.EmailClassification == EmailClassificationType.AvailabilityIssue,
        RequestsMoreInformation = result.EmailClassification == EmailClassificationType.NeedsMoreInformation,
        MissingInformationItems = result.MissingInformationItems.ToArray(),
        RecommendedActions = result.RecommendedActions.ToArray()
    };

    private static decimal ClampConfidence(decimal confidence) => Math.Min(1m, Math.Max(0m, confidence));

    private sealed record ThreadAiAnalysis(EmailThread Thread, EmailMessage LatestMessage, AiForgedProcessingResult Result, EmailSignalExtraction Extraction);
}
