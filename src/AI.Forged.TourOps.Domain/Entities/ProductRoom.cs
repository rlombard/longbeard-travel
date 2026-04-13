namespace AI.Forged.TourOps.Domain.Entities;

public class ProductRoom
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MinimumOccupancy { get; set; }
    public string? MaximumOccupancy { get; set; }
    public string? AdditionalNotes { get; set; }
    public string? RateConditions { get; set; }

    public Product Product { get; set; } = null!;
    public ICollection<Rate> Rates { get; set; } = new List<Rate>();
}
