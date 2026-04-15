using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class MonetizationTransaction
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? UsageRecordId { get; set; }
    public MonetizationTransactionType TransactionType { get; set; }
    public MonetizationTransactionStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string? ExternalReference { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public UsageRecord? UsageRecord { get; set; }
}
