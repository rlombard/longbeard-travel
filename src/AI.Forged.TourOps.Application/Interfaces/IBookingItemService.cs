using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IBookingItemService
{
    Task<BookingItem> UpdateBookingItemStatusAsync(Guid id, BookingItemStatus status, CancellationToken cancellationToken = default);
    Task<BookingItem> AddNoteAsync(Guid id, string? note, CancellationToken cancellationToken = default);
}
