using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IRateService
{
    Task<Rate> CreateRateAsync(Rate rate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Rate>> GetRatesByProductAsync(Guid productId, CancellationToken cancellationToken = default);
}
