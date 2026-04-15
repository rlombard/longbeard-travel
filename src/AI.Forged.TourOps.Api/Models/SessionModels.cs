using AI.Forged.TourOps.Application.Models.Platform;
using AI.Forged.TourOps.Domain.Enums;
using System.Text.Json.Serialization;

namespace AI.Forged.TourOps.Api.Models;

public sealed class SessionBootstrapResponse
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DeploymentMode DeploymentMode { get; init; }
    public bool PlatformManagementEnabled { get; init; }
    public bool PublicSignupEnabled { get; init; }
    public string PublicSignupDisabledReason { get; init; } = string.Empty;
    public AuthTargetResponse ManagementAuth { get; init; } = new();
    public AuthTargetResponse? StandaloneTenantAuth { get; init; }
    public SessionActorResponse? Session { get; init; }
}

public sealed class AuthTargetResponse
{
    public string KeycloakUrl { get; init; } = string.Empty;
    public string Realm { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
}

public sealed class SessionActorResponse
{
    public bool IsAuthenticated { get; init; }
    public bool IsPlatformAdmin { get; init; }
    public string? UserId { get; init; }
    public string? DisplayName { get; init; }
    public string? Email { get; init; }
    public Guid? CurrentTenantId { get; init; }
    public string? CurrentTenantSlug { get; init; }
    public string? CurrentTenantName { get; init; }
    public string HomeArea { get; init; } = "/app";
    public IReadOnlyList<SessionTenantMembershipResponse> Memberships { get; init; } = [];
}

public sealed class SessionTenantMembershipResponse
{
    public Guid TenantId { get; init; }
    public string TenantSlug { get; init; } = string.Empty;
    public string TenantName { get; init; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TenantUserRole Role { get; init; }
    public string RealmName { get; init; } = string.Empty;
}

public sealed class DiscoverTenantRequest
{
    public string? Email { get; init; }
    public string? TenantSlug { get; init; }
}

public sealed class TenantLoginDiscoveryResponse
{
    public bool Found { get; init; }
    public Guid? TenantId { get; init; }
    public string? TenantSlug { get; init; }
    public string? TenantName { get; init; }
    public string ResolutionSource { get; init; } = string.Empty;
    public AuthTargetResponse? Auth { get; init; }
}

public static class SessionMappings
{
    public static DiscoverTenantLoginModel ToModel(this DiscoverTenantRequest request) => new()
    {
        Email = request.Email,
        TenantSlug = request.TenantSlug
    };

    public static SessionBootstrapResponse ToResponse(this SessionBootstrapModel model) => new()
    {
        DeploymentMode = model.DeploymentMode,
        PlatformManagementEnabled = model.PlatformManagementEnabled,
        PublicSignupEnabled = model.PublicSignupEnabled,
        PublicSignupDisabledReason = model.PublicSignupDisabledReason,
        ManagementAuth = model.ManagementAuth.ToResponse(),
        StandaloneTenantAuth = model.StandaloneTenantAuth?.ToResponse(),
        Session = model.Session?.ToResponse()
    };

    public static AuthTargetResponse ToResponse(this AuthTargetModel model) => new()
    {
        KeycloakUrl = model.KeycloakUrl,
        Realm = model.Realm,
        ClientId = model.ClientId
    };

    public static SessionActorResponse ToResponse(this SessionActorModel model) => new()
    {
        IsAuthenticated = model.IsAuthenticated,
        IsPlatformAdmin = model.IsPlatformAdmin,
        UserId = model.UserId,
        DisplayName = model.DisplayName,
        Email = model.Email,
        CurrentTenantId = model.CurrentTenantId,
        CurrentTenantSlug = model.CurrentTenantSlug,
        CurrentTenantName = model.CurrentTenantName,
        HomeArea = model.HomeArea,
        Memberships = model.Memberships.Select(x => x.ToResponse()).ToList()
    };

    public static SessionTenantMembershipResponse ToResponse(this SessionTenantMembershipModel model) => new()
    {
        TenantId = model.TenantId,
        TenantSlug = model.TenantSlug,
        TenantName = model.TenantName,
        Role = model.Role,
        RealmName = model.RealmName
    };

    public static TenantLoginDiscoveryResponse ToResponse(this TenantLoginDiscoveryModel model) => new()
    {
        Found = model.Found,
        TenantId = model.TenantId,
        TenantSlug = model.TenantSlug,
        TenantName = model.TenantName,
        ResolutionSource = model.ResolutionSource,
        Auth = model.Auth?.ToResponse()
    };
}
