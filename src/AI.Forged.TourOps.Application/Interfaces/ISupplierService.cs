using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface ISupplierService
{
    Task<Supplier> CreateSupplierAsync(Supplier supplier, CancellationToken cancellationToken = default);
    Task<Supplier?> GetSupplierAsync(Guid supplierId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Supplier>> GetSuppliersAsync(CancellationToken cancellationToken = default);
    Task<Supplier> UpdateSupplierAsync(Guid supplierId, Supplier supplier, CancellationToken cancellationToken = default);
}
