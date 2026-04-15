namespace AI.Forged.TourOps.Domain.Entities;

public class TenantConfigEntry
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ConfigDomain { get; set; } = string.Empty;
    public string ConfigKey { get; set; } = string.Empty;
    public string JsonValue { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public string UpdatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
