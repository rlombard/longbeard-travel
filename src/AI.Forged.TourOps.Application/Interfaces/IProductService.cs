using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IProductService
{
    Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default);
    Task<Product> UpdateProductAsync(Guid productId, Product product, CancellationToken cancellationToken = default);
}
