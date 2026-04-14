using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Infrastructure.Data;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class LlmAuditLogRepository(AppDbContext dbContext) : ILlmAuditLogRepository
{
    public async Task AddAsync(LlmAuditLog log, CancellationToken cancellationToken = default)
    {
        dbContext.LlmAuditLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
