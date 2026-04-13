namespace AI.Forged.TourOps.Domain.Entities;

public class QuoteLineItem
{
    public Guid Id { get; set; }
    public Guid QuoteId { get; set; }
    public Guid ProductId { get; set; }
    public decimal BaseCost { get; set; }
    public decimal AdjustedCost { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal MarkupPercentage { get; set; }
    public string Currency { get; set; } = string.Empty;

    public Quote Quote { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
