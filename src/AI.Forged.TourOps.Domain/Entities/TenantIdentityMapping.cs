using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class TenantIdentityMapping
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public IdentityIsolationMode IsolationMode { get; set; }
    public IdentityProvisioningStatus ProvisioningStatus { get; set; }
    public string RealmName { get; set; } = string.Empty;
    public string? ClientId { get; set; }
    public string? IssuerUrl { get; set; }
    public string? MetadataJson { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
