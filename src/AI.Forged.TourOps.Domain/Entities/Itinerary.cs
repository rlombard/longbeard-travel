namespace AI.Forged.TourOps.Domain.Entities;

public class Itinerary
{
    public Guid Id { get; set; }
    public DateOnly StartDate { get; set; }
    public int Duration { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<ItineraryItem> Items { get; set; } = new List<ItineraryItem>();
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
}
