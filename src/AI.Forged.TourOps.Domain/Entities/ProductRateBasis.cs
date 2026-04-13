namespace AI.Forged.TourOps.Domain.Entities;

public class ProductRateBasis
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}
