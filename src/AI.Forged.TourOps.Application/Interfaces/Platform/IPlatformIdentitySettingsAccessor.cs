namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface IPlatformIdentitySettingsAccessor
{
    string PublicKeycloakBaseUrl { get; }
    string ManagementRealm { get; }
    string ManagementClientId { get; }
    string TenantClientId { get; }
    string TenantRealmPrefix { get; }
}
