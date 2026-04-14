using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IQuoteRepository
{
    Task<Quote> AddAsync(Quote quote, CancellationToken cancellationToken = default);
    Task<Quote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Quote?> GetByIdForBookingAsync(Guid id, CancellationToken cancellationToken = default);
}
