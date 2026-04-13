namespace AI.Forged.TourOps.Domain.Entities;

public class ProductValidityPeriod
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Value { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}
