using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Route("api/quotes")]
public class QuoteController(IQuoteService quoteService) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<ActionResult<QuoteResponse>> GenerateQuote([FromBody] GenerateQuoteRequest request, CancellationToken cancellationToken)
    {
        var quote = await quoteService.GenerateQuoteAsync(request.ItineraryId, request.Pax, request.Currency, request.Markup, cancellationToken);
        return Ok(quote.ToResponse());
    }

    [HttpGet("{quoteId:guid}")]
    public async Task<ActionResult<QuoteResponse>> GetQuote(Guid quoteId, CancellationToken cancellationToken)
    {
        var quote = await quoteService.GetQuoteAsync(quoteId, cancellationToken);
        return quote is null ? NotFound() : Ok(quote.ToResponse());
    }
}
