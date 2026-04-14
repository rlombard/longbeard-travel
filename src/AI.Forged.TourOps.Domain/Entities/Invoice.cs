using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class Invoice
{
    public Guid Id { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string? ExternalSourceReference { get; set; }
    public string? InvoiceNumber { get; set; }
    public Guid? SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public Guid? BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public Guid? QuoteId { get; set; }
    public Guid? EmailThreadId { get; set; }
    public Guid? ReviewTaskId { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? RebateAmount { get; set; }
    public DateTime? RebateAppliedAt { get; set; }
    public string? Notes { get; set; }
    public string? RawExtractionPayloadJson { get; set; }
    public string? NormalizedSnapshotJson { get; set; }
    public decimal ExtractionConfidence { get; set; }
    public string? ExtractionIssuesJson { get; set; }
    public string? UnresolvedFieldsJson { get; set; }
    public bool RequiresHumanReview { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Supplier? Supplier { get; set; }
    public Booking? Booking { get; set; }
    public BookingItem? BookingItem { get; set; }
    public Quote? Quote { get; set; }
    public EmailThread? EmailThread { get; set; }
    public OperationalTask? ReviewTask { get; set; }
    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
    public ICollection<InvoiceAttachment> Attachments { get; set; } = new List<InvoiceAttachment>();
    public ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();
}
