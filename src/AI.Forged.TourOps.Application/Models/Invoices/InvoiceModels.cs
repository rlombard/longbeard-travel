using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Models.Invoices;

public sealed class InvoiceAttachmentInputModel
{
    public string? ExternalFileReference { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string? ContentType { get; init; }
    public string? SourceUrl { get; init; }
    public string? MetadataJson { get; init; }
}

public sealed class InvoiceLineItemInputModel
{
    public string? ExternalLineReference { get; init; }
    public Guid? BookingItemId { get; init; }
    public string? BookingItemReference { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateOnly? ServiceDate { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public string? Notes { get; init; }
}

public sealed class InvoiceIngestionRequestModel
{
    public string SourceSystem { get; init; } = string.Empty;
    public string? ExternalSourceReference { get; init; }
    public string? InvoiceNumber { get; init; }
    public Guid? SupplierId { get; init; }
    public string? SupplierReference { get; init; }
    public string? SupplierName { get; init; }
    public Guid? BookingId { get; init; }
    public string? BookingReference { get; init; }
    public Guid? BookingItemId { get; init; }
    public string? BookingItemReference { get; init; }
    public Guid? QuoteId { get; init; }
    public string? QuoteReference { get; init; }
    public Guid? EmailThreadId { get; init; }
    public DateOnly InvoiceDate { get; init; }
    public DateOnly? DueDate { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal SubtotalAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal? RebateAmount { get; init; }
    public string? Notes { get; init; }
    public string? RawExtractionPayloadJson { get; init; }
    public string? SourceSnapshotJson { get; init; }
    public decimal ExtractionConfidence { get; init; }
    public IReadOnlyList<string> ExtractionIssues { get; init; } = [];
    public IReadOnlyList<string> UnresolvedFields { get; init; } = [];
    public IReadOnlyList<InvoiceLineItemInputModel> LineItems { get; init; } = [];
    public IReadOnlyList<InvoiceAttachmentInputModel> Attachments { get; init; } = [];
}

public sealed class InvoiceIngestionResultModel
{
    public Guid InvoiceId { get; init; }
    public bool WasExisting { get; init; }
    public Guid? SupplierId { get; init; }
    public Guid? BookingId { get; init; }
    public Guid? BookingItemId { get; init; }
    public Guid? QuoteId { get; init; }
    public Guid? EmailThreadId { get; init; }
    public Guid? ReviewTaskId { get; init; }
    public InvoiceStatus FinalStatus { get; init; }
    public IReadOnlyList<string> UnresolvedFields { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed class InvoiceListQueryModel
{
    public Guid? SupplierId { get; init; }
    public Guid? BookingId { get; init; }
    public Guid? BookingItemId { get; init; }
    public Guid? QuoteId { get; init; }
    public InvoiceStatus? Status { get; init; }
    public DateOnly? DueBefore { get; init; }
    public bool UnpaidOnly { get; init; }
}

public sealed class InvoiceModel
{
    public Guid Id { get; init; }
    public string SourceSystem { get; init; } = string.Empty;
    public string? ExternalSourceReference { get; init; }
    public string? InvoiceNumber { get; init; }
    public Guid? SupplierId { get; init; }
    public string SupplierName { get; init; } = string.Empty;
    public Guid? BookingId { get; init; }
    public Guid? BookingItemId { get; init; }
    public Guid? QuoteId { get; init; }
    public Guid? EmailThreadId { get; init; }
    public Guid? ReviewTaskId { get; init; }
    public DateOnly InvoiceDate { get; init; }
    public DateOnly? DueDate { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal SubtotalAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal? RebateAmount { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal OutstandingAmount { get; init; }
    public string? Notes { get; init; }
    public decimal ExtractionConfidence { get; init; }
    public IReadOnlyList<string> ExtractionIssues { get; init; } = [];
    public IReadOnlyList<string> UnresolvedFields { get; init; } = [];
    public bool RequiresHumanReview { get; init; }
    public InvoiceStatus Status { get; init; }
    public DateTime ReceivedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public IReadOnlyList<InvoiceLineItemModel> LineItems { get; init; } = [];
    public IReadOnlyList<InvoiceAttachmentModel> Attachments { get; init; } = [];
    public IReadOnlyList<PaymentRecordModel> PaymentRecords { get; init; } = [];
}

public sealed class InvoiceListItemModel
{
    public Guid Id { get; init; }
    public string? InvoiceNumber { get; init; }
    public string SupplierName { get; init; } = string.Empty;
    public Guid? SupplierId { get; init; }
    public Guid? BookingId { get; init; }
    public Guid? BookingItemId { get; init; }
    public DateOnly InvoiceDate { get; init; }
    public DateOnly? DueDate { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal OutstandingAmount { get; init; }
    public bool RequiresHumanReview { get; init; }
    public InvoiceStatus Status { get; init; }
}

public sealed class InvoiceLineItemModel
{
    public Guid Id { get; init; }
    public Guid? BookingItemId { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateOnly? ServiceDate { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public string? Notes { get; init; }
}

public sealed class InvoiceAttachmentModel
{
    public Guid Id { get; init; }
    public string? ExternalFileReference { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string? ContentType { get; init; }
    public string? SourceUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class PaymentRecordModel
{
    public Guid Id { get; init; }
    public string? ExternalPaymentReference { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime PaidAt { get; init; }
    public string? PaymentMethod { get; init; }
    public string? Notes { get; init; }
    public string RecordedByUserId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public sealed class UpdateInvoiceStatusModel
{
    public InvoiceStatus Status { get; init; }
    public string? Notes { get; init; }
}

public sealed class RelinkInvoiceModel
{
    public Guid? SupplierId { get; init; }
    public string? SupplierName { get; init; }
    public Guid? BookingId { get; init; }
    public Guid? BookingItemId { get; init; }
    public Guid? QuoteId { get; init; }
    public Guid? EmailThreadId { get; init; }
    public string? Notes { get; init; }
}

public sealed class RecordInvoicePaymentModel
{
    public string? ExternalPaymentReference { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime PaidAt { get; init; }
    public string? PaymentMethod { get; init; }
    public string? Notes { get; init; }
    public string? MetadataJson { get; init; }
}

public sealed class ApplyInvoiceRebateModel
{
    public string? Notes { get; init; }
}
