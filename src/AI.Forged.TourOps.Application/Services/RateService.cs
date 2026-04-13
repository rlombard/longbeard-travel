using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Services;

public class RateService(IRateRepository rateRepository, IProductRepository productRepository) : IRateService
{
    public async Task<Rate> CreateRateAsync(Rate rate, CancellationToken cancellationToken = default)
    {
        await ValidateRateAsync(rate, cancellationToken);
        rate.Id = Guid.NewGuid();
        rate.IsActive = true;
        rate.PreviousRateId = null;
        rate.SupersededAt = null;
        rate.CreatedAt = DateTime.UtcNow;
        return await rateRepository.AddAsync(rate, cancellationToken);
    }

    public Task<Rate?> GetRateAsync(Guid rateId, CancellationToken cancellationToken = default) =>
        rateRepository.GetByIdAsync(rateId, cancellationToken);

    public Task<IReadOnlyList<Rate>> GetRatesByProductAsync(Guid productId, CancellationToken cancellationToken = default) =>
        rateRepository.GetByProductIdAsync(productId, cancellationToken);

    public async Task<Rate> UpdateRateAsync(Guid rateId, Rate rate, CancellationToken cancellationToken = default)
    {
        var existingRate = await rateRepository.GetByIdAsync(rateId, cancellationToken);
        if (existingRate is null)
        {
            throw new InvalidOperationException("Rate not found.");
        }

        var supersededAtUtc = DateTime.UtcNow;
        await rateRepository.MarkAsSupersededAsync(rateId, supersededAtUtc, cancellationToken);

        rate.Id = Guid.NewGuid();
        rate.IsActive = true;
        rate.PreviousRateId = existingRate.Id;
        rate.SupersededAt = null;
        rate.CreatedAt = supersededAtUtc;
        await ValidateRateAsync(rate, cancellationToken);
        return await rateRepository.AddAsync(rate, cancellationToken);
    }

    private async Task ValidateRateAsync(Rate rate, CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(rate.Currency))
        {
            throw new ArgumentException("Currency is required.");
        }

        if (rate.ProductRoomId.HasValue && product.Rooms.All(x => x.Id != rate.ProductRoomId.Value))
        {
            throw new InvalidOperationException("Product room not found for product.");
        }

        rate.Currency = rate.Currency.Trim().ToUpperInvariant();
        rate.ValidityPeriod = NormalizeNullable(rate.ValidityPeriod);
        rate.ValidityPeriodDescription = NormalizeNullable(rate.ValidityPeriodDescription);
        rate.RateVariation = NormalizeNullable(rate.RateVariation);
        rate.RateTypeName = NormalizeNullable(rate.RateTypeName);
        rate.RateBasis = NormalizeNullable(rate.RateBasis);
        rate.OccupancyType = NormalizeNullable(rate.OccupancyType);
        rate.MealBasis = NormalizeNullable(rate.MealBasis);
        rate.MinimumStay = NormalizeNullable(rate.MinimumStay);
    }

    private static string? NormalizeNullable(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
