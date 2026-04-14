using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class ItineraryDraft
{
    public Guid Id { get; set; }
    public string RequestedByUserId { get; set; } = string.Empty;
    public DateOnly? ProposedStartDate { get; set; }
    public int Duration { get; set; }
    public string InputJson { get; set; } = string.Empty;
    public string? CustomerBrief { get; set; }
    public string? AssumptionsJson { get; set; }
    public string? CaveatsJson { get; set; }
    public string? DataGapsJson { get; set; }
    public string? LlmProvider { get; set; }
    public string? LlmModel { get; set; }
    public string? AuditMetadataJson { get; set; }
    public ItineraryDraftStatus Status { get; set; }
    public Guid? PersistedItineraryId { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Itinerary? PersistedItinerary { get; set; }
    public ICollection<ItineraryDraftItem> Items { get; set; } = new List<ItineraryDraftItem>();
}
