using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces.Tasks;

public interface IBookingTaskSuggestionService
{
    Task<IReadOnlyList<OperationalTaskSuggestion>> GetSuggestedTasksAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OperationalTaskSuggestion>> GenerateSuggestedTasksAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OperationalTaskSuggestion>> RegenerateSuggestedTasksAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<OperationalTask> AcceptSuggestedTaskAsync(Guid suggestionId, string assignedToUserId, CancellationToken cancellationToken = default);
    Task<OperationalTaskSuggestion> RejectSuggestedTaskAsync(Guid suggestionId, CancellationToken cancellationToken = default);
}
