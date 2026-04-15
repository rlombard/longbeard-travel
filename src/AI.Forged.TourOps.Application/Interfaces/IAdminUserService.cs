using AI.Forged.TourOps.Application.Models.AdminUsers;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserListItemModel>> SearchUsersAsync(AdminUserSearchQueryModel query, CancellationToken cancellationToken = default);
    Task<AdminUserModel?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<AdminUserCreateResultModel> CreateUserAsync(AdminUserCreateModel model, CancellationToken cancellationToken = default);
    Task<AdminUserModel> UpdateUserAsync(string userId, AdminUserUpdateModel model, CancellationToken cancellationToken = default);
    Task<AdminUserResetPasswordResultModel> ResetPasswordAsync(string userId, AdminUserResetPasswordModel model, CancellationToken cancellationToken = default);
    Task<AdminAccessCatalogModel> GetAccessCatalogAsync(CancellationToken cancellationToken = default);
    Task<AdminUserModel> UpdateRolesAsync(string userId, AdminUserRoleUpdateModel model, CancellationToken cancellationToken = default);
}
