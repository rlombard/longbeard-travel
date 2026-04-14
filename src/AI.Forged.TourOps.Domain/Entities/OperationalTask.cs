namespace AI.Forged.TourOps.Domain.Entities;

public class OperationalTask
{
    public Guid Id { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AI.Forged.TourOps.Domain.Enums.TaskStatus Status { get; set; }
    public string AssignedToUserId { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Booking? Booking { get; set; }
    public BookingItem? BookingItem { get; set; }
    public ICollection<OperationalTaskSuggestion> AcceptedSuggestions { get; set; } = new List<OperationalTaskSuggestion>();
}
