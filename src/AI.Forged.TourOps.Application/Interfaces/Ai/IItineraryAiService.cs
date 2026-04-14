using AI.Forged.TourOps.Application.Models.Itineraries;

namespace AI.Forged.TourOps.Application.Interfaces.Ai;

public interface IItineraryAiService
{
    Task<ItineraryProductAssistResult> GetProductAssistanceAsync(ItineraryProductAssistRequest request, CancellationToken cancellationToken = default);
    Task<ItineraryDraftModel> GenerateDraftAsync(GenerateItineraryDraftRequest request, CancellationToken cancellationToken = default);
    Task<ItineraryDraftApprovalResult> ApproveDraftAsync(Guid draftId, ApproveItineraryDraftRequest request, CancellationToken cancellationToken = default);
}
