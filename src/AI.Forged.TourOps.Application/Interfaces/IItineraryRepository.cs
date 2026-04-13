using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IItineraryRepository
{
    Task<Itinerary> AddAsync(Itinerary itinerary, IEnumerable<ItineraryItem> items, CancellationToken cancellationToken = default);
    Task<Itinerary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
