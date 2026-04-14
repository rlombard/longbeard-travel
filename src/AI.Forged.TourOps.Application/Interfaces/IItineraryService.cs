using AI.Forged.TourOps.Application.Models.Itineraries;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IItineraryService
{
    Task<ItineraryModel> CreateItineraryAsync(CreateItineraryModel request, CancellationToken cancellationToken = default);
    Task<ItineraryModel?> GetItineraryAsync(Guid itineraryId, CancellationToken cancellationToken = default);
}
