using AI.Forged.TourOps.Domain.Enums;
using OperationalTaskStatus = AI.Forged.TourOps.Domain.Enums.TaskStatus;

namespace AI.Forged.TourOps.Application.Models.Operations;

public sealed class SuggestedTaskCandidate
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public OperationalTaskStatus SuggestedStatus { get; init; }
    public DateTime? SuggestedDueDate { get; init; }
    public Guid BookingId { get; init; }
    public Guid? BookingItemId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public decimal Confidence { get; init; }
    public bool RequiresHumanReview { get; init; }
}
