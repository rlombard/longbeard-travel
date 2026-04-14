using AI.Forged.TourOps.Application.Models.Invoices;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice> AddAsync(
        Invoice invoice,
        IEnumerable<InvoiceLineItem> lineItems,
        IEnumerable<InvoiceAttachment> attachments,
        CancellationToken cancellationToken = default);
    Task<PaymentRecord> AddPaymentRecordAsync(PaymentRecord paymentRecord, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetBySourceAsync(string sourceSystem, string externalSourceReference, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invoice>> GetInvoicesAsync(InvoiceListQueryModel query, CancellationToken cancellationToken = default);
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid id, InvoiceStatus status, string? notes, DateTime updatedAt, CancellationToken cancellationToken = default);
}
