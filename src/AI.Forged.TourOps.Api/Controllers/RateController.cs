using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Route("api/rates")]
public class RateController(IRateService rateService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RateResponse>> CreateRate([FromBody] RateRequest request, CancellationToken cancellationToken)
    {
        var rate = await rateService.CreateRateAsync(request.ToEntity(), cancellationToken);

        return CreatedAtAction(nameof(GetRatesByProduct), new { productId = rate.ProductId }, rate.ToResponse());
    }

    [HttpGet("{rateId:guid}")]
    public async Task<ActionResult<RateResponse>> GetRate(Guid rateId, CancellationToken cancellationToken)
    {
        var rate = await rateService.GetRateAsync(rateId, cancellationToken);
        return rate is null ? NotFound() : Ok(rate.ToResponse());
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<IReadOnlyList<RateResponse>>> GetRatesByProduct(Guid productId, CancellationToken cancellationToken)
    {
        var rates = await rateService.GetRatesByProductAsync(productId, cancellationToken);
        return Ok(rates.Select(x => x.ToResponse()).ToList());
    }

    [HttpPut("{rateId:guid}")]
    public async Task<ActionResult<RateResponse>> UpdateRate(Guid rateId, [FromBody] RateRequest request, CancellationToken cancellationToken)
    {
        var rate = await rateService.UpdateRateAsync(rateId, request.ToEntity(), cancellationToken);
        return Ok(rate.ToResponse());
    }
}
