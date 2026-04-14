using AI.Forged.TourOps.Application.Models.Operations;

namespace AI.Forged.TourOps.Application.Interfaces.Operations;

public interface IOperationalDecisionSupportService
{
    Task<BookingStateAnalysis> AnalyzeBookingStateAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<BookingStateAnalysis> AnalyzeCommunicationStateAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> RecommendNextActionsAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
