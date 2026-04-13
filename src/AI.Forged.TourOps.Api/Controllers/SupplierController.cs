using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SupplierController(ISupplierService supplierService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SupplierResponse>> CreateSupplier([FromBody] SupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = await supplierService.CreateSupplierAsync(new Supplier
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone
        }, cancellationToken);

        return CreatedAtAction(nameof(GetSuppliers), new { id = supplier.Id }, supplier.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SupplierResponse>>> GetSuppliers(CancellationToken cancellationToken)
    {
        var suppliers = await supplierService.GetSuppliersAsync(cancellationToken);
        return Ok(suppliers.Select(x => x.ToResponse()).ToList());
    }
}
