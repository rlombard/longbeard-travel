using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class HumanApprovalRepository(AppDbContext dbContext) : IHumanApprovalRepository
{
    public async Task<HumanApprovalRequest> AddAsync(HumanApprovalRequest request, CancellationToken cancellationToken = default)
    {
        dbContext.HumanApprovalRequests.Add(request);
        await dbContext.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<HumanApprovalRequest?> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default) =>
        await dbContext.HumanApprovalRequests
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.EntityType == entityType && x.EntityId == entityId, cancellationToken);

    public async Task<HumanApprovalRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.HumanApprovalRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(HumanApprovalRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.HumanApprovalRequests.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Approval request not found.");

        existing.Status = request.Status;
        existing.ReviewedByUserId = request.ReviewedByUserId;
        existing.DecisionNotes = request.DecisionNotes;
        existing.ReviewedAt = request.ReviewedAt;
        existing.PayloadJson = request.PayloadJson;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
