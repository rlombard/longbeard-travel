namespace AI.Forged.TourOps.Domain.Entities;

public class ItineraryItem
{
    public Guid Id { get; set; }
    public Guid ItineraryId { get; set; }
    public int DayNumber { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }

    public Itinerary Itinerary { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
