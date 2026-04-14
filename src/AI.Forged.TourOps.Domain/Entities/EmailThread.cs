namespace AI.Forged.TourOps.Domain.Entities;

public class EmailThread
{
    public Guid Id { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public string? ExternalThreadId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string SupplierEmail { get; set; } = string.Empty;
    public DateTime? LastMessageAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Booking? Booking { get; set; }
    public BookingItem? BookingItem { get; set; }
    public ICollection<EmailMessage> Messages { get; set; } = new List<EmailMessage>();
    public ICollection<EmailDraft> Drafts { get; set; } = new List<EmailDraft>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
