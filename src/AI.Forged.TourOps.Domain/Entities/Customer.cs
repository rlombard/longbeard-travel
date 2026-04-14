using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Nationality { get; set; }
    public string? CountryOfResidence { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public PreferredContactMethod PreferredContactMethod { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CustomerKycProfile? KycProfile { get; set; }
    public CustomerPreferenceProfile? PreferenceProfile { get; set; }
    public ICollection<CustomerAuditLog> AuditLogs { get; set; } = new List<CustomerAuditLog>();
    public ICollection<Quote> LeadQuotes { get; set; } = new List<Quote>();
    public ICollection<Itinerary> LeadItineraries { get; set; } = new List<Itinerary>();
    public ICollection<Booking> LeadBookings { get; set; } = new List<Booking>();
    public ICollection<BookingTraveller> BookingTravellers { get; set; } = new List<BookingTraveller>();
}
