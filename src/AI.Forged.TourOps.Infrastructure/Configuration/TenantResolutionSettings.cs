namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class TenantResolutionSettings
{
    public Guid StandaloneTenantId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public string StandaloneTenantSlug { get; set; } = "standalone";
    public string StandaloneTenantName { get; set; } = "Standalone Tenant";
    public string HeaderName { get; set; } = "X-Tenant-Id";
}
