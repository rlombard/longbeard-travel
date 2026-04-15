namespace AI.Forged.TourOps.Domain.Entities;

public class AuditEvent
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? ActorUserId { get; set; }
    public string? ActorDisplayName { get; set; }
    public string? TargetEntityType { get; set; }
    public Guid? TargetEntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }

    public Tenant? Tenant { get; set; }
}
