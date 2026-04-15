using AI.Forged.TourOps.Application.Models.AdminUsers;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IKeycloakRealmAdminRepository
{
    Task EnsureRealmAsync(
        string realmName,
        string displayName,
        string clientId,
        string frontendRootUrl,
        IReadOnlyList<string> redirectUris,
        IReadOnlyList<string> webOrigins,
        CancellationToken cancellationToken = default);

    Task<KeycloakAdminUserRecord?> FindUserByUsernameAsync(
        string realmName,
        string username,
        CancellationToken cancellationToken = default);

    Task<string> CreateUserAsync(
        string realmName,
        KeycloakAdminCreateUserInput input,
        IReadOnlyList<string> realmRoleNames,
        CancellationToken cancellationToken = default);

    Task UpdateUserAsync(
        string realmName,
        string userId,
        KeycloakAdminUpdateUserInput input,
        CancellationToken cancellationToken = default);

    Task ResetTemporaryPasswordAsync(
        string realmName,
        string userId,
        string temporaryPassword,
        CancellationToken cancellationToken = default);

    Task SetPasswordAsync(
        string realmName,
        string userId,
        string password,
        bool requirePasswordChange,
        CancellationToken cancellationToken = default);

    Task ReplaceRealmRolesAsync(
        string realmName,
        string userId,
        IReadOnlyList<string> realmRoleNames,
        CancellationToken cancellationToken = default);
}
