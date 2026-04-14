using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IBookingService
{
    Task<IReadOnlyList<Booking>> GetBookingsAsync(CancellationToken cancellationToken = default);
    Task<Booking> CreateBookingFromQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default);
    Task<Booking?> GetBookingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Booking> UpdateBookingStatusAsync(Guid id, BookingStatus status, CancellationToken cancellationToken = default);
}
