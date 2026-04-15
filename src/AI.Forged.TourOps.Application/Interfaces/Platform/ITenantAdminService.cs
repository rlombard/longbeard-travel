using AI.Forged.TourOps.Application.Models.Platform;

namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface ITenantAdminService
{
    Task<TenantAdminWorkspaceModel> GetWorkspaceAsync(CancellationToken cancellationToken = default);
    Task<TenantAdminWorkspaceModel> UpdateWorkspaceProfileAsync(UpdateTenantWorkspaceProfileModel model, CancellationToken cancellationToken = default);
    Task<TenantConfigEntryModel> UpsertConfigAsync(UpsertTenantConfigModel model, CancellationToken cancellationToken = default);
    Task<TenantWorkspaceUserCreateResultModel> CreateUserAsync(CreateTenantWorkspaceUserModel model, CancellationToken cancellationToken = default);
    Task<TenantWorkspaceUserModel> UpdateUserAsync(string userId, UpdateTenantWorkspaceUserModel model, CancellationToken cancellationToken = default);
    Task<TenantWorkspaceUserPasswordResetResultModel> ResetPasswordAsync(string userId, ResetTenantWorkspaceUserPasswordModel model, CancellationToken cancellationToken = default);
}
