using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IBookingItemRepository
{
    Task<BookingItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid id, BookingItemStatus status, CancellationToken cancellationToken = default);
    Task UpdateNotesAsync(Guid id, string? notes, CancellationToken cancellationToken = default);
}
