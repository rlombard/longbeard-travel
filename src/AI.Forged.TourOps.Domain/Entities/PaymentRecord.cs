namespace AI.Forged.TourOps.Domain.Entities;

public class PaymentRecord
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string? ExternalPaymentReference { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public string RecordedByUserId { get; set; } = string.Empty;
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }

    public Invoice Invoice { get; set; } = null!;
}
