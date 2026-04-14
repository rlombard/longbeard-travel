using AI.Forged.TourOps.Application.Models.Invoices;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceIngestionResultModel> IngestInvoiceAsync(InvoiceIngestionRequestModel request, CancellationToken cancellationToken = default);
    Task<InvoiceModel?> GetInvoiceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvoiceListItemModel>> GetInvoicesAsync(InvoiceListQueryModel query, CancellationToken cancellationToken = default);
    Task<InvoiceModel> UpdateInvoiceStatusAsync(Guid id, UpdateInvoiceStatusModel request, CancellationToken cancellationToken = default);
    Task<InvoiceModel> RelinkInvoiceAsync(Guid id, RelinkInvoiceModel request, CancellationToken cancellationToken = default);
    Task<InvoiceModel> RecordPaymentAsync(Guid id, RecordInvoicePaymentModel request, CancellationToken cancellationToken = default);
    Task<InvoiceModel> ApplyRebateAsync(Guid id, ApplyInvoiceRebateModel request, CancellationToken cancellationToken = default);
}
