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

    public async Task<IReadOnlyList<Rate>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default) =>
        await dbContext.Rates.AsNoTracking().Where(x => x.ProductId == productId).OrderBy(x => x.SeasonStart).ToListAsync(cancellationToken);

    public async Task<Rate?> GetApplicableRateAsync(Guid productId, DateOnly date, string currency, CancellationToken cancellationToken = default) =>
        await dbContext.Rates
            .AsNoTracking()
            .Where(x => x.ProductId == productId && x.Currency == currency && x.SeasonStart <= date && x.SeasonEnd >= date)
            .OrderBy(x => x.SeasonStart)
            .FirstOrDefaultAsync(cancellationToken);
}
