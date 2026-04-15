using AI.Forged.TourOps.Application.Models.Platform;

namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface IKeycloakProvisioningService
{
    Task<TenantIdentityModel> EnsureTenantIdentityAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
