namespace AI.Forged.TourOps.Application.Models.Operations;

public sealed class BookingStateAnalysis
{
    public string Summary { get; init; } = string.Empty;
    public string[] Risks { get; init; } = [];
    public string[] RecommendedActions { get; init; } = [];
    public decimal Confidence { get; init; }
    public bool RequiresHumanReview { get; init; }
}
