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
            .Include(x => x.LeadCustomer)
            .Include(x => x.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<ItineraryDraft> AddDraftAsync(ItineraryDraft draft, IEnumerable<ItineraryDraftItem> items, CancellationToken cancellationToken = default)
    {
        dbContext.ItineraryDrafts.Add(draft);
        dbContext.ItineraryDraftItems.AddRange(items);
        await dbContext.SaveChangesAsync(cancellationToken);
        draft.Items = items.ToList();
        return draft;
    }

    public async Task<ItineraryDraft?> GetDraftByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.ItineraryDrafts
            .Include(x => x.Items.OrderBy(item => item.DayNumber).ThenBy(item => item.Sequence))
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateDraftAsync(ItineraryDraft draft, CancellationToken cancellationToken = default)
    {
        var existingDraft = await dbContext.ItineraryDrafts.FirstOrDefaultAsync(x => x.Id == draft.Id, cancellationToken)
            ?? throw new InvalidOperationException("Itinerary draft not found.");

        dbContext.Entry(existingDraft).CurrentValues.SetValues(draft);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateLeadCustomerAsync(Guid id, Guid? customerId, CancellationToken cancellationToken = default)
    {
        var itinerary = await dbContext.Itineraries.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Itinerary not found.");

        itinerary.LeadCustomerId = customerId;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
