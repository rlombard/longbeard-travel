using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IQuoteService
{
    Task<Quote> GenerateQuoteAsync(Guid itineraryId, int pax, string currency, decimal markupPercentage, CancellationToken cancellationToken = default);
    Task<Quote?> GetQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default);
}
