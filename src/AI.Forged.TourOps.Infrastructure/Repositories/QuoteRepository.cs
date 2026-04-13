using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class QuoteRepository(AppDbContext dbContext) : IQuoteRepository
{
    public async Task<Quote> AddAsync(Quote quote, CancellationToken cancellationToken = default)
    {
        dbContext.Quotes.Add(quote);
        dbContext.QuoteLineItems.AddRange(quote.LineItems);
        await dbContext.SaveChangesAsync(cancellationToken);
        return quote;
    }

    public async Task<Quote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Quotes
            .Include(x => x.LineItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
