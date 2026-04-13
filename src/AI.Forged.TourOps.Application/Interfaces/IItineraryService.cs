using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IItineraryService
{
    Task<Itinerary> CreateItineraryAsync(Itinerary itinerary, IEnumerable<ItineraryItem> items, CancellationToken cancellationToken = default);
    Task<Itinerary?> GetItineraryAsync(Guid itineraryId, CancellationToken cancellationToken = default);
}
