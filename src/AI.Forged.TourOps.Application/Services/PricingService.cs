using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services;

public class PricingService(IItineraryRepository itineraryRepository, IRateRepository rateRepository) : IPricingService
{
    public async Task<Quote> GenerateQuoteAsync(PricingRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            throw new InvalidOperationException("Currency is required.");
        }

        var itinerary = await itineraryRepository.GetByIdAsync(request.ItineraryId, cancellationToken)
            ?? throw new InvalidOperationException("Itinerary not found.");

        var lineItems = new List<QuoteLineItem>();
        decimal totalCost = 0m;
        decimal totalPrice = 0m;

        foreach (var item in itinerary.Items)
        {
            var travelDate = itinerary.StartDate.AddDays(item.DayNumber - 1);
            var rate = await rateRepository.GetApplicableRateAsync(item.ProductId, travelDate, request.Currency, cancellationToken)
                ?? throw new InvalidOperationException($"No applicable rate found for product '{item.ProductId}' on {travelDate:yyyy-MM-dd}.");

            var baseCost = CalculateBaseCost(rate, request.Pax, item.Quantity);
            var adjustedCost = ApplyAdjustments(baseCost, rate, request.Pax);
            var finalPrice = adjustedCost * (1 + request.MarkupPercentage);

            totalCost += adjustedCost;
            totalPrice += finalPrice;

            lineItems.Add(new QuoteLineItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                BaseCost = decimal.Round(baseCost, 2),
                AdjustedCost = decimal.Round(adjustedCost, 2),
                FinalPrice = decimal.Round(finalPrice, 2),
                MarkupPercentage = request.MarkupPercentage,
                Currency = request.Currency
            });
        }

        var quote = new Quote
        {
            Id = Guid.NewGuid(),
            ItineraryId = itinerary.Id,
            Currency = request.Currency,
            Status = QuoteStatus.Generated,
            TotalCost = decimal.Round(totalCost, 2),
            TotalPrice = decimal.Round(totalPrice, 2),
            Margin = decimal.Round(totalPrice - totalCost, 2),
            CreatedAt = DateTime.UtcNow,
            LineItems = lineItems
        };

        foreach (var lineItem in quote.LineItems)
        {
            lineItem.QuoteId = quote.Id;
        }

        return quote;
    }

    private static decimal CalculateBaseCost(Rate rate, int pax, int quantity)
    {
        return rate.PricingModel switch
        {
            PricingModel.PerPerson => rate.BaseCost * pax * quantity,
            PricingModel.PerGroup => rate.BaseCost * quantity,
            PricingModel.PerUnit => rate.BaseCost * Math.Max(1, rate.Capacity.HasValue ? (int)Math.Ceiling((double)pax / rate.Capacity.Value) : throw new InvalidOperationException("Capacity is required for PerUnit pricing.")) * quantity,
            _ => throw new InvalidOperationException("Unsupported pricing model.")
        };
    }

    private static decimal ApplyAdjustments(decimal baseCost, Rate rate, int pax)
    {
        if (rate.MaxPax.HasValue && pax > rate.MaxPax.Value)
        {
            throw new InvalidOperationException($"Pax {pax} exceeds maximum allowed pax {rate.MaxPax.Value}.");
        }

        var adjusted = baseCost;

        if (rate.MinPax.HasValue && pax < rate.MinPax.Value)
        {
            var shortfall = rate.MinPax.Value - pax;
            adjusted += shortfall * rate.BaseCost;
        }

        if (rate.ChildDiscount.HasValue)
        {
            adjusted -= adjusted * rate.ChildDiscount.Value;
        }

        if (rate.SingleSupplement.HasValue && pax == 1)
        {
            adjusted += rate.SingleSupplement.Value;
        }

        return adjusted;
    }
}
