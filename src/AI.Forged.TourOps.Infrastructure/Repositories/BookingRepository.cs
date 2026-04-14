using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class BookingRepository(AppDbContext dbContext) : IBookingRepository
{
    public async Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync(cancellationToken);
        return booking;
    }

    public async Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Bookings
            .Include(x => x.LeadCustomer)
            .Include(x => x.Items)
            .Include(x => x.Travellers)
                .ThenInclude(x => x.Customer)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Bookings
            .Include(x => x.LeadCustomer)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .Include(x => x.Items)
                .ThenInclude(x => x.Supplier)
            .Include(x => x.Travellers)
                .ThenInclude(x => x.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Booking?> GetByQuoteIdAsync(Guid quoteId, CancellationToken cancellationToken = default) =>
        await dbContext.Bookings
            .Include(x => x.LeadCustomer)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .Include(x => x.Items)
                .ThenInclude(x => x.Supplier)
            .Include(x => x.Travellers)
                .ThenInclude(x => x.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.QuoteId == quoteId, cancellationToken);

    public async Task UpdateLeadCustomerAsync(Guid id, Guid? customerId, CancellationToken cancellationToken = default)
    {
        var booking = await dbContext.Bookings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        booking.LeadCustomerId = customerId;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid id, BookingStatus status, CancellationToken cancellationToken = default)
    {
        var booking = await dbContext.Bookings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        booking.Status = status;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
