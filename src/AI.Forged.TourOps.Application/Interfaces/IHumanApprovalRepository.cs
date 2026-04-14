using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IHumanApprovalRepository
{
    Task<HumanApprovalRequest> AddAsync(HumanApprovalRequest request, CancellationToken cancellationToken = default);
    Task<HumanApprovalRequest?> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task<HumanApprovalRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(HumanApprovalRequest request, CancellationToken cancellationToken = default);
}
