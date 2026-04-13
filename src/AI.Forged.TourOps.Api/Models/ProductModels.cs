using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Api.Models;

public sealed class ProductRequest
{
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductType Type { get; set; }
    public string Metadata { get; set; } = "{}";
}

public sealed class ProductResponse
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductType Type { get; set; }
    public string Metadata { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}
