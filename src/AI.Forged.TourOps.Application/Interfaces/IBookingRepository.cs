using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Booking?> GetByQuoteIdAsync(Guid quoteId, CancellationToken cancellationToken = default);
    Task UpdateLeadCustomerAsync(Guid id, Guid? customerId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid id, BookingStatus status, CancellationToken cancellationToken = default);
}
