using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class HumanApprovalRequest
{
    public Guid Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string RequestedByUserId { get; set; } = string.Empty;
    public string? ReviewedByUserId { get; set; }
    public HumanApprovalStatus Status { get; set; }
    public string? PayloadJson { get; set; }
    public string? DecisionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
