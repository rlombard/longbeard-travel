namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class AiForgedSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
