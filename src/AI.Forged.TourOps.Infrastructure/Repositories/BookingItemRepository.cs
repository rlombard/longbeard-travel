using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class BookingItemRepository(AppDbContext dbContext) : IBookingItemRepository
{
    public async Task<BookingItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.BookingItems
            .Include(x => x.Product)
            .Include(x => x.Supplier)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateStatusAsync(Guid id, BookingItemStatus status, CancellationToken cancellationToken = default)
    {
        var bookingItem = await dbContext.BookingItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Booking item not found.");

        bookingItem.Status = status;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateNotesAsync(Guid id, string? notes, CancellationToken cancellationToken = default)
    {
        var bookingItem = await dbContext.BookingItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Booking item not found.");

        bookingItem.Notes = notes;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
