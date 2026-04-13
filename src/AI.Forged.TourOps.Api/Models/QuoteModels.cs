using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Api.Models;

public sealed class GenerateQuoteRequest
{
    public Guid ItineraryId { get; set; }
    public int Pax { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Markup { get; set; }
}

public sealed class QuoteResponse
{
    public Guid Id { get; set; }
    public Guid ItineraryId { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal Margin { get; set; }
    public string Currency { get; set; } = string.Empty;
    public QuoteStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<QuoteLineItemResponse> LineItems { get; set; } = [];
}

public sealed class QuoteLineItemResponse
{
    public Guid ProductId { get; set; }
    public decimal BaseCost { get; set; }
    public decimal AdjustedCost { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal MarkupPercentage { get; set; }
    public string Currency { get; set; } = string.Empty;
}
