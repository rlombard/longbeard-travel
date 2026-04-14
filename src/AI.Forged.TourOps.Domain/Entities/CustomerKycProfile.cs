using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class CustomerKycProfile
{
    public Guid CustomerId { get; set; }
    public string? PassportNumber { get; set; }
    public string? DocumentReference { get; set; }
    public DateOnly? PassportExpiry { get; set; }
    public string? IssuingCountry { get; set; }
    public string? VisaNotes { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    public CustomerVerificationStatus VerificationStatus { get; set; }
    public string? VerificationNotes { get; set; }
    public bool ProfileDataConsentGranted { get; set; }
    public bool KycDataConsentGranted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Customer Customer { get; set; } = null!;
}
