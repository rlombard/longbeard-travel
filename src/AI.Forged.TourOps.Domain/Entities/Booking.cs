using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid QuoteId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Quote Quote { get; set; } = null!;
    public ICollection<BookingItem> Items { get; set; } = new List<BookingItem>();
    public ICollection<OperationalTask> Tasks { get; set; } = new List<OperationalTask>();
    public ICollection<OperationalTaskSuggestion> SuggestedTasks { get; set; } = new List<OperationalTaskSuggestion>();
    public ICollection<EmailThread> EmailThreads { get; set; } = new List<EmailThread>();
    public ICollection<EmailDraft> EmailDrafts { get; set; } = new List<EmailDraft>();
}
