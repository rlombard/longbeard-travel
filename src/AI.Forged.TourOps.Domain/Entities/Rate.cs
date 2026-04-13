using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class Rate
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public DateOnly SeasonStart { get; set; }
    public DateOnly SeasonEnd { get; set; }
    public PricingModel PricingModel { get; set; }
    public decimal BaseCost { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int? MinPax { get; set; }
    public int? MaxPax { get; set; }
    public decimal? ChildDiscount { get; set; }
    public decimal? SingleSupplement { get; set; }
    public int? Capacity { get; set; }
    public DateTime CreatedAt { get; set; }

    public Product Product { get; set; } = null!;
}
