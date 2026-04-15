using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Platform;

public sealed class PlatformIdentitySettingsAccessor(IOptions<KeycloakTenantProvisioningSettings> settings) : IPlatformIdentitySettingsAccessor
{
    public string PublicKeycloakBaseUrl => settings.Value.PublicBaseUrl;
    public string ManagementRealm => settings.Value.ManagementRealm;
    public string ManagementClientId => settings.Value.ManagementClientId;
    public string TenantClientId => settings.Value.TenantClientId;
    public string TenantRealmPrefix => settings.Value.TenantRealmPrefix;
}
