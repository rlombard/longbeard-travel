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
        var rate = await rateService.CreateRateAsync(new Rate
        {
            ProductId = request.ProductId,
            SeasonStart = request.SeasonStart,
            SeasonEnd = request.SeasonEnd,
            PricingModel = request.PricingModel,
            BaseCost = request.BaseCost,
            Currency = request.Currency,
            MinPax = request.MinPax,
            MaxPax = request.MaxPax,
            ChildDiscount = request.ChildDiscount,
            SingleSupplement = request.SingleSupplement,
            Capacity = request.Capacity
        }, cancellationToken);

        return CreatedAtAction(nameof(GetRatesByProduct), new { productId = rate.ProductId }, rate.ToResponse());
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<IReadOnlyList<RateResponse>>> GetRatesByProduct(Guid productId, CancellationToken cancellationToken)
    {
        var rates = await rateService.GetRatesByProductAsync(productId, cancellationToken);
        return Ok(rates.Select(x => x.ToResponse()).ToList());
    }
}
