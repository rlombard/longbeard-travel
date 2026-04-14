using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Models.Itineraries;

public sealed class CreateItineraryModel
{
    public DateOnly StartDate { get; init; }
    public int Duration { get; init; }
    public IReadOnlyList<CreateItineraryItemModel> Items { get; init; } = [];
}

public sealed class CreateItineraryItemModel
{
    public int DayNumber { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public string? Notes { get; init; }
}

public sealed class ItineraryModel
{
    public Guid Id { get; init; }
    public Guid? LeadCustomerId { get; init; }
    public string? LeadCustomerName { get; init; }
    public DateOnly StartDate { get; init; }
    public int Duration { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<ItineraryItemModel> Items { get; init; } = [];
}

public sealed class ItineraryItemModel
{
    public Guid Id { get; init; }
    public int DayNumber { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public string? Notes { get; init; }
}

public sealed class ItineraryProductAssistRequest
{
    public string? Destination { get; init; }
    public string? Region { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Season { get; init; }
    public int? TravellerCount { get; init; }
    public string? BudgetLevel { get; init; }
    public string? PreferredCurrency { get; init; }
    public string? TravelStyle { get; init; }
    public IReadOnlyList<string> Interests { get; init; } = [];
    public string? AccommodationPreference { get; init; }
    public IReadOnlyList<string> SpecialConstraints { get; init; } = [];
    public IReadOnlyList<ProductType> ProductTypes { get; init; } = [];
    public string? CustomerBrief { get; init; }
    public int MaxResults { get; init; } = 10;
}

public sealed class ItineraryProductAssistResult
{
    public int CandidateCount { get; init; }
    public int ReturnedCount { get; init; }
    public IReadOnlyList<string> Assumptions { get; init; } = [];
    public IReadOnlyList<ItineraryProductRecommendationModel> Recommendations { get; init; } = [];
}

public sealed class ItineraryProductRecommendationModel
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string SupplierName { get; init; } = string.Empty;
    public ProductType ProductType { get; init; }
    public decimal MatchScore { get; init; }
    public string Reason { get; init; } = string.Empty;
    public IReadOnlyList<string> Warnings { get; init; } = [];
    public IReadOnlyList<string> AssumptionFlags { get; init; } = [];
    public IReadOnlyList<string> MissingData { get; init; } = [];
}

public sealed class GenerateItineraryDraftRequest
{
    public string? Destination { get; init; }
    public string? Region { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public int? Duration { get; init; }
    public string? Season { get; init; }
    public int? TravellerCount { get; init; }
    public string? BudgetLevel { get; init; }
    public string? PreferredCurrency { get; init; }
    public string? TravelStyle { get; init; }
    public IReadOnlyList<string> Interests { get; init; } = [];
    public string? AccommodationPreference { get; init; }
    public IReadOnlyList<string> SpecialConstraints { get; init; } = [];
    public string? CustomerBrief { get; init; }
}

public sealed class ItineraryDraftModel
{
    public Guid Id { get; init; }
    public ItineraryDraftStatus Status { get; init; }
    public DateOnly? ProposedStartDate { get; init; }
    public int Duration { get; init; }
    public string? CustomerBrief { get; init; }
    public string? LlmProvider { get; init; }
    public string? LlmModel { get; init; }
    public Guid? PersistedItineraryId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public IReadOnlyList<string> Assumptions { get; init; } = [];
    public IReadOnlyList<string> Caveats { get; init; } = [];
    public IReadOnlyList<string> DataGaps { get; init; } = [];
    public IReadOnlyList<ItineraryDraftItemModel> Items { get; init; } = [];
}

public sealed class ItineraryDraftItemModel
{
    public Guid Id { get; init; }
    public int DayNumber { get; init; }
    public int Sequence { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid? ProductId { get; init; }
    public string? ProductName { get; init; }
    public string? SupplierName { get; init; }
    public int Quantity { get; init; }
    public string? Notes { get; init; }
    public decimal Confidence { get; init; }
    public string Reason { get; init; } = string.Empty;
    public bool IsUnresolved { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = [];
    public IReadOnlyList<string> MissingData { get; init; } = [];
}

public sealed class ApproveItineraryDraftRequest
{
    public DateOnly? StartDate { get; init; }
    public int? Duration { get; init; }
    public string? DecisionNotes { get; init; }
    public IReadOnlyList<ApproveItineraryDraftItemModel> Items { get; init; } = [];
}

public sealed class ApproveItineraryDraftItemModel
{
    public int DayNumber { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public string? Notes { get; init; }
}

public sealed class ItineraryDraftApprovalResult
{
    public Guid DraftId { get; init; }
    public Guid ApprovalRequestId { get; init; }
    public DateTime ApprovedAt { get; init; }
    public ItineraryModel Itinerary { get; init; } = new();
}

public sealed class ProductCatalogFilter
{
    public IReadOnlyList<string> LocationTerms { get; init; } = [];
    public IReadOnlyList<string> SearchTerms { get; init; } = [];
    public IReadOnlyList<ProductType> ProductTypes { get; init; } = [];
    public int MaxResults { get; init; } = 40;
}
