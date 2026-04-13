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
        var product = await productService.CreateProductAsync(request.ToEntity(), cancellationToken);

        return CreatedAtAction(nameof(GetProduct), new { productId = product.Id }, product.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductListItemResponse>>> GetProducts(CancellationToken cancellationToken)
    {
        var products = await productService.GetProductsAsync(cancellationToken);
        return Ok(products.Select(x => x.ToListItemResponse()).ToList());
    }

    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<ProductResponse>> GetProduct(Guid productId, CancellationToken cancellationToken)
    {
        var product = await productService.GetProductAsync(productId, cancellationToken);
        return product is null ? NotFound() : Ok(product.ToResponse());
    }

    [HttpPut("{productId:guid}")]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(Guid productId, [FromBody] ProductRequest request, CancellationToken cancellationToken)
    {
        var product = await productService.UpdateProductAsync(productId, request.ToEntity(), cancellationToken);
        return Ok(product.ToResponse());
    }
}
