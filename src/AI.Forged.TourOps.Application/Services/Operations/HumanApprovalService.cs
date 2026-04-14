using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Operations;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services.Operations;

public class HumanApprovalService(IHumanApprovalRepository repository, ICurrentUserContext currentUserContext) : IHumanApprovalService
{
    public Task<HumanApprovalRequest> CreateApprovalRequestAsync(string actionType, string entityType, Guid entityId, string? payloadJson, CancellationToken cancellationToken = default) =>
        repository.AddAsync(new HumanApprovalRequest
        {
            Id = Guid.NewGuid(),
            ActionType = Normalize(actionType, "Action type is required.", 128),
            EntityType = Normalize(entityType, "Entity type is required.", 128),
            EntityId = entityId,
            RequestedByUserId = currentUserContext.GetRequiredUserId(),
            Status = HumanApprovalStatus.Pending,
            PayloadJson = NormalizeOptional(payloadJson, 8000),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

    public async Task<HumanApprovalRequest> ApproveActionAsync(Guid approvalRequestId, string? decisionNotes, CancellationToken cancellationToken = default)
    {
        var request = await repository.GetByIdAsync(approvalRequestId, cancellationToken)
            ?? throw new InvalidOperationException("Approval request not found.");

        request.Status = HumanApprovalStatus.Approved;
        request.ReviewedByUserId = currentUserContext.GetRequiredUserId();
        request.DecisionNotes = NormalizeOptional(decisionNotes, 4000);
        request.ReviewedAt = DateTime.UtcNow;
        await repository.UpdateAsync(request, cancellationToken);
        return request;
    }

    public async Task<HumanApprovalRequest> RejectActionAsync(Guid approvalRequestId, string? decisionNotes, CancellationToken cancellationToken = default)
    {
        var request = await repository.GetByIdAsync(approvalRequestId, cancellationToken)
            ?? throw new InvalidOperationException("Approval request not found.");

        request.Status = HumanApprovalStatus.Rejected;
        request.ReviewedByUserId = currentUserContext.GetRequiredUserId();
        request.DecisionNotes = NormalizeOptional(decisionNotes, 4000);
        request.ReviewedAt = DateTime.UtcNow;
        await repository.UpdateAsync(request, cancellationToken);
        return request;
    }

    private static string Normalize(string? value, string message, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(message);
        }

        if (normalized.Length > maxLength)
        {
            throw new InvalidOperationException($"Value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized is { Length: > 0 } && normalized.Length > maxLength)
        {
            throw new InvalidOperationException($"Value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }
}
