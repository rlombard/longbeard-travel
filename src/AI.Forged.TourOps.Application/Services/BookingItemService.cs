using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services;

public class BookingItemService(IBookingItemRepository bookingItemRepository) : IBookingItemService
{
    public async Task<BookingItem> UpdateBookingItemStatusAsync(Guid id, BookingItemStatus status, CancellationToken cancellationToken = default)
    {
        var bookingItem = await bookingItemRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Booking item not found.");

        if (!IsValidStatusTransition(bookingItem.Status, status))
        {
            throw new InvalidOperationException($"Invalid booking item status transition from '{bookingItem.Status}' to '{status}'.");
        }

        await bookingItemRepository.UpdateStatusAsync(id, status, cancellationToken);

        return await bookingItemRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Booking item not found after update.");
    }

    public async Task<BookingItem> AddNoteAsync(Guid id, string? note, CancellationToken cancellationToken = default)
    {
        var bookingItem = await bookingItemRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Booking item not found.");

        var trimmedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (trimmedNote is { Length: > 2000 })
        {
            throw new InvalidOperationException("Booking item notes cannot exceed 2000 characters.");
        }

        await bookingItemRepository.UpdateNotesAsync(id, trimmedNote, cancellationToken);

        return await bookingItemRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Booking item not found after update.");
    }

    private static bool IsValidStatusTransition(BookingItemStatus currentStatus, BookingItemStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return true;
        }

        if (nextStatus == BookingItemStatus.Cancelled)
        {
            return true;
        }

        return currentStatus switch
        {
            BookingItemStatus.Pending => nextStatus == BookingItemStatus.Requested,
            BookingItemStatus.Requested => nextStatus == BookingItemStatus.Confirmed,
            _ => false
        };
    }
}
