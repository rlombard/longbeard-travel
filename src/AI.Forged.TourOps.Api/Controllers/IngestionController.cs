using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Route("api/ingestion")]
public class IngestionController(IIngestionService ingestionService) : ControllerBase
{
    [HttpPost("rates")]
    public async Task<ActionResult<RateResponse>> ProcessRatePayload([FromBody] IngestionRatePayloadRequest request, CancellationToken cancellationToken)
    {
        var rate = await ingestionService.ProcessRatePayloadAsync(request.ToPayload(), cancellationToken);
        return Ok(rate.ToResponse());
    }
}
