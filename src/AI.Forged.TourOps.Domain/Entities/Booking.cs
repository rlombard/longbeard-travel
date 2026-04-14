using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid QuoteId { get; set; }
    public Guid? LeadCustomerId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Quote Quote { get; set; } = null!;
    public Customer? LeadCustomer { get; set; }
    public ICollection<BookingItem> Items { get; set; } = new List<BookingItem>();
    public ICollection<BookingTraveller> Travellers { get; set; } = new List<BookingTraveller>();
    public ICollection<OperationalTask> Tasks { get; set; } = new List<OperationalTask>();
    public ICollection<OperationalTaskSuggestion> SuggestedTasks { get; set; } = new List<OperationalTaskSuggestion>();
    public ICollection<EmailThread> EmailThreads { get; set; } = new List<EmailThread>();
    public ICollection<EmailDraft> EmailDrafts { get; set; } = new List<EmailDraft>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
