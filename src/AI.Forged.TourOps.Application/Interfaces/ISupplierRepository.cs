using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface ISupplierRepository
{
    Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Supplier?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Supplier> UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default);
}
