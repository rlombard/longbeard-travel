namespace AI.Forged.TourOps.Application.Models.Llm;

public sealed class LlmClassificationResult<T>
{
    public T Label { get; init; } = default!;
    public string Reason { get; init; } = string.Empty;
    public decimal Confidence { get; init; }
    public string Provider { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
}
