using System.Text.Json.Serialization;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Api.Models;

public sealed class InvoiceAttachmentRequest
{
    public string? ExternalFileReference { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string? SourceUrl { get; set; }
    public string? MetadataJson { get; set; }
}

public sealed class InvoiceLineItemRequest
{
    public string? ExternalLineReference { get; set; }
    public Guid? BookingItemId { get; set; }
    public string? BookingItemReference { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly? ServiceDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
}

public sealed class InvoiceIngestionRequest
{
    public string SourceSystem { get; set; } = string.Empty;
    public string? ExternalSourceReference { get; set; }
    public string? InvoiceNumber { get; set; }
    public Guid? SupplierId { get; set; }
    public string? SupplierReference { get; set; }
    public string? SupplierName { get; set; }
    public Guid? BookingId { get; set; }
    public string? BookingReference { get; set; }
    public Guid? BookingItemId { get; set; }
    public string? BookingItemReference { get; set; }
    public Guid? QuoteId { get; set; }
    public string? QuoteReference { get; set; }
    public Guid? EmailThreadId { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? RebateAmount { get; set; }
    public string? Notes { get; set; }
    public string? RawExtractionPayloadJson { get; set; }
    public string? SourceSnapshotJson { get; set; }
    public decimal ExtractionConfidence { get; set; }
    public List<string> ExtractionIssues { get; set; } = [];
    public List<string> UnresolvedFields { get; set; } = [];
    public List<InvoiceLineItemRequest> LineItems { get; set; } = [];
    public List<InvoiceAttachmentRequest> Attachments { get; set; } = [];
}

public sealed class InvoiceIngestionResponse
{
    public Guid InvoiceId { get; set; }
    public bool WasExisting { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public Guid? QuoteId { get; set; }
    public Guid? EmailThreadId { get; set; }
    public Guid? ReviewTaskId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InvoiceStatus FinalStatus { get; set; }
    public List<string> UnresolvedFields { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}

public sealed class InvoiceListResponse
{
    public Guid Id { get; set; }
    public string? InvoiceNumber { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public Guid? SupplierId { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }
    public bool RequiresHumanReview { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InvoiceStatus Status { get; set; }
}

public sealed class InvoiceResponse
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
    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string? Notes { get; set; }
    public decimal ExtractionConfidence { get; set; }
    public List<string> ExtractionIssues { get; set; } = [];
    public List<string> UnresolvedFields { get; set; } = [];
    public bool RequiresHumanReview { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InvoiceStatus Status { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<InvoiceLineItemResponse> LineItems { get; set; } = [];
    public List<InvoiceAttachmentResponse> Attachments { get; set; } = [];
    public List<PaymentRecordResponse> PaymentRecords { get; set; } = [];
}

public sealed class InvoiceLineItemResponse
{
    public Guid Id { get; set; }
    public Guid? BookingItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly? ServiceDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
}

public sealed class InvoiceAttachmentResponse
{
    public Guid Id { get; set; }
    public string? ExternalFileReference { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class PaymentRecordResponse
{
    public Guid Id { get; set; }
    public string? ExternalPaymentReference { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public string RecordedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class UpdateInvoiceStatusRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
}

public sealed class RelinkInvoiceRequest
{
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public Guid? QuoteId { get; set; }
    public Guid? EmailThreadId { get; set; }
    public string? Notes { get; set; }
}

public sealed class RecordInvoicePaymentRequest
{
    public string? ExternalPaymentReference { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public string? MetadataJson { get; set; }
}

public sealed class ApplyInvoiceRebateRequest
{
    public string? Notes { get; set; }
}
