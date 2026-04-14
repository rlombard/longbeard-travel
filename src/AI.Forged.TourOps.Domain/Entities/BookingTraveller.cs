namespace AI.Forged.TourOps.Domain.Entities;

public class BookingTraveller
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public string? RelationshipToLeadCustomer { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public Booking Booking { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
}
