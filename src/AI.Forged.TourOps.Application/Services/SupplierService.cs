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

        supplier.Name = supplier.Name.Trim();
        supplier.Id = Guid.NewGuid();
        supplier.CreatedAt = DateTime.UtcNow;
        return await supplierRepository.AddAsync(supplier, cancellationToken);
    }

    public Task<Supplier?> GetSupplierAsync(Guid supplierId, CancellationToken cancellationToken = default) =>
        supplierRepository.GetByIdAsync(supplierId, cancellationToken);

    public Task<IReadOnlyList<Supplier>> GetSuppliersAsync(CancellationToken cancellationToken = default) =>
        supplierRepository.GetAllAsync(cancellationToken);

    public async Task<Supplier> UpdateSupplierAsync(Guid supplierId, Supplier supplier, CancellationToken cancellationToken = default)
    {
        var existingSupplier = await supplierRepository.GetByIdAsync(supplierId, cancellationToken);
        if (existingSupplier is null)
        {
            throw new InvalidOperationException("Supplier not found.");
        }

        if (string.IsNullOrWhiteSpace(supplier.Name))
        {
            throw new ArgumentException("Supplier name is required.");
        }

        existingSupplier.Name = supplier.Name.Trim();
        existingSupplier.Email = string.IsNullOrWhiteSpace(supplier.Email) ? null : supplier.Email.Trim();
        existingSupplier.Phone = string.IsNullOrWhiteSpace(supplier.Phone) ? null : supplier.Phone.Trim();

        return await supplierRepository.UpdateAsync(existingSupplier, cancellationToken);
    }
}
