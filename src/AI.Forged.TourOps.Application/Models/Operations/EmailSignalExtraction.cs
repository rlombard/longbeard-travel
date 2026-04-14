using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Models.Operations;

public sealed class EmailSignalExtraction
{
    public EmailClassificationType Classification { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public decimal Confidence { get; init; }
    public bool RequiresHumanReview { get; init; }
    public bool HasConfirmation { get; init; }
    public bool HasPricingChange { get; init; }
    public bool HasAvailabilityIssue { get; init; }
    public bool RequestsMoreInformation { get; init; }
    public string[] MissingInformationItems { get; init; } = [];
    public string[] RecommendedActions { get; init; } = [];
}
