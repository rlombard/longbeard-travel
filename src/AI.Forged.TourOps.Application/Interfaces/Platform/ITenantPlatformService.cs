using AI.Forged.TourOps.Application.Models.Platform;

namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface ITenantPlatformService
{
    Task<IReadOnlyList<TenantSummaryModel>> GetTenantsAsync(CancellationToken cancellationToken = default);
    Task<TenantDetailModel?> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantDetailModel> CreateTenantAsync(CreateTenantModel model, CancellationToken cancellationToken = default);
    Task<TenantOnboardingModel> UpdateOnboardingAsync(Guid tenantId, UpdateTenantOnboardingModel model, CancellationToken cancellationToken = default);
    Task<TenantConfigEntryModel> UpsertConfigAsync(Guid tenantId, UpsertTenantConfigModel model, CancellationToken cancellationToken = default);
    Task<TenantUserMembershipModel> AssignUserAsync(AssignTenantUserModel model, CancellationToken cancellationToken = default);
}
