using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class EmailDraft
{
    public Guid Id { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public Guid? EmailThreadId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailDraftStatus Status { get; set; }
    public EmailDraftGeneratedBy GeneratedBy { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? LlmProvider { get; set; }
    public string? LlmModel { get; set; }
    public string? AuditMetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Booking? Booking { get; set; }
    public BookingItem? BookingItem { get; set; }
    public EmailThread? EmailThread { get; set; }
}
