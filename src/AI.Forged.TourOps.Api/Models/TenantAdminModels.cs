using System.Text.Json.Serialization;
using AI.Forged.TourOps.Application.Models.EmailIntegrations;
using AI.Forged.TourOps.Application.Models.Platform;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Api.Models;

public sealed class TenantAdminWorkspaceResponse
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
    public IReadOnlyList<TenantConfigEntryResponse> ConfigEntries { get; init; } = [];
    public IReadOnlyList<TenantWorkspaceUserResponse> Users { get; init; } = [];
    public IReadOnlyList<EmailProviderConnectionListItemResponse> EmailConnections { get; init; } = [];
}

public sealed class UpdateTenantWorkspaceProfileRequest
{
    public string TenantName { get; init; } = string.Empty;
    public string? LegalName { get; init; }
    public string DefaultCurrency { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public string? BillingEmail { get; init; }
    public string? Notes { get; init; }
}

public sealed class TenantWorkspaceUserResponse
{
    public Guid MembershipId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TenantUserRole Role { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TenantUserStatus Status { get; init; }
    public bool Enabled { get; init; }
    public DateTime InvitedAt { get; init; }
    public DateTime? LastSeenAt { get; init; }
}

public sealed class CreateTenantWorkspaceUserRequest
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TenantUserRole Role { get; init; } = TenantUserRole.Operator;
    public string? TemporaryPassword { get; init; }
}

public sealed class TenantWorkspaceUserCreateResponse
{
    public TenantWorkspaceUserResponse User { get; init; } = new();
    public string TemporaryPassword { get; init; } = string.Empty;
}

public sealed class UpdateTenantWorkspaceUserRequest
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TenantUserRole Role { get; init; } = TenantUserRole.Operator;
    public bool Enabled { get; init; } = true;
}

public sealed class ResetTenantWorkspaceUserPasswordRequest
{
    public string? TemporaryPassword { get; init; }
}

public sealed class TenantWorkspaceUserPasswordResetResponse
{
    public string UserId { get; init; } = string.Empty;
    public string TemporaryPassword { get; init; } = string.Empty;
}

public static class TenantAdminMappings
{
    public static CreateTenantWorkspaceUserModel ToModel(this CreateTenantWorkspaceUserRequest request) => new()
    {
        Username = request.Username,
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName,
        Role = request.Role,
        TemporaryPassword = request.TemporaryPassword
    };

    public static UpdateTenantWorkspaceProfileModel ToModel(this UpdateTenantWorkspaceProfileRequest request) => new()
    {
        TenantName = request.TenantName,
        LegalName = request.LegalName,
        DefaultCurrency = request.DefaultCurrency,
        TimeZone = request.TimeZone,
        BillingEmail = request.BillingEmail,
        Notes = request.Notes
    };

    public static UpdateTenantWorkspaceUserModel ToModel(this UpdateTenantWorkspaceUserRequest request) => new()
    {
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName,
        Role = request.Role,
        Enabled = request.Enabled
    };

    public static ResetTenantWorkspaceUserPasswordModel ToModel(this ResetTenantWorkspaceUserPasswordRequest request) => new()
    {
        TemporaryPassword = request.TemporaryPassword
    };

    public static TenantAdminWorkspaceResponse ToResponse(this TenantAdminWorkspaceModel model) => new()
    {
        TenantId = model.TenantId,
        TenantSlug = model.TenantSlug,
        TenantName = model.TenantName,
        LegalName = model.LegalName,
        DefaultCurrency = model.DefaultCurrency,
        TimeZone = model.TimeZone,
        BillingEmail = model.BillingEmail,
        Notes = model.Notes,
        RealmName = model.RealmName,
        ConfigEntries = model.ConfigEntries.Select(x => x.ToResponse()).ToList(),
        Users = model.Users.Select(x => x.ToResponse()).ToList(),
        EmailConnections = model.EmailConnections.Select(ToEmailConnectionResponse).ToList()
    };

    public static TenantWorkspaceUserResponse ToResponse(this TenantWorkspaceUserModel model) => new()
    {
        MembershipId = model.MembershipId,
        UserId = model.UserId,
        Username = model.Username,
        Email = model.Email,
        FirstName = model.FirstName,
        LastName = model.LastName,
        DisplayName = model.DisplayName,
        Role = model.Role,
        Status = model.Status,
        Enabled = model.Enabled,
        InvitedAt = model.InvitedAt,
        LastSeenAt = model.LastSeenAt
    };

    public static TenantWorkspaceUserCreateResponse ToResponse(this TenantWorkspaceUserCreateResultModel model) => new()
    {
        User = model.User.ToResponse(),
        TemporaryPassword = model.TemporaryPassword
    };

    public static TenantWorkspaceUserPasswordResetResponse ToResponse(this TenantWorkspaceUserPasswordResetResultModel model) => new()
    {
        UserId = model.UserId,
        TemporaryPassword = model.TemporaryPassword
    };

    private static EmailProviderConnectionListItemResponse ToEmailConnectionResponse(EmailProviderConnectionListItemModel model) => new()
    {
        Id = model.Id,
        ConnectionName = model.ConnectionName,
        ProviderType = model.ProviderType,
        AuthMethod = model.AuthMethod,
        Status = model.Status,
        MailboxAddress = model.MailboxAddress,
        DisplayName = model.DisplayName,
        AllowSend = model.AllowSend,
        AllowSync = model.AllowSync,
        IsDefaultConnection = model.IsDefaultConnection,
        LastSyncedAt = model.LastSyncedAt,
        LastSuccessfulSendAt = model.LastSuccessfulSendAt,
        LastTestedAt = model.LastTestedAt,
        LastError = model.LastError,
        CreatedAt = model.CreatedAt,
        UpdatedAt = model.UpdatedAt
    };
}
