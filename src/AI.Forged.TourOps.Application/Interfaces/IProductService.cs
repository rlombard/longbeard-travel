using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IProductService
{
    Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default);
}
