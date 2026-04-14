using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Api.Models;

public sealed class ProductAssistRequest
{
    public string? Destination { get; set; }
    public string? Region { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Season { get; set; }
    public int? TravellerCount { get; set; }
    public string? BudgetLevel { get; set; }
    public string? PreferredCurrency { get; set; }
    public string? TravelStyle { get; set; }
    public List<string> Interests { get; set; } = [];
    public string? AccommodationPreference { get; set; }
    public List<string> SpecialConstraints { get; set; } = [];
    public List<ProductType> ProductTypes { get; set; } = [];
    public string? CustomerBrief { get; set; }
    public int MaxResults { get; set; } = 10;
}

public sealed class ProductAssistResponse
{
    public int CandidateCount { get; set; }
    public int ReturnedCount { get; set; }
    public List<string> Assumptions { get; set; } = [];
    public List<ProductRecommendationResponse> Recommendations { get; set; } = [];
}

public sealed class ProductRecommendationResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public ProductType ProductType { get; set; }
    public decimal MatchScore { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = [];
    public List<string> AssumptionFlags { get; set; } = [];
    public List<string> MissingData { get; set; } = [];
}

public sealed class GenerateItineraryDraftRequestDto
{
    public string? Destination { get; set; }
    public string? Region { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? Duration { get; set; }
    public string? Season { get; set; }
    public int? TravellerCount { get; set; }
    public string? BudgetLevel { get; set; }
    public string? PreferredCurrency { get; set; }
    public string? TravelStyle { get; set; }
    public List<string> Interests { get; set; } = [];
    public string? AccommodationPreference { get; set; }
    public List<string> SpecialConstraints { get; set; } = [];
    public string? CustomerBrief { get; set; }
}

public sealed class ItineraryDraftResponse
{
    public Guid Id { get; set; }
    public ItineraryDraftStatus Status { get; set; }
    public DateOnly? ProposedStartDate { get; set; }
    public int Duration { get; set; }
    public string? CustomerBrief { get; set; }
    public string? LlmProvider { get; set; }
    public string? LlmModel { get; set; }
    public Guid? PersistedItineraryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public List<string> Assumptions { get; set; } = [];
    public List<string> Caveats { get; set; } = [];
    public List<string> DataGaps { get; set; } = [];
    public List<ItineraryDraftItemResponse> Items { get; set; } = [];
}

public sealed class ItineraryDraftItemResponse
{
    public Guid Id { get; set; }
    public int DayNumber { get; set; }
    public int Sequence { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? SupplierName { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public decimal Confidence { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsUnresolved { get; set; }
    public List<string> Warnings { get; set; } = [];
    public List<string> MissingData { get; set; } = [];
}

public sealed class ApproveItineraryDraftRequestDto
{
    public DateOnly? StartDate { get; set; }
    public int? Duration { get; set; }
    public string? DecisionNotes { get; set; }
    public List<ApproveItineraryDraftItemRequestDto> Items { get; set; } = [];
}

public sealed class ApproveItineraryDraftItemRequestDto
{
    public int DayNumber { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

public sealed class ItineraryDraftApprovalResponse
{
    public Guid DraftId { get; set; }
    public Guid ApprovalRequestId { get; set; }
    public DateTime ApprovedAt { get; set; }
    public ItineraryResponse Itinerary { get; set; } = new();
}
