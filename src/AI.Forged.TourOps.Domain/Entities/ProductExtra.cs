namespace AI.Forged.TourOps.Domain.Entities;

public class ProductExtra
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ChargeUnit { get; set; } = string.Empty;
    public string Charge { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}
