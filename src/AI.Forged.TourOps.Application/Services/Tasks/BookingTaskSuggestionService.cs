using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Ai;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Interfaces.Tasks;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using OperationalTaskStatus = AI.Forged.TourOps.Domain.Enums.TaskStatus;

namespace AI.Forged.TourOps.Application.Services.Tasks;

public class BookingTaskSuggestionService(
    ITaskSuggestionRepository taskSuggestionRepository,
    ITaskService taskService,
    ICurrentUserContext currentUserContext,
    IBookingAiService bookingAiService,
    ILicensePolicyService? licensePolicyService = null,
    IUsageMeteringService? usageMeteringService = null) : IBookingTaskSuggestionService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<IReadOnlyList<OperationalTaskSuggestion>> GetSuggestedTasksAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        taskSuggestionRepository.GetByBookingAsync(bookingId, cancellationToken);

    public async Task<IReadOnlyList<OperationalTaskSuggestion>> GenerateSuggestedTasksAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        if (licensePolicyService is not null)
        {
            await licensePolicyService.AssertAllowedAsync("ai.task-suggestions", cancellationToken);
        }

        var existingSuggestions = await taskSuggestionRepository.GetByBookingAsync(bookingId, cancellationToken);
        var existingPendingBookingSuggestions = existingSuggestions
            .Where(x => x.State == TaskSuggestionState.PendingReview && x.Source == "AiForgedBooking")
            .ToList();

        if (existingPendingBookingSuggestions.Count > 0)
        {
            return existingSuggestions;
        }

        var aiSuggestions = await bookingAiService.GenerateSuggestedTasksAsync(bookingId, cancellationToken);
        var suggestions = aiSuggestions
            .Where(x => x.BookingId == bookingId)
            .Select(x => new OperationalTaskSuggestion
            {
                Id = Guid.NewGuid(),
                BookingId = x.BookingId,
                BookingItemId = x.BookingItemId,
                Title = NormalizeRequired(x.Title, "Suggested task title is required."),
                Description = NormalizeRequired(x.Description, "Suggested task description is required."),
                SuggestedStatus = x.SuggestedStatus,
                SuggestedDueDate = x.SuggestedDueDate?.ToUniversalTime(),
                Reason = NormalizeRequired(x.Reason, "Suggested task reason is required."),
                Confidence = ClampConfidence(x.Confidence),
                RequiresHumanReview = true,
                State = TaskSuggestionState.PendingReview,
                Source = "AiForgedBooking",
                LlmProvider = "AiForgedStub",
                LlmModel = "PdfIngestion",
                AuditMetadataJson = JsonSerializer.Serialize(new
                {
                    source = "AiForgedBooking",
                    generatedAt = DateTime.UtcNow
                }, JsonOptions),
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (suggestions.Count == 0)
        {
            return existingSuggestions;
        }

        await taskSuggestionRepository.AddRangeAsync(suggestions, cancellationToken);
        if (usageMeteringService is not null)
        {
            await usageMeteringService.RecordAsync(new AI.Forged.TourOps.Application.Models.Platform.MeterUsageModel
            {
                Category = "AI",
                MetricKey = "ai.jobs.monthly",
                Quantity = 1,
                Unit = "job",
                Source = "BookingTaskSuggestionService",
                ReferenceEntityType = nameof(Booking),
                ReferenceEntityId = bookingId
            }, cancellationToken);
        }

        return await taskSuggestionRepository.GetByBookingAsync(bookingId, cancellationToken);
    }

    public async Task<IReadOnlyList<OperationalTaskSuggestion>> RegenerateSuggestedTasksAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        await taskSuggestionRepository.DeletePendingByBookingAsync(bookingId, cancellationToken);
        return await GenerateSuggestedTasksAsync(bookingId, cancellationToken);
    }

    public async Task<OperationalTask> AcceptSuggestedTaskAsync(Guid suggestionId, string assignedToUserId, CancellationToken cancellationToken = default)
    {
        var suggestion = await taskSuggestionRepository.GetByIdAsync(suggestionId, cancellationToken)
            ?? throw new InvalidOperationException("Suggested task not found.");

        if (suggestion.State != TaskSuggestionState.PendingReview)
        {
            throw new InvalidOperationException("Only pending suggested tasks can be accepted.");
        }

        var createdTask = await taskService.CreateTaskAsync(
            suggestion.BookingId,
            suggestion.BookingItemId,
            suggestion.Title,
            $"{suggestion.Description}\n\nAI reason: {suggestion.Reason}",
            suggestion.SuggestedDueDate,
            assignedToUserId,
            cancellationToken);

        foreach (var nextStatus in GetTransitionPath(suggestion.SuggestedStatus))
        {
            createdTask = await taskService.UpdateTaskStatusAsync(createdTask.Id, nextStatus, cancellationToken);
        }

        suggestion.State = TaskSuggestionState.Accepted;
        suggestion.AcceptedTaskId = createdTask.Id;
        suggestion.ReviewedByUserId = currentUserContext.GetRequiredUserId();
        suggestion.ReviewedAt = DateTime.UtcNow;

        await taskSuggestionRepository.UpdateAsync(suggestion, cancellationToken);
        return createdTask;
    }

    public async Task<OperationalTaskSuggestion> RejectSuggestedTaskAsync(Guid suggestionId, CancellationToken cancellationToken = default)
    {
        var suggestion = await taskSuggestionRepository.GetByIdAsync(suggestionId, cancellationToken)
            ?? throw new InvalidOperationException("Suggested task not found.");

        if (suggestion.State != TaskSuggestionState.PendingReview)
        {
            throw new InvalidOperationException("Only pending suggested tasks can be rejected.");
        }

        suggestion.State = TaskSuggestionState.Rejected;
        suggestion.ReviewedByUserId = currentUserContext.GetRequiredUserId();
        suggestion.ReviewedAt = DateTime.UtcNow;
        await taskSuggestionRepository.UpdateAsync(suggestion, cancellationToken);
        return suggestion;
    }

    private static decimal ClampConfidence(decimal confidence) => Math.Min(1m, Math.Max(0m, confidence));

    private static IReadOnlyList<OperationalTaskStatus> GetTransitionPath(OperationalTaskStatus targetStatus) => targetStatus switch
    {
        OperationalTaskStatus.ToDo => [],
        OperationalTaskStatus.Waiting => [OperationalTaskStatus.Waiting],
        OperationalTaskStatus.FollowUp => [OperationalTaskStatus.Waiting, OperationalTaskStatus.FollowUp],
        OperationalTaskStatus.Blocked => [OperationalTaskStatus.Blocked],
        OperationalTaskStatus.Done => [OperationalTaskStatus.Done],
        _ => []
    };

    private static string NormalizeRequired(string? value, string message)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(message);
        }

        return normalized;
    }
}
