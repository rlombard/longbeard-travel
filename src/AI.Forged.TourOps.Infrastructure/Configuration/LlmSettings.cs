namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class LlmSettings
{
    public string DefaultProvider { get; set; } = "Deterministic";
    public string DefaultModel { get; set; } = "ops-assist-v1";
    public bool EnableStructuredOutputs { get; set; } = true;
    public int RequestTimeoutSeconds { get; set; } = 30;
    public bool EnablePromptLogging { get; set; }
    public bool EnableResponseLogging { get; set; }
}
