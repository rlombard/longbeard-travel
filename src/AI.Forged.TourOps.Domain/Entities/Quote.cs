using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class Quote
{
    public Guid Id { get; set; }
    public Guid ItineraryId { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal Margin { get; set; }
    public string Currency { get; set; } = string.Empty;
    public QuoteStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Itinerary Itinerary { get; set; } = null!;
    public ICollection<QuoteLineItem> LineItems { get; set; } = new List<QuoteLineItem>();
}
