namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class GrokSettings
{
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
    public string Model { get; set; } = "grok-3-mini";
}
