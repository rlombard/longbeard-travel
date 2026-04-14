using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models.Invoices;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class InvoiceRepository(AppDbContext dbContext) : IInvoiceRepository
{
    public async Task<Invoice> AddAsync(
        Invoice invoice,
        IEnumerable<InvoiceLineItem> lineItems,
        IEnumerable<InvoiceAttachment> attachments,
        CancellationToken cancellationToken = default)
    {
        dbContext.Invoices.Add(invoice);
        dbContext.InvoiceLineItems.AddRange(lineItems);
        dbContext.InvoiceAttachments.AddRange(attachments);
        await dbContext.SaveChangesAsync(cancellationToken);
        invoice.LineItems = lineItems.ToList();
        invoice.Attachments = attachments.ToList();
        return invoice;
    }

    public async Task<PaymentRecord> AddPaymentRecordAsync(PaymentRecord paymentRecord, CancellationToken cancellationToken = default)
    {
        dbContext.PaymentRecords.Add(paymentRecord);
        await dbContext.SaveChangesAsync(cancellationToken);
        return paymentRecord;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await BuildInvoiceQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Invoice?> GetBySourceAsync(string sourceSystem, string externalSourceReference, CancellationToken cancellationToken = default) =>
        await BuildInvoiceQuery()
            .FirstOrDefaultAsync(
                x => x.SourceSystem == sourceSystem && x.ExternalSourceReference == externalSourceReference,
                cancellationToken);

    public async Task<IReadOnlyList<Invoice>> GetInvoicesAsync(InvoiceListQueryModel query, CancellationToken cancellationToken = default)
    {
        var invoices = BuildInvoiceQuery();

        if (query.SupplierId.HasValue)
        {
            invoices = invoices.Where(x => x.SupplierId == query.SupplierId);
        }

        if (query.BookingId.HasValue)
        {
            invoices = invoices.Where(x => x.BookingId == query.BookingId || (x.BookingItem != null && x.BookingItem.BookingId == query.BookingId));
        }

        if (query.BookingItemId.HasValue)
        {
            invoices = invoices.Where(x => x.BookingItemId == query.BookingItemId);
        }

        if (query.QuoteId.HasValue)
        {
            invoices = invoices.Where(x => x.QuoteId == query.QuoteId);
        }

        if (query.Status.HasValue)
        {
            invoices = invoices.Where(x => x.Status == query.Status.Value);
        }

        if (query.DueBefore.HasValue)
        {
            invoices = invoices.Where(x => x.DueDate.HasValue && x.DueDate.Value <= query.DueBefore.Value);
        }

        var results = await invoices
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        if (query.UnpaidOnly)
        {
            results = results
                .Where(x => x.Status is not InvoiceStatus.Paid and not InvoiceStatus.Cancelled and not InvoiceStatus.Rejected)
                .ToList();
        }

        return results;
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Invoices.FirstOrDefaultAsync(x => x.Id == invoice.Id, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        existing.SourceSystem = invoice.SourceSystem;
        existing.ExternalSourceReference = invoice.ExternalSourceReference;
        existing.InvoiceNumber = invoice.InvoiceNumber;
        existing.SupplierId = invoice.SupplierId;
        existing.SupplierName = invoice.SupplierName;
        existing.BookingId = invoice.BookingId;
        existing.BookingItemId = invoice.BookingItemId;
        existing.QuoteId = invoice.QuoteId;
        existing.EmailThreadId = invoice.EmailThreadId;
        existing.ReviewTaskId = invoice.ReviewTaskId;
        existing.InvoiceDate = invoice.InvoiceDate;
        existing.DueDate = invoice.DueDate;
        existing.Currency = invoice.Currency;
        existing.SubtotalAmount = invoice.SubtotalAmount;
        existing.TaxAmount = invoice.TaxAmount;
        existing.TotalAmount = invoice.TotalAmount;
        existing.RebateAmount = invoice.RebateAmount;
        existing.RebateAppliedAt = invoice.RebateAppliedAt;
        existing.Notes = invoice.Notes;
        existing.RawExtractionPayloadJson = invoice.RawExtractionPayloadJson;
        existing.NormalizedSnapshotJson = invoice.NormalizedSnapshotJson;
        existing.ExtractionConfidence = invoice.ExtractionConfidence;
        existing.ExtractionIssuesJson = invoice.ExtractionIssuesJson;
        existing.UnresolvedFieldsJson = invoice.UnresolvedFieldsJson;
        existing.RequiresHumanReview = invoice.RequiresHumanReview;
        existing.Status = invoice.Status;
        existing.ReceivedAt = invoice.ReceivedAt;
        existing.CreatedAt = invoice.CreatedAt;
        existing.UpdatedAt = invoice.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid id, InvoiceStatus status, string? notes, DateTime updatedAt, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Invoices.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        existing.Status = status;
        existing.Notes = notes;
        existing.UpdatedAt = updatedAt;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Invoice> BuildInvoiceQuery() =>
        dbContext.Invoices
            .Include(x => x.Supplier)
            .Include(x => x.Booking)
            .Include(x => x.BookingItem)
                .ThenInclude(x => x!.Supplier)
            .Include(x => x.Quote)
            .Include(x => x.EmailThread)
            .Include(x => x.ReviewTask)
            .Include(x => x.LineItems.OrderBy(li => li.ServiceDate).ThenBy(li => li.Description))
            .Include(x => x.Attachments.OrderBy(a => a.CreatedAt))
            .Include(x => x.PaymentRecords.OrderByDescending(p => p.PaidAt))
            .AsNoTracking();
}
