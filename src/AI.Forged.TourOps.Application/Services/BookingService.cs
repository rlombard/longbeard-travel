using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services;

public class BookingService(IBookingRepository bookingRepository, IQuoteRepository quoteRepository) : IBookingService
{
    public Task<IReadOnlyList<Booking>> GetBookingsAsync(CancellationToken cancellationToken = default) =>
        bookingRepository.GetAllAsync(cancellationToken);

    public async Task<Booking> CreateBookingFromQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default)
    {
        var quote = await quoteRepository.GetByIdForBookingAsync(quoteId, cancellationToken)
            ?? throw new InvalidOperationException("Quote not found.");

        if (quote.LineItems.Count == 0)
        {
            throw new InvalidOperationException("Cannot create a booking from a quote without line items.");
        }

        var existingBooking = await bookingRepository.GetByQuoteIdAsync(quoteId, cancellationToken);
        if (existingBooking is not null)
        {
            throw new InvalidOperationException("A booking already exists for this quote.");
        }

        var createdAt = DateTime.UtcNow;
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            QuoteId = quote.Id,
            LeadCustomerId = quote.LeadCustomerId,
            Status = BookingStatus.Draft,
            CreatedAt = createdAt,
            Items = quote.LineItems.Select(lineItem => new BookingItem
            {
                Id = Guid.NewGuid(),
                BookingId = Guid.Empty,
                ProductId = lineItem.ProductId,
                SupplierId = lineItem.Product.SupplierId,
                Status = BookingItemStatus.Pending,
                Notes = null,
                CreatedAt = createdAt
            }).ToList()
        };

        foreach (var item in booking.Items)
        {
            item.BookingId = booking.Id;
        }

        await bookingRepository.AddAsync(booking, cancellationToken);

        return await bookingRepository.GetByIdAsync(booking.Id, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found after creation.");
    }

    public Task<Booking?> GetBookingAsync(Guid id, CancellationToken cancellationToken = default) =>
        bookingRepository.GetByIdAsync(id, cancellationToken);

    public async Task<Booking> UpdateBookingStatusAsync(Guid id, BookingStatus status, CancellationToken cancellationToken = default)
    {
        var booking = await bookingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        if (!IsValidStatusTransition(booking.Status, status))
        {
            throw new InvalidOperationException($"Invalid booking status transition from '{booking.Status}' to '{status}'.");
        }

        await bookingRepository.UpdateStatusAsync(id, status, cancellationToken);

        return await bookingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found after update.");
    }

    private static bool IsValidStatusTransition(BookingStatus currentStatus, BookingStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return true;
        }

        return currentStatus switch
        {
            BookingStatus.Draft => nextStatus is BookingStatus.Confirmed or BookingStatus.Cancelled,
            BookingStatus.Confirmed => nextStatus == BookingStatus.Cancelled,
            BookingStatus.Cancelled => false,
            _ => false
        };
    }
}
