using AI.Forged.TourOps.Application.Models.Operations;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Models.Ai;

public sealed class AiForgedProcessingResult
{
    public IReadOnlyList<SuggestedTaskCandidate> Tasks { get; init; } = [];
    public EmailClassificationType EmailClassification { get; init; } = EmailClassificationType.Unclear;
    public string Summary { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public decimal Confidence { get; init; }
    public bool RequiresHumanReview { get; init; }
    public IReadOnlyList<string> MissingInformationItems { get; init; } = [];
    public IReadOnlyList<string> RecommendedActions { get; init; } = [];
    public AiForgedSuggestedReply? SuggestedReply { get; init; }
    public string Provider { get; init; } = "AiForgedStub";
    public string ProcessingMode { get; init; } = "PdfIngestion";
}
