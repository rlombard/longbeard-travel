using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class KeycloakTenantProvisioningSettings
{
    public IdentityIsolationMode IsolationMode { get; set; } = IdentityIsolationMode.RealmPerTenant;
    public string PublicBaseUrl { get; set; } = "http://localhost:8080";
    public string ManagementRealm { get; set; } = "tourops";
    public string ManagementClientId { get; set; } = "frontend";
    public string ManagementIssuerUrl { get; set; } = "http://localhost:8080/realms/tourops";
    public string TenantClientId { get; set; } = "frontend";
    public string TenantRealmPrefix { get; set; } = "tenant-";
    public string FrontendRootUrl { get; set; } = "http://localhost:3000";
    public string[] RedirectUris { get; set; } = ["http://localhost:3000/*"];
    public string[] WebOrigins { get; set; } = ["http://localhost:3000"];
}
