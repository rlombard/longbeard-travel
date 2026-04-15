using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface ITenantExecutionContextAccessor
{
    Guid? CurrentTenantId { get; }
    DeploymentMode DeploymentMode { get; }
    bool IsPlatformScope { get; }
    IDisposable BeginTenantScope(Guid tenantId);
    IDisposable BeginPlatformScope();
}
