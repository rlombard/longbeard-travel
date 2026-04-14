using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface ILlmAuditLogRepository
{
    Task AddAsync(LlmAuditLog log, CancellationToken cancellationToken = default);
}
