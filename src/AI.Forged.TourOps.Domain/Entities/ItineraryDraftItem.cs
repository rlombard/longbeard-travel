namespace AI.Forged.TourOps.Domain.Entities;

public class ItineraryDraftItem
{
    public Guid Id { get; set; }
    public Guid ItineraryDraftId { get; set; }
    public int DayNumber { get; set; }
    public int Sequence { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? SupplierName { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public decimal Confidence { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsUnresolved { get; set; }
    public string? WarningFlagsJson { get; set; }
    public string? MissingDataJson { get; set; }

    public ItineraryDraft ItineraryDraft { get; set; } = null!;
    public Product? Product { get; set; }
}
