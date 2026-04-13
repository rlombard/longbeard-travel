using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class SupplierRepository(AppDbContext dbContext) : ISupplierRepository
{
    public async Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        dbContext.Suppliers.Add(supplier);
        await dbContext.SaveChangesAsync(cancellationToken);
        return supplier;
    }

    public async Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Suppliers.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Suppliers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Supplier?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToUpperInvariant();

        return await dbContext.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name.ToUpper() == normalizedName, cancellationToken);
    }

    public async Task<Supplier> UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        dbContext.Suppliers.Update(supplier);
        await dbContext.SaveChangesAsync(cancellationToken);
        return supplier;
    }
}
