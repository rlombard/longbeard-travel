using System.Security.Cryptography;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models.AdminUsers;

namespace AI.Forged.TourOps.Application.Services;

public class AdminUserService(IKeycloakAdminRepository keycloakAdminRepository) : IAdminUserService
{
    public async Task<IReadOnlyList<AdminUserListItemModel>> SearchUsersAsync(AdminUserSearchQueryModel query, CancellationToken cancellationToken = default)
    {
        var users = await keycloakAdminRepository.SearchUsersAsync(query, cancellationToken);
        return users.Select(ToListItemModel).ToList();
    }

    public async Task<AdminUserModel?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var user = await keycloakAdminRepository.GetUserAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var assignments = await keycloakAdminRepository.GetUserRoleAssignmentsAsync(userId, cancellationToken);
        return ToModel(user, assignments);
    }

    public async Task<AdminUserCreateResultModel> CreateUserAsync(AdminUserCreateModel model, CancellationToken cancellationToken = default)
    {
        ValidateCreateModel(model);
        await ValidateAccessAssignmentsAsync(model.RealmRoleNames, model.ClientRoles, cancellationToken);

        var temporaryPassword = NormalizePassword(model.TemporaryPassword) ?? GenerateTemporaryPassword();
        var createdUserId = await keycloakAdminRepository.CreateUserAsync(new KeycloakAdminCreateUserInput
        {
            Username = model.Username.Trim(),
            Email = model.Email.Trim(),
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Enabled = model.Enabled,
            EmailVerified = model.EmailVerified,
            TemporaryPassword = temporaryPassword
        }, cancellationToken);

        await keycloakAdminRepository.ReplaceUserRoleAssignmentsAsync(createdUserId, new AdminUserRoleUpdateModel
        {
            RealmRoleNames = NormalizeValues(model.RealmRoleNames),
            ClientRoles = NormalizeClientRoles(model.ClientRoles)
        }, cancellationToken);

        var user = await GetUserAsync(createdUserId, cancellationToken)
            ?? throw new InvalidOperationException("Created user could not be loaded.");

        return new AdminUserCreateResultModel
        {
            User = user,
            TemporaryPassword = temporaryPassword
        };
    }

    public async Task<AdminUserModel> UpdateUserAsync(string userId, AdminUserUpdateModel model, CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateUpdateModel(model);

        await keycloakAdminRepository.UpdateUserAsync(userId, new KeycloakAdminUpdateUserInput
        {
            Username = model.Username.Trim(),
            Email = model.Email.Trim(),
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Enabled = model.Enabled,
            EmailVerified = model.EmailVerified
        }, cancellationToken);

        return await GetUserAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Updated user could not be loaded.");
    }

    public async Task<AdminUserResetPasswordResultModel> ResetPasswordAsync(string userId, AdminUserResetPasswordModel model, CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var temporaryPassword = NormalizePassword(model.TemporaryPassword) ?? GenerateTemporaryPassword();
        await keycloakAdminRepository.ResetTemporaryPasswordAsync(userId, temporaryPassword, cancellationToken);

        return new AdminUserResetPasswordResultModel
        {
            UserId = userId,
            TemporaryPassword = temporaryPassword
        };
    }

    public async Task<AdminAccessCatalogModel> GetAccessCatalogAsync(CancellationToken cancellationToken = default)
    {
        var catalog = await keycloakAdminRepository.GetAccessCatalogAsync(cancellationToken);

        return new AdminAccessCatalogModel
        {
            RealmRoles = catalog.RealmRoles
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => new AdminRoleCatalogModel
                {
                    Name = x.Name,
                    Description = x.Description
                })
                .ToList(),
            ClientRoles = catalog.ClientRoles
                .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Select(x => new AdminClientRoleCatalogModel
                {
                    ClientId = x.ClientId,
                    DisplayName = x.DisplayName,
                    Roles = x.Roles
                        .OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                        .Select(r => new AdminRoleCatalogModel
                        {
                            Name = r.Name,
                            Description = r.Description
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    public async Task<AdminUserModel> UpdateRolesAsync(string userId, AdminUserRoleUpdateModel model, CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        await ValidateAccessAssignmentsAsync(model.RealmRoleNames, model.ClientRoles, cancellationToken);

        await keycloakAdminRepository.ReplaceUserRoleAssignmentsAsync(userId, new AdminUserRoleUpdateModel
        {
            RealmRoleNames = NormalizeValues(model.RealmRoleNames),
            ClientRoles = NormalizeClientRoles(model.ClientRoles)
        }, cancellationToken);

        return await GetUserAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Updated role assignments could not be loaded.");
    }

    private static AdminUserListItemModel ToListItemModel(KeycloakAdminUserRecord user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Enabled = user.Enabled,
        EmailVerified = user.EmailVerified,
        CreatedAt = user.CreatedAt
    };

    private static AdminUserModel ToModel(KeycloakAdminUserRecord user, KeycloakAdminUserRoleAssignments assignments) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Enabled = user.Enabled,
        EmailVerified = user.EmailVerified,
        RequiredActions = user.RequiredActions,
        RealmRoleNames = assignments.RealmRoleNames.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList(),
        ClientRoles = assignments.ClientRoles
            .OrderBy(x => x.ClientId, StringComparer.OrdinalIgnoreCase)
            .Select(x => new AdminClientRoleSelectionModel
            {
                ClientId = x.ClientId,
                RoleNames = x.RoleNames.OrderBy(y => y, StringComparer.OrdinalIgnoreCase).ToList()
            })
            .ToList(),
        CreatedAt = user.CreatedAt
    };

    private static IReadOnlyList<string> NormalizeValues(IReadOnlyList<string> values) =>
        values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static IReadOnlyList<AdminClientRoleSelectionModel> NormalizeClientRoles(IReadOnlyList<AdminClientRoleSelectionModel> clientRoles) =>
        clientRoles
            .Where(x => !string.IsNullOrWhiteSpace(x.ClientId))
            .Select(x => new AdminClientRoleSelectionModel
            {
                ClientId = x.ClientId.Trim(),
                RoleNames = NormalizeValues(x.RoleNames)
            })
            .Where(x => x.RoleNames.Count > 0)
            .ToList();

    private static string? NormalizePassword(string? password) =>
        string.IsNullOrWhiteSpace(password) ? null : password.Trim();

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("User id is required.");
        }
    }

    private static void ValidateCreateModel(AdminUserCreateModel model)
    {
        ValidateCoreFields(model.Username, model.Email, model.FirstName, model.LastName);
    }

    private static void ValidateUpdateModel(AdminUserUpdateModel model)
    {
        ValidateCoreFields(model.Username, model.Email, model.FirstName, model.LastName);
    }

    private static void ValidateCoreFields(string username, string email, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        if (!email.Contains('@', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Email format is invalid.");
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new InvalidOperationException("First name is required.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new InvalidOperationException("Last name is required.");
        }
    }

    private static string GenerateTemporaryPassword()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@$%";
        Span<byte> bytes = stackalloc byte[14];
        RandomNumberGenerator.Fill(bytes);
        var chars = new char[bytes.Length];
        for (var i = 0; i < bytes.Length; i++)
        {
            chars[i] = alphabet[bytes[i] % alphabet.Length];
        }

        return new string(chars);
    }

    private async Task ValidateAccessAssignmentsAsync(IReadOnlyList<string> realmRoleNames, IReadOnlyList<AdminClientRoleSelectionModel> clientRoles, CancellationToken cancellationToken)
    {
        var catalog = await keycloakAdminRepository.GetAccessCatalogAsync(cancellationToken);
        var normalizedRealmRoles = NormalizeValues(realmRoleNames);
        var normalizedClientRoles = NormalizeClientRoles(clientRoles);

        var missingRealmRoles = normalizedRealmRoles
            .Where(requestedRole => !catalog.RealmRoles.Any(role => string.Equals(role.Name, requestedRole, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (missingRealmRoles.Count > 0)
        {
            throw new InvalidOperationException($"Unknown realm roles requested: {string.Join(", ", missingRealmRoles)}.");
        }

        foreach (var clientRoleSelection in normalizedClientRoles)
        {
            var client = catalog.ClientRoles.FirstOrDefault(x => string.Equals(x.ClientId, clientRoleSelection.ClientId, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Unknown client role source requested: {clientRoleSelection.ClientId}.");

            var missingClientRoles = clientRoleSelection.RoleNames
                .Where(requestedRole => !client.Roles.Any(role => string.Equals(role.Name, requestedRole, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (missingClientRoles.Count > 0)
            {
                throw new InvalidOperationException($"Unknown client roles requested for {client.ClientId}: {string.Join(", ", missingClientRoles)}.");
            }
        }
    }
}
