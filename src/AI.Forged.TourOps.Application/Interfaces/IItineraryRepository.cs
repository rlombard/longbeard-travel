using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IItineraryRepository
{
    Task<Itinerary> AddAsync(Itinerary itinerary, IEnumerable<ItineraryItem> items, CancellationToken cancellationToken = default);
    Task<Itinerary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ItineraryDraft> AddDraftAsync(ItineraryDraft draft, IEnumerable<ItineraryDraftItem> items, CancellationToken cancellationToken = default);
    Task<ItineraryDraft?> GetDraftByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateLeadCustomerAsync(Guid id, Guid? customerId, CancellationToken cancellationToken = default);
    Task UpdateDraftAsync(ItineraryDraft draft, CancellationToken cancellationToken = default);
}
