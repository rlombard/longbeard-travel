using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Services;

public class IngestionService(IRateService rateService) : IIngestionService
{
    public async Task<Rate> ProcessRatePayloadAsync(IngestionRatePayload payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payload.Currency))
        {
            throw new InvalidOperationException("Currency is required.");
        }

        var rate = new Rate
        {
            ProductId = payload.ProductId,
            SeasonStart = payload.SeasonStart,
            SeasonEnd = payload.SeasonEnd,
            PricingModel = payload.PricingModel,
            BaseCost = payload.BaseCost,
            Currency = payload.Currency,
            MinPax = payload.MinPax,
            MaxPax = payload.MaxPax,
            ChildDiscount = payload.ChildDiscount,
            SingleSupplement = payload.SingleSupplement,
            Capacity = payload.Capacity
        };

        return await rateService.CreateRateAsync(rate, cancellationToken);
    }
}
