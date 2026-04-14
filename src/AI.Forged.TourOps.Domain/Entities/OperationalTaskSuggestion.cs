using AI.Forged.TourOps.Domain.Enums;
using OperationalTaskStatus = AI.Forged.TourOps.Domain.Enums.TaskStatus;

namespace AI.Forged.TourOps.Domain.Entities;

public class OperationalTaskSuggestion
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OperationalTaskStatus SuggestedStatus { get; set; }
    public DateTime? SuggestedDueDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public bool RequiresHumanReview { get; set; }
    public TaskSuggestionState State { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? LlmProvider { get; set; }
    public string? LlmModel { get; set; }
    public string? AuditMetadataJson { get; set; }
    public Guid? AcceptedTaskId { get; set; }
    public string? ReviewedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public Booking Booking { get; set; } = null!;
    public BookingItem? BookingItem { get; set; }
    public OperationalTask? AcceptedTask { get; set; }
}
