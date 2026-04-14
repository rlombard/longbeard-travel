namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class OpenAiSettings
{
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
    public string Model { get; set; } = "gpt-4.1-mini";
}
