using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/invoices")]
public class InvoiceController(IInvoiceService invoiceService) : ControllerBase
{
    [HttpPost("ingest")]
    public async Task<ActionResult<InvoiceIngestionResponse>> IngestInvoice([FromBody] InvoiceIngestionRequest request, CancellationToken cancellationToken)
    {
        var result = await invoiceService.IngestInvoiceAsync(request.ToModel(), cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceResponse>> GetInvoice(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetInvoiceAsync(id, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvoiceListResponse>>> GetInvoices(
        [FromQuery] Guid? supplierId,
        [FromQuery] Guid? bookingId,
        [FromQuery] Guid? bookingItemId,
        [FromQuery] Guid? quoteId,
        [FromQuery] InvoiceStatus? status,
        [FromQuery] DateOnly? dueBefore,
        [FromQuery] bool unpaidOnly,
        CancellationToken cancellationToken)
    {
        var invoices = await invoiceService.GetInvoicesAsync(new AI.Forged.TourOps.Application.Models.Invoices.InvoiceListQueryModel
        {
            SupplierId = supplierId,
            BookingId = bookingId,
            BookingItemId = bookingItemId,
            QuoteId = quoteId,
            Status = status,
            DueBefore = dueBefore,
            UnpaidOnly = unpaidOnly
        }, cancellationToken);

        return Ok(invoices.Select(x => x.ToResponse()).ToList());
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<InvoiceResponse>> UpdateStatus(Guid id, [FromBody] UpdateInvoiceStatusRequest request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.UpdateInvoiceStatusAsync(id, request.ToModel(), cancellationToken);
        return Ok(invoice.ToResponse());
    }

    [HttpPatch("{id:guid}/links")]
    public async Task<ActionResult<InvoiceResponse>> Relink(Guid id, [FromBody] RelinkInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.RelinkInvoiceAsync(id, request.ToModel(), cancellationToken);
        return Ok(invoice.ToResponse());
    }

    [HttpPost("{id:guid}/payments")]
    public async Task<ActionResult<InvoiceResponse>> RecordPayment(Guid id, [FromBody] RecordInvoicePaymentRequest request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.RecordPaymentAsync(id, request.ToModel(), cancellationToken);
        return Ok(invoice.ToResponse());
    }

    [HttpPost("{id:guid}/rebate/apply")]
    public async Task<ActionResult<InvoiceResponse>> ApplyRebate(Guid id, [FromBody] ApplyInvoiceRebateRequest request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.ApplyRebateAsync(id, request.ToModel(), cancellationToken);
        return Ok(invoice.ToResponse());
    }
}
