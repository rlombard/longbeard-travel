using AI.Forged.TourOps.Application.Models.Platform;

namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface ITenantConfigurationService
{
    Task<IReadOnlyList<TenantConfigEntryModel>> GetAsync(Guid tenantId, string? configDomain, CancellationToken cancellationToken = default);
    Task<TenantConfigEntryModel> UpsertAsync(Guid tenantId, UpsertTenantConfigModel model, CancellationToken cancellationToken = default);
}
