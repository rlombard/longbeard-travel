using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Services;

public class RateService(IRateRepository rateRepository, IProductRepository productRepository) : IRateService
{
    public async Task<Rate> CreateRateAsync(Rate rate, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(rate.ProductId, cancellationToken);
        if (product is null)
        {
            throw new InvalidOperationException("Product not found.");
        }

        if (rate.SeasonEnd < rate.SeasonStart)
        {
            throw new ArgumentException("Season end must be on or after season start.");
        }

        rate.Id = Guid.NewGuid();
        rate.CreatedAt = DateTime.UtcNow;
        return await rateRepository.AddAsync(rate, cancellationToken);
    }

    public Task<IReadOnlyList<Rate>> GetRatesByProductAsync(Guid productId, CancellationToken cancellationToken = default) =>
        rateRepository.GetByProductIdAsync(productId, cancellationToken);
}
