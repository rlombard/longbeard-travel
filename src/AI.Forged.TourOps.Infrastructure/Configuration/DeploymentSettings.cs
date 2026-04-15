using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class DeploymentSettings
{
    public DeploymentMode Mode { get; set; } = DeploymentMode.Standalone;
}
