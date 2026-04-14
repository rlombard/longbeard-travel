namespace AI.Forged.TourOps.Application.Models.Llm;

public sealed class LlmRequest
{
    public string Category { get; init; } = string.Empty;
    public string Operation { get; init; } = string.Empty;
    public string SystemInstruction { get; init; } = string.Empty;
    public string Prompt { get; init; } = string.Empty;
    public string? Model { get; init; }
    public double? Temperature { get; init; }
    public int? MaxTokens { get; init; }
    public bool PreferStructuredOutput { get; init; }
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
