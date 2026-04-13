using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Services;

public class ProductService(IProductRepository productRepository, ISupplierRepository supplierRepository) : IProductService
{
    public async Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new ArgumentException("Product name is required.");
        }

        var suppliers = await supplierRepository.GetAllAsync(cancellationToken);
        if (suppliers.All(s => s.Id != product.SupplierId))
        {
            throw new InvalidOperationException("Supplier not found.");
        }

        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;
        return await productRepository.AddAsync(product, cancellationToken);
    }

    public Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default) =>
        productRepository.GetAllAsync(cancellationToken);
}
