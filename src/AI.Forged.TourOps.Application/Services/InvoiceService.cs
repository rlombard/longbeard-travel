using System.Globalization;
using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Operations;
using AI.Forged.TourOps.Application.Models.Invoices;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using OperationalTaskStatus = AI.Forged.TourOps.Domain.Enums.TaskStatus;

namespace AI.Forged.TourOps.Application.Services;

public class InvoiceService(
    IInvoiceRepository invoiceRepository,
    ISupplierRepository supplierRepository,
    IBookingRepository bookingRepository,
    IBookingItemRepository bookingItemRepository,
    IQuoteRepository quoteRepository,
    IEmailRepository emailRepository,
    ITaskRepository taskRepository,
    ICurrentUserContext currentUserContext,
    IHumanApprovalService humanApprovalService) : IInvoiceService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<InvoiceIngestionResultModel> IngestInvoiceAsync(InvoiceIngestionRequestModel request, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeIngestionRequest(request);

        if (!string.IsNullOrWhiteSpace(normalized.ExternalSourceReference))
        {
            var existing = await invoiceRepository.GetBySourceAsync(
                normalized.SourceSystem,
                normalized.ExternalSourceReference,
                cancellationToken);

            if (existing is not null)
            {
                return BuildIngestionResult(existing, true, DeserializeList(existing.UnresolvedFieldsJson), ["Invoice already exists for this source reference."]);
            }
        }

        var match = await MatchInvoiceAsync(normalized, cancellationToken);
        var lineItemEntities = await BuildLineItemsAsync(normalized, match, cancellationToken);
        var warnings = new List<string>(match.Warnings);
        var unresolvedFields = new List<string>(match.UnresolvedFields);

        ValidateLineItems(lineItemEntities, unresolvedFields, warnings);
        ValidateTotals(normalized, lineItemEntities, warnings);

        var status = DetermineStatus(
            normalized.ExtractionConfidence,
            normalized.ExtractionIssues,
            unresolvedFields,
            match.HasSupplierMatch,
            match.HasOperationalMatch);

        var now = DateTime.UtcNow;
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            SourceSystem = normalized.SourceSystem,
            ExternalSourceReference = normalized.ExternalSourceReference,
            InvoiceNumber = normalized.InvoiceNumber,
            SupplierId = match.Supplier?.Id,
            SupplierName = match.Supplier?.Name ?? normalized.SupplierName ?? string.Empty,
            BookingId = match.Booking?.Id,
            BookingItemId = match.BookingItem?.Id,
            QuoteId = match.Quote?.Id,
            EmailThreadId = match.EmailThread?.Id,
            InvoiceDate = normalized.InvoiceDate,
            DueDate = normalized.DueDate,
            Currency = normalized.Currency,
            SubtotalAmount = normalized.SubtotalAmount,
            TaxAmount = normalized.TaxAmount,
            TotalAmount = normalized.TotalAmount,
            RebateAmount = normalized.RebateAmount,
            Notes = normalized.Notes,
            RawExtractionPayloadJson = normalized.RawExtractionPayloadJson ?? Serialize(normalized),
            NormalizedSnapshotJson = normalized.SourceSnapshotJson ?? Serialize(new
            {
                normalized.InvoiceNumber,
                normalized.SupplierId,
                normalized.SupplierName,
                normalized.BookingId,
                normalized.BookingItemId,
                normalized.QuoteId,
                normalized.InvoiceDate,
                normalized.DueDate,
                normalized.Currency,
                normalized.SubtotalAmount,
                normalized.TaxAmount,
                normalized.TotalAmount,
                normalized.RebateAmount,
                normalized.LineItems
            }),
            ExtractionConfidence = normalized.ExtractionConfidence,
            ExtractionIssuesJson = Serialize(normalized.ExtractionIssues),
            UnresolvedFieldsJson = Serialize(unresolvedFields),
            RequiresHumanReview = status is InvoiceStatus.PendingReview or InvoiceStatus.Unmatched or InvoiceStatus.Received,
            Status = status,
            ReceivedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        AssignLineItemIds(invoice.Id, lineItemEntities);
        var attachmentEntities = BuildAttachments(invoice.Id, normalized.Attachments, now);

        await invoiceRepository.AddAsync(invoice, lineItemEntities, attachmentEntities, cancellationToken);

        if (invoice.RequiresHumanReview && (invoice.BookingItemId.HasValue || invoice.BookingId.HasValue))
        {
            invoice.ReviewTaskId = await CreateReviewTaskAsync(invoice, unresolvedFields, warnings, cancellationToken);
            invoice.UpdatedAt = DateTime.UtcNow;
            await invoiceRepository.UpdateAsync(invoice, cancellationToken);
        }

        return BuildIngestionResult(invoice, false, unresolvedFields, warnings);
    }

    public async Task<InvoiceModel?> GetInvoiceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken);
        return invoice is null ? null : MapInvoice(invoice);
    }

    public async Task<IReadOnlyList<InvoiceListItemModel>> GetInvoicesAsync(InvoiceListQueryModel query, CancellationToken cancellationToken = default)
    {
        var invoices = await invoiceRepository.GetInvoicesAsync(query, cancellationToken);
        return invoices.Select(MapInvoiceListItem).ToList();
    }

    public async Task<InvoiceModel> UpdateInvoiceStatusAsync(Guid id, UpdateInvoiceStatusModel request, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        if (!IsValidStatusTransition(invoice.Status, request.Status))
        {
            throw new InvalidOperationException($"Invalid invoice status transition from '{invoice.Status}' to '{request.Status}'.");
        }

        var combinedNotes = CombineNotes(invoice.Notes, request.Notes);
        var updatedAt = DateTime.UtcNow;

        if (request.Status == InvoiceStatus.Approved)
        {
            var approval = await humanApprovalService.CreateApprovalRequestAsync(
                "ApproveInvoice",
                nameof(Invoice),
                invoice.Id,
                Serialize(new { invoice.Id, targetStatus = request.Status, notes = request.Notes }),
                cancellationToken);
            await humanApprovalService.ApproveActionAsync(approval.Id, request.Notes, cancellationToken);
        }
        else if (request.Status == InvoiceStatus.Rejected)
        {
            var approval = await humanApprovalService.CreateApprovalRequestAsync(
                "RejectInvoice",
                nameof(Invoice),
                invoice.Id,
                Serialize(new { invoice.Id, targetStatus = request.Status, notes = request.Notes }),
                cancellationToken);
            await humanApprovalService.RejectActionAsync(approval.Id, request.Notes, cancellationToken);
        }

        await invoiceRepository.UpdateStatusAsync(invoice.Id, request.Status, combinedNotes, updatedAt, cancellationToken);

        var updated = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found after update.");

        return MapInvoice(updated);
    }

    public async Task<InvoiceModel> RelinkInvoiceAsync(Guid id, RelinkInvoiceModel request, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        if (!CanRelink(invoice.Status))
        {
            throw new InvalidOperationException("This invoice can no longer be relinked.");
        }

        var relinkRequest = new InvoiceIngestionRequestModel
        {
            SourceSystem = invoice.SourceSystem,
            ExternalSourceReference = invoice.ExternalSourceReference,
            InvoiceNumber = invoice.InvoiceNumber,
            SupplierId = request.SupplierId,
            SupplierName = request.SupplierName,
            BookingId = request.BookingId,
            BookingItemId = request.BookingItemId,
            QuoteId = request.QuoteId,
            EmailThreadId = request.EmailThreadId,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Currency = invoice.Currency,
            SubtotalAmount = invoice.SubtotalAmount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            RebateAmount = invoice.RebateAmount,
            Notes = CombineNotes(invoice.Notes, request.Notes),
            RawExtractionPayloadJson = invoice.RawExtractionPayloadJson,
            SourceSnapshotJson = invoice.NormalizedSnapshotJson,
            ExtractionConfidence = invoice.ExtractionConfidence,
            ExtractionIssues = DeserializeList(invoice.ExtractionIssuesJson),
            UnresolvedFields = DeserializeList(invoice.UnresolvedFieldsJson),
            LineItems = invoice.LineItems.Select(x => new InvoiceLineItemInputModel
            {
                ExternalLineReference = x.ExternalLineReference,
                BookingItemId = x.BookingItemId,
                Description = x.Description,
                ServiceDate = x.ServiceDate,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount,
                Notes = x.Notes
            }).ToList()
        };

        var normalized = NormalizeIngestionRequest(relinkRequest);
        var match = await MatchInvoiceAsync(normalized, cancellationToken);
        var unresolvedFields = new List<string>(match.UnresolvedFields);
        var warnings = new List<string>(match.Warnings);

        invoice.SupplierId = match.Supplier?.Id;
        invoice.SupplierName = match.Supplier?.Name ?? normalized.SupplierName ?? invoice.SupplierName;
        invoice.BookingId = match.Booking?.Id;
        invoice.BookingItemId = match.BookingItem?.Id;
        invoice.QuoteId = match.Quote?.Id;
        invoice.EmailThreadId = match.EmailThread?.Id;
        invoice.Notes = normalized.Notes;
        invoice.UnresolvedFieldsJson = Serialize(unresolvedFields);
        invoice.RequiresHumanReview = unresolvedFields.Count > 0 || invoice.ExtractionConfidence < 0.85m;
        invoice.Status = DetermineStatus(
            invoice.ExtractionConfidence,
            DeserializeList(invoice.ExtractionIssuesJson),
            unresolvedFields,
            match.HasSupplierMatch,
            match.HasOperationalMatch);
        invoice.UpdatedAt = DateTime.UtcNow;

        if (invoice.RequiresHumanReview &&
            invoice.ReviewTaskId is null &&
            (invoice.BookingItemId.HasValue || invoice.BookingId.HasValue))
        {
            invoice.ReviewTaskId = await CreateReviewTaskAsync(invoice, unresolvedFields, warnings, cancellationToken);
        }

        await invoiceRepository.UpdateAsync(invoice, cancellationToken);

        var updated = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found after relink.");

        return MapInvoice(updated);
    }

    public async Task<InvoiceModel> RecordPaymentAsync(Guid id, RecordInvoicePaymentModel request, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        if (!CanRecordPayment(invoice.Status))
        {
            throw new InvalidOperationException("Payments cannot be recorded for this invoice status.");
        }

        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Payment amount must be greater than zero.");
        }

        var currency = NormalizeCurrency(request.Currency);
        if (!string.Equals(currency, invoice.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Payment currency must match the invoice currency.");
        }

        var payment = new PaymentRecord
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            ExternalPaymentReference = NormalizeOptional(request.ExternalPaymentReference, 256),
            Amount = Decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero),
            Currency = currency,
            PaidAt = request.PaidAt.ToUniversalTime(),
            PaymentMethod = NormalizeOptional(request.PaymentMethod, 128),
            Notes = NormalizeOptional(request.Notes, 2000),
            MetadataJson = NormalizeOptional(request.MetadataJson, 4000),
            RecordedByUserId = currentUserContext.GetRequiredUserId(),
            CreatedAt = DateTime.UtcNow
        };

        await invoiceRepository.AddPaymentRecordAsync(payment, cancellationToken);

        var reloaded = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found after payment record.");

        reloaded.Status = DeterminePaymentStatus(reloaded);
        reloaded.UpdatedAt = DateTime.UtcNow;
        await invoiceRepository.UpdateAsync(reloaded, cancellationToken);

        var updated = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found after update.");

        return MapInvoice(updated);
    }

    public async Task<InvoiceModel> ApplyRebateAsync(Guid id, ApplyInvoiceRebateModel request, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        if (!CanApplyRebate(invoice.Status))
        {
            throw new InvalidOperationException("Rebate cannot be applied for this invoice status.");
        }

        if (!invoice.RebateAmount.HasValue || invoice.RebateAmount.Value <= 0)
        {
            throw new InvalidOperationException("This invoice does not have a rebate amount to apply.");
        }

        invoice.RebateAppliedAt = DateTime.UtcNow;
        invoice.Status = DeterminePaymentStatus(invoice, rebateApplied: true);
        invoice.Notes = CombineNotes(invoice.Notes, request.Notes);
        invoice.UpdatedAt = DateTime.UtcNow;
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);

        var updated = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found after rebate update.");

        return MapInvoice(updated);
    }

    private async Task<MatchResult> MatchInvoiceAsync(InvoiceIngestionRequestModel request, CancellationToken cancellationToken)
    {
        var warnings = new List<string>();
        var unresolved = new List<string>(request.UnresolvedFields);

        Supplier? supplier = null;
        if (request.SupplierId.HasValue)
        {
            supplier = await supplierRepository.GetByIdAsync(request.SupplierId.Value, cancellationToken);
            if (supplier is null)
            {
                unresolved.Add("SupplierId");
                warnings.Add("Supplier id did not match an existing supplier.");
            }
        }
        else if (!string.IsNullOrWhiteSpace(request.SupplierReference))
        {
            supplier = await MatchSupplierAsync(request.SupplierReference, cancellationToken);
            if (supplier is null)
            {
                unresolved.Add("SupplierReference");
                warnings.Add("Supplier reference did not match an existing supplier.");
            }
        }
        else if (!string.IsNullOrWhiteSpace(request.SupplierName))
        {
            supplier = await supplierRepository.GetByNameAsync(request.SupplierName, cancellationToken);
            if (supplier is null)
            {
                unresolved.Add("SupplierName");
                warnings.Add("Supplier name did not match an existing supplier.");
            }
        }

        var bookingItem = await MatchBookingItemAsync(request.BookingItemId, request.BookingItemReference, cancellationToken, warnings, unresolved);
        var booking = await MatchBookingAsync(request.BookingId, request.BookingReference, cancellationToken, warnings, unresolved);
        var quote = await MatchQuoteAsync(request.QuoteId, request.QuoteReference, cancellationToken, warnings, unresolved);
        var emailThread = await MatchEmailThreadAsync(request.EmailThreadId, cancellationToken, warnings, unresolved);

        if (bookingItem is not null)
        {
            booking ??= await bookingRepository.GetByIdAsync(bookingItem.BookingId, cancellationToken);
            supplier ??= bookingItem.Supplier;
        }

        if (quote is not null)
        {
            booking ??= await bookingRepository.GetByQuoteIdAsync(quote.Id, cancellationToken);
        }

        if (emailThread is not null)
        {
            if (emailThread.BookingItemId.HasValue)
            {
                bookingItem ??= await bookingItemRepository.GetByIdAsync(emailThread.BookingItemId.Value, cancellationToken);
            }

            if (emailThread.BookingId.HasValue)
            {
                booking ??= await bookingRepository.GetByIdAsync(emailThread.BookingId.Value, cancellationToken);
            }
        }

        if (bookingItem is not null && booking is not null && bookingItem.BookingId != booking.Id)
        {
            unresolved.Add("BookingItemId");
            warnings.Add("Booking item does not belong to the matched booking.");
            booking = await bookingRepository.GetByIdAsync(bookingItem.BookingId, cancellationToken);
        }

        if (booking is not null && quote is not null && booking.QuoteId != quote.Id)
        {
            unresolved.Add("QuoteId");
            warnings.Add("Quote does not match the linked booking.");
        }

        if (supplier is not null && bookingItem is not null && bookingItem.SupplierId != supplier.Id)
        {
            unresolved.Add("SupplierId");
            warnings.Add("Supplier does not match the linked booking item.");
            supplier = bookingItem.Supplier;
        }

        return new MatchResult(
            supplier,
            booking,
            bookingItem,
            quote,
            emailThread,
            warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            unresolved.Distinct(StringComparer.OrdinalIgnoreCase).ToList());
    }

    private async Task<List<InvoiceLineItem>> BuildLineItemsAsync(
        InvoiceIngestionRequestModel request,
        MatchResult match,
        CancellationToken cancellationToken)
    {
        var result = new List<InvoiceLineItem>();

        foreach (var item in request.LineItems)
        {
            var bookingItem = await MatchBookingItemAsync(item.BookingItemId, item.BookingItemReference, cancellationToken, [], []);
            if (bookingItem is not null && match.Booking is not null && bookingItem.BookingId != match.Booking.Id)
            {
                bookingItem = null;
            }

            result.Add(new InvoiceLineItem
            {
                Id = Guid.NewGuid(),
                ExternalLineReference = NormalizeOptional(item.ExternalLineReference, 128),
                BookingItemId = bookingItem?.Id,
                Description = NormalizeRequired(item.Description, "Invoice line item description is required.", 500),
                ServiceDate = item.ServiceDate,
                Quantity = Decimal.Round(item.Quantity <= 0 ? 1m : item.Quantity, 2, MidpointRounding.AwayFromZero),
                UnitPrice = Decimal.Round(item.UnitPrice, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Decimal.Round(item.TaxAmount, 2, MidpointRounding.AwayFromZero),
                TotalAmount = Decimal.Round(item.TotalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = NormalizeOptional(item.Notes, 2000)
            });
        }

        return result;
    }

    private async Task<Booking?> MatchBookingAsync(
        Guid? bookingId,
        string? bookingReference,
        CancellationToken cancellationToken,
        List<string> warnings,
        List<string> unresolved)
    {
        if (bookingId.HasValue)
        {
            var booking = await bookingRepository.GetByIdAsync(bookingId.Value, cancellationToken);
            if (booking is null)
            {
                unresolved.Add("BookingId");
                warnings.Add("Booking id did not match an existing booking.");
            }

            return booking;
        }

        if (!TryParseGuid(bookingReference, out var parsedBookingId))
        {
            return null;
        }

        var matched = await bookingRepository.GetByIdAsync(parsedBookingId, cancellationToken);
        if (matched is null)
        {
            unresolved.Add("BookingReference");
            warnings.Add("Booking reference did not match an existing booking.");
        }

        return matched;
    }

    private async Task<BookingItem?> MatchBookingItemAsync(
        Guid? bookingItemId,
        string? bookingItemReference,
        CancellationToken cancellationToken,
        List<string> warnings,
        List<string> unresolved)
    {
        if (bookingItemId.HasValue)
        {
            var bookingItem = await bookingItemRepository.GetByIdAsync(bookingItemId.Value, cancellationToken);
            if (bookingItem is null)
            {
                unresolved.Add("BookingItemId");
                warnings.Add("Booking item id did not match an existing booking item.");
            }

            return bookingItem;
        }

        if (!TryParseGuid(bookingItemReference, out var parsedBookingItemId))
        {
            return null;
        }

        var matched = await bookingItemRepository.GetByIdAsync(parsedBookingItemId, cancellationToken);
        if (matched is null)
        {
            unresolved.Add("BookingItemReference");
            warnings.Add("Booking item reference did not match an existing booking item.");
        }

        return matched;
    }

    private async Task<Quote?> MatchQuoteAsync(
        Guid? quoteId,
        string? quoteReference,
        CancellationToken cancellationToken,
        List<string> warnings,
        List<string> unresolved)
    {
        if (quoteId.HasValue)
        {
            var quote = await quoteRepository.GetByIdAsync(quoteId.Value, cancellationToken);
            if (quote is null)
            {
                unresolved.Add("QuoteId");
                warnings.Add("Quote id did not match an existing quote.");
            }

            return quote;
        }

        if (!TryParseGuid(quoteReference, out var parsedQuoteId))
        {
            return null;
        }

        var matched = await quoteRepository.GetByIdAsync(parsedQuoteId, cancellationToken);
        if (matched is null)
        {
            unresolved.Add("QuoteReference");
            warnings.Add("Quote reference did not match an existing quote.");
        }

        return matched;
    }

    private async Task<EmailThread?> MatchEmailThreadAsync(
        Guid? emailThreadId,
        CancellationToken cancellationToken,
        List<string> warnings,
        List<string> unresolved)
    {
        if (!emailThreadId.HasValue)
        {
            return null;
        }

        var emailThread = await emailRepository.GetThreadByIdAsync(emailThreadId.Value, cancellationToken);
        if (emailThread is null)
        {
            unresolved.Add("EmailThreadId");
            warnings.Add("Email thread id did not match an existing email thread.");
        }

        return emailThread;
    }

    private async Task<Supplier?> MatchSupplierAsync(string supplierReference, CancellationToken cancellationToken)
    {
        if (TryParseGuid(supplierReference, out var supplierId))
        {
            return await supplierRepository.GetByIdAsync(supplierId, cancellationToken);
        }

        return await supplierRepository.GetByNameAsync(supplierReference, cancellationToken);
    }

    private async Task<Guid?> CreateReviewTaskAsync(
        Invoice invoice,
        IReadOnlyList<string> unresolvedFields,
        IReadOnlyList<string> warnings,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var title = $"Review invoice {invoice.InvoiceNumber ?? invoice.Id.ToString()[..8]}";
        var description = BuildReviewTaskDescription(invoice, unresolvedFields, warnings);
        var userId = currentUserContext.GetRequiredUserId();

        var task = new OperationalTask
        {
            Id = Guid.NewGuid(),
            BookingId = invoice.BookingItemId.HasValue ? null : invoice.BookingId,
            BookingItemId = invoice.BookingItemId,
            Title = title,
            Description = description,
            Status = OperationalTaskStatus.ToDo,
            AssignedToUserId = userId,
            CreatedByUserId = userId,
            DueDate = invoice.DueDate.HasValue
                ? invoice.DueDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                : now.AddDays(1),
            CreatedAt = now,
            UpdatedAt = now
        };

        await taskRepository.AddAsync(task, cancellationToken);
        return task.Id;
    }

    private static string BuildReviewTaskDescription(
        Invoice invoice,
        IReadOnlyList<string> unresolvedFields,
        IReadOnlyList<string> warnings)
    {
        var parts = new List<string>
        {
            $"Invoice {invoice.InvoiceNumber ?? invoice.Id.ToString()} needs review."
        };

        if (unresolvedFields.Count > 0)
        {
            parts.Add($"Unresolved fields: {string.Join(", ", unresolvedFields)}.");
        }

        if (warnings.Count > 0)
        {
            parts.Add(string.Join(" ", warnings.Take(3)));
        }

        return string.Join(" ", parts);
    }

    private static void AssignLineItemIds(Guid invoiceId, IEnumerable<InvoiceLineItem> lineItems)
    {
        foreach (var lineItem in lineItems)
        {
            lineItem.InvoiceId = invoiceId;
        }
    }

    private static List<InvoiceAttachment> BuildAttachments(Guid invoiceId, IEnumerable<InvoiceAttachmentInputModel> attachments, DateTime createdAt)
    {
        return attachments.Select(attachment => new InvoiceAttachment
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            ExternalFileReference = NormalizeOptional(attachment.ExternalFileReference, 256),
            FileName = NormalizeRequired(attachment.FileName, "Attachment file name is required.", 256),
            ContentType = NormalizeOptional(attachment.ContentType, 128),
            SourceUrl = NormalizeOptional(attachment.SourceUrl, 2000),
            MetadataJson = NormalizeOptional(attachment.MetadataJson, 4000),
            CreatedAt = createdAt
        }).ToList();
    }

    private static void ValidateLineItems(
        IEnumerable<InvoiceLineItem> lineItems,
        ICollection<string> unresolvedFields,
        ICollection<string> warnings)
    {
        foreach (var lineItem in lineItems)
        {
            if (lineItem.UnitPrice < 0 || lineItem.TaxAmount < 0 || lineItem.TotalAmount < 0)
            {
                unresolvedFields.Add("LineItems");
                warnings.Add("Invoice line items contained negative monetary values.");
            }
        }
    }

    private static void ValidateTotals(
        InvoiceIngestionRequestModel request,
        IReadOnlyList<InvoiceLineItem> lineItems,
        ICollection<string> warnings)
    {
        var expectedLineTotal = lineItems.Sum(x => x.TotalAmount);
        if (lineItems.Count > 0 && Math.Abs(expectedLineTotal - request.TotalAmount) > 0.01m)
        {
            warnings.Add("Invoice total does not match summed line item totals.");
        }
    }

    private static InvoiceStatus DetermineStatus(
        decimal extractionConfidence,
        IReadOnlyList<string> extractionIssues,
        IReadOnlyList<string> unresolvedFields,
        bool hasSupplierMatch,
        bool hasOperationalMatch)
    {
        if (!hasSupplierMatch && !hasOperationalMatch)
        {
            return InvoiceStatus.Unmatched;
        }

        if (unresolvedFields.Count > 0 || extractionIssues.Count > 0 || extractionConfidence < 0.75m)
        {
            return InvoiceStatus.PendingReview;
        }

        if (hasSupplierMatch && hasOperationalMatch)
        {
            return InvoiceStatus.Matched;
        }

        return InvoiceStatus.Received;
    }

    private static InvoiceStatus DeterminePaymentStatus(Invoice invoice, bool rebateApplied = false)
    {
        var amountPaid = invoice.PaymentRecords.Sum(x => x.Amount);
        var rebateAmount = rebateApplied || invoice.RebateAppliedAt.HasValue
            ? Math.Max(0m, invoice.RebateAmount ?? 0m)
            : 0m;
        var outstanding = Math.Max(0m, invoice.TotalAmount - rebateAmount - amountPaid);

        if (outstanding <= 0m)
        {
            return InvoiceStatus.Paid;
        }

        if (rebateAmount > 0m)
        {
            return InvoiceStatus.RebateApplied;
        }

        if (amountPaid > 0m)
        {
            return InvoiceStatus.PartiallyPaid;
        }

        return InvoiceStatus.Unpaid;
    }

    private static bool IsValidStatusTransition(InvoiceStatus currentStatus, InvoiceStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return true;
        }

        return currentStatus switch
        {
            InvoiceStatus.Draft => nextStatus is InvoiceStatus.Received or InvoiceStatus.PendingReview or InvoiceStatus.Cancelled,
            InvoiceStatus.Received => nextStatus is InvoiceStatus.Matched or InvoiceStatus.Unmatched or InvoiceStatus.PendingReview or InvoiceStatus.Approved or InvoiceStatus.Rejected or InvoiceStatus.Cancelled,
            InvoiceStatus.Matched => nextStatus is InvoiceStatus.PendingReview or InvoiceStatus.Approved or InvoiceStatus.Unpaid or InvoiceStatus.Rejected or InvoiceStatus.Cancelled,
            InvoiceStatus.Unmatched => nextStatus is InvoiceStatus.PendingReview or InvoiceStatus.Matched or InvoiceStatus.Rejected or InvoiceStatus.Cancelled,
            InvoiceStatus.PendingReview => nextStatus is InvoiceStatus.Matched or InvoiceStatus.Unmatched or InvoiceStatus.Approved or InvoiceStatus.Rejected or InvoiceStatus.Cancelled,
            InvoiceStatus.Approved => nextStatus is InvoiceStatus.Unpaid or InvoiceStatus.PartiallyPaid or InvoiceStatus.Paid or InvoiceStatus.RebatePending or InvoiceStatus.RebateApplied or InvoiceStatus.Cancelled,
            InvoiceStatus.Rejected => nextStatus is InvoiceStatus.PendingReview or InvoiceStatus.Cancelled,
            InvoiceStatus.Unpaid => nextStatus is InvoiceStatus.PartiallyPaid or InvoiceStatus.Paid or InvoiceStatus.Overdue or InvoiceStatus.RebatePending or InvoiceStatus.RebateApplied or InvoiceStatus.Cancelled,
            InvoiceStatus.PartiallyPaid => nextStatus is InvoiceStatus.Paid or InvoiceStatus.Overdue or InvoiceStatus.RebatePending or InvoiceStatus.RebateApplied or InvoiceStatus.Cancelled,
            InvoiceStatus.Paid => nextStatus is InvoiceStatus.RebatePending or InvoiceStatus.RebateApplied,
            InvoiceStatus.Overdue => nextStatus is InvoiceStatus.PartiallyPaid or InvoiceStatus.Paid or InvoiceStatus.RebatePending or InvoiceStatus.RebateApplied or InvoiceStatus.Cancelled,
            InvoiceStatus.RebatePending => nextStatus is InvoiceStatus.RebateApplied or InvoiceStatus.PartiallyPaid or InvoiceStatus.Paid or InvoiceStatus.Overdue or InvoiceStatus.Cancelled,
            InvoiceStatus.RebateApplied => nextStatus is InvoiceStatus.PartiallyPaid or InvoiceStatus.Paid or InvoiceStatus.Overdue or InvoiceStatus.Cancelled,
            InvoiceStatus.Cancelled => false,
            _ => false
        };
    }

    private static bool CanRelink(InvoiceStatus status) =>
        status is not InvoiceStatus.Paid and not InvoiceStatus.PartiallyPaid and not InvoiceStatus.RebateApplied and not InvoiceStatus.Cancelled;

    private static bool CanRecordPayment(InvoiceStatus status) =>
        status is InvoiceStatus.Approved or InvoiceStatus.Unpaid or InvoiceStatus.PartiallyPaid or InvoiceStatus.Overdue or InvoiceStatus.RebatePending or InvoiceStatus.RebateApplied;

    private static bool CanApplyRebate(InvoiceStatus status) =>
        status is InvoiceStatus.Approved or InvoiceStatus.Unpaid or InvoiceStatus.PartiallyPaid or InvoiceStatus.Overdue or InvoiceStatus.RebatePending or InvoiceStatus.RebateApplied;

    private static InvoiceIngestionResultModel BuildIngestionResult(
        Invoice invoice,
        bool wasExisting,
        IReadOnlyList<string> unresolvedFields,
        IReadOnlyList<string> warnings) => new()
    {
        InvoiceId = invoice.Id,
        WasExisting = wasExisting,
        SupplierId = invoice.SupplierId,
        BookingId = invoice.BookingId,
        BookingItemId = invoice.BookingItemId,
        QuoteId = invoice.QuoteId,
        EmailThreadId = invoice.EmailThreadId,
        ReviewTaskId = invoice.ReviewTaskId,
        FinalStatus = invoice.Status,
        UnresolvedFields = unresolvedFields,
        Warnings = warnings
    };

    private static InvoiceModel MapInvoice(Invoice invoice)
    {
        var amountPaid = invoice.PaymentRecords.Sum(x => x.Amount);
        var outstanding = Math.Max(0m, invoice.TotalAmount - amountPaid - ((invoice.Status == InvoiceStatus.RebateApplied || invoice.RebateAppliedAt.HasValue) ? invoice.RebateAmount ?? 0m : 0m));

        return new InvoiceModel
        {
            Id = invoice.Id,
            SourceSystem = invoice.SourceSystem,
            ExternalSourceReference = invoice.ExternalSourceReference,
            InvoiceNumber = invoice.InvoiceNumber,
            SupplierId = invoice.SupplierId,
            SupplierName = invoice.Supplier?.Name ?? invoice.SupplierName,
            BookingId = invoice.BookingId,
            BookingItemId = invoice.BookingItemId,
            QuoteId = invoice.QuoteId,
            EmailThreadId = invoice.EmailThreadId,
            ReviewTaskId = invoice.ReviewTaskId,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Currency = invoice.Currency,
            SubtotalAmount = invoice.SubtotalAmount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            RebateAmount = invoice.RebateAmount,
            AmountPaid = amountPaid,
            OutstandingAmount = outstanding,
            Notes = invoice.Notes,
            ExtractionConfidence = invoice.ExtractionConfidence,
            ExtractionIssues = DeserializeList(invoice.ExtractionIssuesJson),
            UnresolvedFields = DeserializeList(invoice.UnresolvedFieldsJson),
            RequiresHumanReview = invoice.RequiresHumanReview,
            Status = invoice.Status,
            ReceivedAt = invoice.ReceivedAt,
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt,
            LineItems = invoice.LineItems.Select(lineItem => new InvoiceLineItemModel
            {
                Id = lineItem.Id,
                BookingItemId = lineItem.BookingItemId,
                Description = lineItem.Description,
                ServiceDate = lineItem.ServiceDate,
                Quantity = lineItem.Quantity,
                UnitPrice = lineItem.UnitPrice,
                TaxAmount = lineItem.TaxAmount,
                TotalAmount = lineItem.TotalAmount,
                Notes = lineItem.Notes
            }).ToList(),
            Attachments = invoice.Attachments.Select(attachment => new InvoiceAttachmentModel
            {
                Id = attachment.Id,
                ExternalFileReference = attachment.ExternalFileReference,
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                SourceUrl = attachment.SourceUrl,
                CreatedAt = attachment.CreatedAt
            }).ToList(),
            PaymentRecords = invoice.PaymentRecords.Select(payment => new PaymentRecordModel
            {
                Id = payment.Id,
                ExternalPaymentReference = payment.ExternalPaymentReference,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaidAt = payment.PaidAt,
                PaymentMethod = payment.PaymentMethod,
                Notes = payment.Notes,
                RecordedByUserId = payment.RecordedByUserId,
                CreatedAt = payment.CreatedAt
            }).ToList()
        };
    }

    private static InvoiceListItemModel MapInvoiceListItem(Invoice invoice)
    {
        var amountPaid = invoice.PaymentRecords.Sum(x => x.Amount);
        var outstanding = Math.Max(0m, invoice.TotalAmount - amountPaid - ((invoice.Status == InvoiceStatus.RebateApplied || invoice.RebateAppliedAt.HasValue) ? invoice.RebateAmount ?? 0m : 0m));

        return new InvoiceListItemModel
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            SupplierName = invoice.Supplier?.Name ?? invoice.SupplierName,
            SupplierId = invoice.SupplierId,
            BookingId = invoice.BookingId,
            BookingItemId = invoice.BookingItemId,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Currency = invoice.Currency,
            TotalAmount = invoice.TotalAmount,
            AmountPaid = amountPaid,
            OutstandingAmount = outstanding,
            RequiresHumanReview = invoice.RequiresHumanReview,
            Status = invoice.Status
        };
    }

    private static InvoiceIngestionRequestModel NormalizeIngestionRequest(InvoiceIngestionRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.SourceSystem))
        {
            throw new InvalidOperationException("Source system is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ExternalSourceReference) && string.IsNullOrWhiteSpace(request.InvoiceNumber))
        {
            throw new InvalidOperationException("Either an external source reference or an invoice number is required.");
        }

        if (request.InvoiceDate == default)
        {
            throw new InvalidOperationException("Invoice date is required.");
        }

        if (request.DueDate.HasValue && request.DueDate.Value < request.InvoiceDate)
        {
            throw new InvalidOperationException("Due date cannot be before the invoice date.");
        }

        if (request.TotalAmount < 0 || request.SubtotalAmount < 0 || request.TaxAmount < 0)
        {
            throw new InvalidOperationException("Invoice amounts cannot be negative.");
        }

        if (request.RebateAmount.HasValue && request.RebateAmount.Value < 0)
        {
            throw new InvalidOperationException("Rebate amount cannot be negative.");
        }

        return new InvoiceIngestionRequestModel
        {
            SourceSystem = NormalizeRequired(request.SourceSystem, "Source system is required.", 64),
            ExternalSourceReference = NormalizeOptional(request.ExternalSourceReference, 256),
            InvoiceNumber = NormalizeOptional(request.InvoiceNumber, 128),
            SupplierId = request.SupplierId,
            SupplierReference = NormalizeOptional(request.SupplierReference, 256),
            SupplierName = NormalizeOptional(request.SupplierName, 200),
            BookingId = request.BookingId,
            BookingReference = NormalizeOptional(request.BookingReference, 128),
            BookingItemId = request.BookingItemId,
            BookingItemReference = NormalizeOptional(request.BookingItemReference, 128),
            QuoteId = request.QuoteId,
            QuoteReference = NormalizeOptional(request.QuoteReference, 128),
            EmailThreadId = request.EmailThreadId,
            InvoiceDate = request.InvoiceDate,
            DueDate = request.DueDate,
            Currency = NormalizeCurrency(request.Currency),
            SubtotalAmount = Decimal.Round(request.SubtotalAmount, 2, MidpointRounding.AwayFromZero),
            TaxAmount = Decimal.Round(request.TaxAmount, 2, MidpointRounding.AwayFromZero),
            TotalAmount = Decimal.Round(request.TotalAmount, 2, MidpointRounding.AwayFromZero),
            RebateAmount = request.RebateAmount.HasValue ? Decimal.Round(request.RebateAmount.Value, 2, MidpointRounding.AwayFromZero) : null,
            Notes = NormalizeOptional(request.Notes, 4000),
            RawExtractionPayloadJson = NormalizeOptional(request.RawExtractionPayloadJson, 16000),
            SourceSnapshotJson = NormalizeOptional(request.SourceSnapshotJson, 16000),
            ExtractionConfidence = ClampConfidence(request.ExtractionConfidence),
            ExtractionIssues = request.ExtractionIssues
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => NormalizeRequired(x, "Extraction issue is required.", 500))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            UnresolvedFields = request.UnresolvedFields
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => NormalizeRequired(x, "Unresolved field is required.", 128))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            LineItems = request.LineItems.Select(lineItem => new InvoiceLineItemInputModel
            {
                ExternalLineReference = NormalizeOptional(lineItem.ExternalLineReference, 128),
                BookingItemId = lineItem.BookingItemId,
                BookingItemReference = NormalizeOptional(lineItem.BookingItemReference, 128),
                Description = NormalizeRequired(lineItem.Description, "Invoice line item description is required.", 500),
                ServiceDate = lineItem.ServiceDate,
                Quantity = Decimal.Round(lineItem.Quantity <= 0 ? 1m : lineItem.Quantity, 2, MidpointRounding.AwayFromZero),
                UnitPrice = Decimal.Round(lineItem.UnitPrice, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Decimal.Round(lineItem.TaxAmount, 2, MidpointRounding.AwayFromZero),
                TotalAmount = Decimal.Round(lineItem.TotalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = NormalizeOptional(lineItem.Notes, 2000)
            }).ToList(),
            Attachments = request.Attachments.Select(attachment => new InvoiceAttachmentInputModel
            {
                ExternalFileReference = NormalizeOptional(attachment.ExternalFileReference, 256),
                FileName = NormalizeRequired(attachment.FileName, "Attachment file name is required.", 256),
                ContentType = NormalizeOptional(attachment.ContentType, 128),
                SourceUrl = NormalizeOptional(attachment.SourceUrl, 2000),
                MetadataJson = NormalizeOptional(attachment.MetadataJson, 4000)
            }).ToList()
        };
    }

    private static string NormalizeCurrency(string? currency)
    {
        var normalized = currency?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Currency is required.");
        }

        if (normalized.Length > 8)
        {
            throw new InvalidOperationException("Currency cannot exceed 8 characters.");
        }

        return normalized;
    }

    private static string NormalizeRequired(string? value, string message, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(message);
        }

        if (normalized.Length > maxLength)
        {
            throw new InvalidOperationException($"Value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized is { Length: > 0 } && normalized.Length > maxLength)
        {
            throw new InvalidOperationException($"Value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static decimal ClampConfidence(decimal value) => Math.Min(1m, Math.Max(0m, value));

    private static bool TryParseGuid(string? value, out Guid id)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            id = Guid.Empty;
            return false;
        }

        return Guid.TryParse(value.Trim(), CultureInfo.InvariantCulture, out id);
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static List<string> DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
    }

    private static string? CombineNotes(string? existingNotes, string? newNotes)
    {
        var normalizedNewNotes = NormalizeOptional(newNotes, 4000);
        if (string.IsNullOrWhiteSpace(normalizedNewNotes))
        {
            return existingNotes;
        }

        var normalizedExistingNotes = NormalizeOptional(existingNotes, 4000);
        if (string.IsNullOrWhiteSpace(normalizedExistingNotes))
        {
            return normalizedNewNotes;
        }

        var combined = $"{normalizedExistingNotes}\n{normalizedNewNotes}";
        return NormalizeOptional(combined, 4000);
    }

    private sealed record MatchResult(
        Supplier? Supplier,
        Booking? Booking,
        BookingItem? BookingItem,
        Quote? Quote,
        EmailThread? EmailThread,
        IReadOnlyList<string> Warnings,
        IReadOnlyList<string> UnresolvedFields)
    {
        public bool HasSupplierMatch => Supplier is not null;
        public bool HasOperationalMatch => Booking is not null || BookingItem is not null || Quote is not null;
    }
}
