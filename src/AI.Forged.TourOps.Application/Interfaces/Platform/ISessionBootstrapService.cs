using AI.Forged.TourOps.Application.Models.Platform;

namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface ISessionBootstrapService
{
    Task<SessionBootstrapModel> GetBootstrapAsync(CancellationToken cancellationToken = default);
    Task<TenantLoginDiscoveryModel> DiscoverTenantAsync(DiscoverTenantLoginModel model, CancellationToken cancellationToken = default);
}
