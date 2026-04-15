using AI.Forged.TourOps.Application.Models.Platform;

namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface IUsageMeteringService
{
    Task RecordAsync(MeterUsageModel model, CancellationToken cancellationToken = default);
}
