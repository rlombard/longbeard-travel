using AI.Forged.TourOps.Application.Models.EmailIntegrations;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Models.Platform;

public sealed class TenantAdminWorkspaceModel
{
    public Guid TenantId { get; init; }
    public string TenantSlug { get; init; } = string.Empty;
    public string TenantName { get; init; } = string.Empty;
    public string? LegalName { get; init; }
    public string DefaultCurrency { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public string? BillingEmail { get; init; }
    public string? Notes { get; init; }
    public string RealmName { get; init; } = string.Empty;
    public IReadOnlyList<TenantConfigEntryModel> ConfigEntries { get; init; } = [];
    public IReadOnlyList<TenantWorkspaceUserModel> Users { get; init; } = [];
    public IReadOnlyList<EmailProviderConnectionListItemModel> EmailConnections { get; init; } = [];
}

public sealed class UpdateTenantWorkspaceProfileModel
{
    public string TenantName { get; init; } = string.Empty;
    public string? LegalName { get; init; }
    public string DefaultCurrency { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public string? BillingEmail { get; init; }
    public string? Notes { get; init; }
}

public sealed class TenantWorkspaceUserModel
{
    public Guid MembershipId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public TenantUserRole Role { get; init; }
    public TenantUserStatus Status { get; init; }
    public bool Enabled { get; init; }
    public DateTime InvitedAt { get; init; }
    public DateTime? LastSeenAt { get; init; }
}

public sealed class CreateTenantWorkspaceUserModel
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public TenantUserRole Role { get; init; } = TenantUserRole.Operator;
    public string? TemporaryPassword { get; init; }
}

public sealed class TenantWorkspaceUserCreateResultModel
{
    public TenantWorkspaceUserModel User { get; init; } = new();
    public string TemporaryPassword { get; init; } = string.Empty;
}

public sealed class UpdateTenantWorkspaceUserModel
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public TenantUserRole Role { get; init; } = TenantUserRole.Operator;
    public bool Enabled { get; init; } = true;
}

public sealed class ResetTenantWorkspaceUserPasswordModel
{
    public string? TemporaryPassword { get; init; }
}

public sealed class TenantWorkspaceUserPasswordResetResultModel
{
    public string UserId { get; init; } = string.Empty;
    public string TemporaryPassword { get; init; } = string.Empty;
}
