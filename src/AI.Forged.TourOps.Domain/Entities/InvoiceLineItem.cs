namespace AI.Forged.TourOps.Domain.Entities;

public class InvoiceLineItem
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string? ExternalLineReference { get; set; }
    public Guid? BookingItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly? ServiceDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public Invoice Invoice { get; set; } = null!;
    public BookingItem? BookingItem { get; set; }
}
