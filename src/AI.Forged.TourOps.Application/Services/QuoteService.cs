using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Services;

public class QuoteService(IPricingService pricingService, IQuoteRepository quoteRepository) : IQuoteService
{
    public async Task<Quote> GenerateQuoteAsync(Guid itineraryId, int pax, string currency, decimal markupPercentage, CancellationToken cancellationToken = default)
    {
        var quote = await pricingService.GenerateQuoteAsync(new PricingRequest(itineraryId, pax, currency, markupPercentage), cancellationToken);
        return await quoteRepository.AddAsync(quote, cancellationToken);
    }

    public Task<Quote?> GetQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default) =>
        quoteRepository.GetByIdAsync(quoteId, cancellationToken);
}
