using AI.Forged.TourOps.Application.Models.Platform;

namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface IAuditService
{
    Task WriteAsync(AuditEventCreateModel model, CancellationToken cancellationToken = default);
}
