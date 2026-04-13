using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Services;

public class ItineraryService(IItineraryRepository itineraryRepository) : IItineraryService
{
    public async Task<Itinerary> CreateItineraryAsync(Itinerary itinerary, IEnumerable<ItineraryItem> items, CancellationToken cancellationToken = default)
    {
        if (itinerary.Duration <= 0)
        {
            throw new ArgumentException("Duration must be greater than zero.");
        }

        var itineraryItems = items.ToList();
        if (itineraryItems.Count == 0)
        {
            throw new ArgumentException("At least one itinerary item is required.");
        }

        itinerary.Id = Guid.NewGuid();
        itinerary.CreatedAt = DateTime.UtcNow;

        foreach (var item in itineraryItems)
        {
            item.Id = Guid.NewGuid();
            item.ItineraryId = itinerary.Id;
        }

        return await itineraryRepository.AddAsync(itinerary, itineraryItems, cancellationToken);
    }

    public Task<Itinerary?> GetItineraryAsync(Guid itineraryId, CancellationToken cancellationToken = default) =>
        itineraryRepository.GetByIdAsync(itineraryId, cancellationToken);
}
