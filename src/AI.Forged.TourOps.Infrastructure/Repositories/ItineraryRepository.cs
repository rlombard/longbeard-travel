using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class ItineraryRepository(AppDbContext dbContext) : IItineraryRepository
{
    public async Task<Itinerary> AddAsync(Itinerary itinerary, IEnumerable<ItineraryItem> items, CancellationToken cancellationToken = default)
    {
        dbContext.Itineraries.Add(itinerary);
        dbContext.ItineraryItems.AddRange(items);
        await dbContext.SaveChangesAsync(cancellationToken);
        itinerary.Items = items.ToList();
        return itinerary;
    }

    public async Task<Itinerary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Itineraries
            .Include(x => x.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
