namespace AI.Forged.TourOps.Application.Models.AdminUsers;

public sealed class AdminUserSearchQueryModel
{
    public string? SearchTerm { get; init; }
    public bool? Enabled { get; init; }
}

public sealed class AdminUserCreateModel
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool Enabled { get; init; } = true;
    public bool EmailVerified { get; init; }
    public string? TemporaryPassword { get; init; }
    public IReadOnlyList<string> RealmRoleNames { get; init; } = [];
    public IReadOnlyList<AdminClientRoleSelectionModel> ClientRoles { get; init; } = [];
}

public sealed class AdminUserUpdateModel
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool Enabled { get; init; } = true;
    public bool EmailVerified { get; init; }
}

public sealed class AdminUserResetPasswordModel
{
    public string? TemporaryPassword { get; init; }
}

public sealed class AdminUserRoleUpdateModel
{
    public IReadOnlyList<string> RealmRoleNames { get; init; } = [];
    public IReadOnlyList<AdminClientRoleSelectionModel> ClientRoles { get; init; } = [];
}

public sealed class AdminClientRoleSelectionModel
{
    public string ClientId { get; init; } = string.Empty;
    public IReadOnlyList<string> RoleNames { get; init; } = [];
}

public sealed class AdminUserListItemModel
{
    public string Id { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool Enabled { get; init; }
    public bool EmailVerified { get; init; }
    public DateTime? CreatedAt { get; init; }
}

public sealed class AdminUserModel
{
    public string Id { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool Enabled { get; init; }
    public bool EmailVerified { get; init; }
    public IReadOnlyList<string> RequiredActions { get; init; } = [];
    public IReadOnlyList<string> RealmRoleNames { get; init; } = [];
    public IReadOnlyList<AdminClientRoleSelectionModel> ClientRoles { get; init; } = [];
    public DateTime? CreatedAt { get; init; }
}

public sealed class AdminUserCreateResultModel
{
    public AdminUserModel User { get; init; } = new();
    public string TemporaryPassword { get; init; } = string.Empty;
}

public sealed class AdminUserResetPasswordResultModel
{
    public string UserId { get; init; } = string.Empty;
    public string TemporaryPassword { get; init; } = string.Empty;
}

public sealed class AdminRoleCatalogModel
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed class AdminClientRoleCatalogModel
{
    public string ClientId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public IReadOnlyList<AdminRoleCatalogModel> Roles { get; init; } = [];
}

public sealed class AdminAccessCatalogModel
{
    public IReadOnlyList<AdminRoleCatalogModel> RealmRoles { get; init; } = [];
    public IReadOnlyList<AdminClientRoleCatalogModel> ClientRoles { get; init; } = [];
}

public sealed class KeycloakAdminUserRecord
{
    public string Id { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool Enabled { get; init; }
    public bool EmailVerified { get; init; }
    public IReadOnlyList<string> RequiredActions { get; init; } = [];
    public DateTime? CreatedAt { get; init; }
}

public sealed class KeycloakAdminRoleCatalog
{
    public IReadOnlyList<KeycloakAdminRoleRecord> RealmRoles { get; init; } = [];
    public IReadOnlyList<KeycloakAdminClientRoleRecord> ClientRoles { get; init; } = [];
}

public sealed class KeycloakAdminRoleRecord
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed class KeycloakAdminClientRoleRecord
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientInternalId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public IReadOnlyList<KeycloakAdminRoleRecord> Roles { get; init; } = [];
}

public sealed class KeycloakAdminUserRoleAssignments
{
    public IReadOnlyList<string> RealmRoleNames { get; init; } = [];
    public IReadOnlyList<AdminClientRoleSelectionModel> ClientRoles { get; init; } = [];
}

public sealed class KeycloakAdminCreateUserInput
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool Enabled { get; init; }
    public bool EmailVerified { get; init; }
    public string TemporaryPassword { get; init; } = string.Empty;
}

public sealed class KeycloakAdminUpdateUserInput
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool Enabled { get; init; }
    public bool EmailVerified { get; init; }
}
