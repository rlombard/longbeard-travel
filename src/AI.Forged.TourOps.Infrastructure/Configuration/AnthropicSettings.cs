namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class AnthropicSettings
{
    public string? ApiKey { get; set; }
    public string Model { get; set; } = "claude-3-5-sonnet-latest";
    public string? BaseUrl { get; set; }
}
