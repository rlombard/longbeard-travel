using AI.Forged.TourOps.Application.Models;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Api.Models;

public static class ModelMappings
{
    public static SupplierResponse ToResponse(this Supplier supplier) => new()
    {
        Id = supplier.Id,
        Name = supplier.Name,
        Email = supplier.Email,
        Phone = supplier.Phone,
        CreatedAt = supplier.CreatedAt
    };

    public static ProductResponse ToResponse(this Product product) => new()
    {
        Id = product.Id,
        SupplierId = product.SupplierId,
        Name = product.Name,
        Type = product.Type,
        Metadata = product.Metadata,
        CreatedAt = product.CreatedAt
    };

    public static RateResponse ToResponse(this Rate rate) => new()
    {
        Id = rate.Id,
        ProductId = rate.ProductId,
        SeasonStart = rate.SeasonStart,
        SeasonEnd = rate.SeasonEnd,
        PricingModel = rate.PricingModel,
        BaseCost = rate.BaseCost,
        Currency = rate.Currency,
        MinPax = rate.MinPax,
        MaxPax = rate.MaxPax,
        ChildDiscount = rate.ChildDiscount,
        SingleSupplement = rate.SingleSupplement,
        Capacity = rate.Capacity,
        CreatedAt = rate.CreatedAt
    };

    public static ItineraryResponse ToResponse(this Itinerary itinerary) => new()
    {
        Id = itinerary.Id,
        StartDate = itinerary.StartDate,
        Duration = itinerary.Duration,
        CreatedAt = itinerary.CreatedAt,
        Items = itinerary.Items.Select(i => new ItineraryItemResponse
        {
            Id = i.Id,
            DayNumber = i.DayNumber,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            Notes = i.Notes
        }).ToList()
    };

    public static QuoteResponse ToResponse(this Quote quote) => new()
    {
        Id = quote.Id,
        ItineraryId = quote.ItineraryId,
        TotalCost = quote.TotalCost,
        TotalPrice = quote.TotalPrice,
        Margin = quote.Margin,
        Currency = quote.Currency,
        Status = quote.Status,
        CreatedAt = quote.CreatedAt,
        LineItems = quote.LineItems.Select(li => new QuoteLineItemResponse
        {
            ProductId = li.ProductId,
            BaseCost = li.BaseCost,
            AdjustedCost = li.AdjustedCost,
            FinalPrice = li.FinalPrice,
            MarkupPercentage = li.MarkupPercentage,
            Currency = li.Currency
        }).ToList()
    };

    public static IngestionRatePayload ToPayload(this IngestionRatePayloadRequest request) => new()
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
    };
}
