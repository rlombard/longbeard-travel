using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IRateRepository
{
    Task<Rate> AddAsync(Rate rate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Rate>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Rate?> GetApplicableRateAsync(Guid productId, DateOnly date, string currency, CancellationToken cancellationToken = default);
}
