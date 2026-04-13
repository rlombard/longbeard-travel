using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class RateRepository(AppDbContext dbContext) : IRateRepository
{
    public async Task<Rate> AddAsync(Rate rate, CancellationToken cancellationToken = default)
    {
        dbContext.Rates.Add(rate);
        await dbContext.SaveChangesAsync(cancellationToken);
        return rate;
    }

    public async Task<Rate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Rates
            .Include(x => x.ProductRoom)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Rate>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default) =>
        await dbContext.Rates
            .Include(x => x.ProductRoom)
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.CreatedAt)
            .ThenBy(x => x.SeasonStart)
            .ToListAsync(cancellationToken);

    public async Task<Rate?> GetApplicableRateAsync(Guid productId, DateOnly date, string currency, CancellationToken cancellationToken = default) =>
        await dbContext.Rates
            .AsNoTracking()
            .Where(x => x.ProductId == productId && x.IsActive && x.Currency == currency && x.SeasonStart <= date && x.SeasonEnd >= date)
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.SeasonStart)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task MarkAsSupersededAsync(Guid rateId, DateTime supersededAtUtc, CancellationToken cancellationToken = default)
    {
        var existingRate = await dbContext.Rates.FirstOrDefaultAsync(x => x.Id == rateId, cancellationToken)
            ?? throw new InvalidOperationException("Rate not found.");

        existingRate.IsActive = false;
        existingRate.SupersededAt = supersededAtUtc;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
