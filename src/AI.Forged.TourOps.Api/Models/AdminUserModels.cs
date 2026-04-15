namespace AI.Forged.TourOps.Api.Models;

public sealed class AdminUserListResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class AdminUserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public bool EmailVerified { get; set; }
    public List<string> RequiredActions { get; set; } = [];
    public List<string> RealmRoleNames { get; set; } = [];
    public List<AdminClientRoleSelectionResponse> ClientRoles { get; set; } = [];
    public DateTime? CreatedAt { get; set; }
}

public sealed class AdminUserCreateRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool EmailVerified { get; set; }
    public string? TemporaryPassword { get; set; }
    public List<string> RealmRoleNames { get; set; } = [];
    public List<AdminClientRoleSelectionRequest> ClientRoles { get; set; } = [];
}

public sealed class AdminUserUpdateRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool EmailVerified { get; set; }
}

public sealed class AdminUserRoleUpdateRequest
{
    public List<string> RealmRoleNames { get; set; } = [];
    public List<AdminClientRoleSelectionRequest> ClientRoles { get; set; } = [];
}

public sealed class AdminClientRoleSelectionRequest
{
    public string ClientId { get; set; } = string.Empty;
    public List<string> RoleNames { get; set; } = [];
}

public sealed class AdminClientRoleSelectionResponse
{
    public string ClientId { get; set; } = string.Empty;
    public List<string> RoleNames { get; set; } = [];
}

public sealed class AdminUserCreateResponse
{
    public AdminUserResponse User { get; set; } = new();
    public string TemporaryPassword { get; set; } = string.Empty;
}

public sealed class AdminResetPasswordRequest
{
    public string? TemporaryPassword { get; set; }
}

public sealed class AdminResetPasswordResponse
{
    public string UserId { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
}

public sealed class AdminAccessCatalogResponse
{
    public List<AdminRoleCatalogResponse> RealmRoles { get; set; } = [];
    public List<AdminClientRoleCatalogResponse> ClientRoles { get; set; } = [];
}

public sealed class AdminRoleCatalogResponse
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class AdminClientRoleCatalogResponse
{
    public string ClientId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<AdminRoleCatalogResponse> Roles { get; set; } = [];
}
