using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Services;

public class SupplierService(ISupplierRepository supplierRepository) : ISupplierService
{
    public async Task<Supplier> CreateSupplierAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(supplier.Name))
        {
            throw new ArgumentException("Supplier name is required.");
        }

        supplier.Id = Guid.NewGuid();
        supplier.CreatedAt = DateTime.UtcNow;
        return await supplierRepository.AddAsync(supplier, cancellationToken);
    }

    public Task<IReadOnlyList<Supplier>> GetSuppliersAsync(CancellationToken cancellationToken = default) =>
        supplierRepository.GetAllAsync(cancellationToken);
}
