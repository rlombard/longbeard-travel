using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Models;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Services;

public class QuoteService(
    IPricingService pricingService,
    IQuoteRepository quoteRepository,
    IUsageMeteringService? usageMeteringService = null) : IQuoteService
{
    public async Task<Quote> GenerateQuoteAsync(Guid itineraryId, int pax, string currency, decimal markupPercentage, CancellationToken cancellationToken = default)
    {
        var quote = await pricingService.GenerateQuoteAsync(new PricingRequest(itineraryId, pax, currency, markupPercentage), cancellationToken);
        var created = await quoteRepository.AddAsync(quote, cancellationToken);
        if (usageMeteringService is not null)
        {
            await usageMeteringService.RecordAsync(new AI.Forged.TourOps.Application.Models.Platform.MeterUsageModel
            {
                Category = "Commercial",
                MetricKey = "quotes.generated",
                Quantity = 1,
                Unit = "quote",
                Source = "QuoteService",
                ReferenceEntityType = nameof(Quote),
                ReferenceEntityId = created.Id,
                IsBillable = false
            }, cancellationToken);
        }

        return created;
    }

    public Task<Quote?> GetQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default) =>
        quoteRepository.GetByIdAsync(quoteId, cancellationToken);
}
