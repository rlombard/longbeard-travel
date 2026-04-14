using AI.Forged.TourOps.Application.Models.Operations;

namespace AI.Forged.TourOps.Application.Interfaces.Ai;

public interface IBookingAiService
{
    Task<IReadOnlyList<SuggestedTaskCandidate>> GenerateSuggestedTasksAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
