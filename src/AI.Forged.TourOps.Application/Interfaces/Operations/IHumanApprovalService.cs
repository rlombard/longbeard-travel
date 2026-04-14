using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces.Operations;

public interface IHumanApprovalService
{
    Task<HumanApprovalRequest> CreateApprovalRequestAsync(string actionType, string entityType, Guid entityId, string? payloadJson, CancellationToken cancellationToken = default);
    Task<HumanApprovalRequest> ApproveActionAsync(Guid approvalRequestId, string? decisionNotes, CancellationToken cancellationToken = default);
    Task<HumanApprovalRequest> RejectActionAsync(Guid approvalRequestId, string? decisionNotes, CancellationToken cancellationToken = default);
}
