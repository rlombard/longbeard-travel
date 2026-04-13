using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductType Type { get; set; }
    public string Metadata { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public ICollection<Rate> Rates { get; set; } = new List<Rate>();
}
