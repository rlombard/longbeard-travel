using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController(IProductService productService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] ProductRequest request, CancellationToken cancellationToken)
    {
        var product = await productService.CreateProductAsync(new Product
        {
            SupplierId = request.SupplierId,
            Name = request.Name,
            Type = request.Type,
            Metadata = request.Metadata
        }, cancellationToken);

        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetProducts(CancellationToken cancellationToken)
    {
        var products = await productService.GetProductsAsync(cancellationToken);
        return Ok(products.Select(x => x.ToResponse()).ToList());
    }
}
