using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Platform;

public sealed class TenantExecutionContextAccessor(IOptions<DeploymentSettings> deploymentSettings) : ITenantExecutionContextAccessor
{
    private static readonly AsyncLocal<TenantExecutionState?> Current = new();

    public Guid? CurrentTenantId => Current.Value?.TenantId;
    public DeploymentMode DeploymentMode => deploymentSettings.Value.Mode;
    public bool IsPlatformScope => Current.Value?.IsPlatformScope == true;

    public IDisposable BeginTenantScope(Guid tenantId)
    {
        var previous = Current.Value;
        Current.Value = new TenantExecutionState
        {
            TenantId = tenantId,
            IsPlatformScope = false
        };

        return new Scope(() => Current.Value = previous);
    }

    public IDisposable BeginPlatformScope()
    {
        var previous = Current.Value;
        Current.Value = new TenantExecutionState
        {
            TenantId = null,
            IsPlatformScope = true
        };

        return new Scope(() => Current.Value = previous);
    }

    private sealed class Scope(Action close) : IDisposable
    {
        public void Dispose() => close();
    }

    private sealed class TenantExecutionState
    {
        public Guid? TenantId { get; init; }
        public bool IsPlatformScope { get; init; }
    }
}
