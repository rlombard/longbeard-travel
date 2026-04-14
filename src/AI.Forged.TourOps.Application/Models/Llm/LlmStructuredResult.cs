namespace AI.Forged.TourOps.Application.Models.Llm;

public sealed class LlmStructuredResult<T>
{
    public T Data { get; init; } = default!;
    public string RawContent { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
}
