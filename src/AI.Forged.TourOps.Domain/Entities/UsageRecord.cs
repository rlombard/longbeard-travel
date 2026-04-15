namespace AI.Forged.TourOps.Domain.Entities;

public class UsageRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string MetricKey { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsBillable { get; set; }
    public string? Source { get; set; }
    public string? ReferenceEntityType { get; set; }
    public Guid? ReferenceEntityId { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<MonetizationTransaction> MonetizationTransactions { get; set; } = new List<MonetizationTransaction>();
}
