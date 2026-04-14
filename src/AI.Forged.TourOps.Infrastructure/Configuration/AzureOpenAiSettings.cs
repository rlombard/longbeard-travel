namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class AzureOpenAiSettings
{
    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }
    public string? DeploymentName { get; set; }
    public string ApiVersion { get; set; } = "2024-10-21";
}
