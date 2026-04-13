namespace AI.Forged.TourOps.Application.Models;

public sealed record PricingRequest(Guid ItineraryId, int Pax, string Currency, decimal MarkupPercentage);
