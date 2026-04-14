namespace AI.Forged.TourOps.Application.Models.Llm;

public sealed class LlmProviderResponse
{
    public string Content { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string? FinishReason { get; init; }
}
