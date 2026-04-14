using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface ITaskSuggestionRepository
{
    Task AddRangeAsync(IEnumerable<OperationalTaskSuggestion> suggestions, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OperationalTaskSuggestion>> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<OperationalTaskSuggestion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeletePendingByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task UpdateAsync(OperationalTaskSuggestion suggestion, CancellationToken cancellationToken = default);
}
