using AI.Forged.TourOps.Application.Models;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IPricingService
{
    Task<Quote> GenerateQuoteAsync(PricingRequest request, CancellationToken cancellationToken = default);
}
