using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class BookingItem
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid ProductId { get; set; }
    public Guid SupplierId { get; set; }
    public BookingItemStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public Booking Booking { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
    public ICollection<OperationalTask> Tasks { get; set; } = new List<OperationalTask>();
    public ICollection<OperationalTaskSuggestion> SuggestedTasks { get; set; } = new List<OperationalTaskSuggestion>();
    public ICollection<EmailThread> EmailThreads { get; set; } = new List<EmailThread>();
    public ICollection<EmailDraft> EmailDrafts { get; set; } = new List<EmailDraft>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<InvoiceLineItem> InvoiceLineItems { get; set; } = new List<InvoiceLineItem>();
}
