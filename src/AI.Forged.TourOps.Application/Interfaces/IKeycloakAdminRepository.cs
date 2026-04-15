using AI.Forged.TourOps.Application.Models.AdminUsers;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IKeycloakAdminRepository
{
    Task<IReadOnlyList<KeycloakAdminUserRecord>> SearchUsersAsync(AdminUserSearchQueryModel query, CancellationToken cancellationToken = default);
    Task<KeycloakAdminUserRecord?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<string> CreateUserAsync(KeycloakAdminCreateUserInput input, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(string userId, KeycloakAdminUpdateUserInput input, CancellationToken cancellationToken = default);
    Task ResetTemporaryPasswordAsync(string userId, string temporaryPassword, CancellationToken cancellationToken = default);
    Task<KeycloakAdminRoleCatalog> GetAccessCatalogAsync(CancellationToken cancellationToken = default);
    Task<KeycloakAdminUserRoleAssignments> GetUserRoleAssignmentsAsync(string userId, CancellationToken cancellationToken = default);
    Task ReplaceUserRoleAssignmentsAsync(string userId, AdminUserRoleUpdateModel model, CancellationToken cancellationToken = default);
}
