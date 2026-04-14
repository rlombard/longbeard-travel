namespace AI.Forged.TourOps.Application.Models.Llm;

public sealed class LlmDraftResult
{
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
}
