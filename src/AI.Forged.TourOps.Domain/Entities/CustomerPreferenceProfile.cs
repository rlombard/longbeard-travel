using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class CustomerPreferenceProfile
{
    public Guid CustomerId { get; set; }
    public CustomerBudgetBand BudgetBand { get; set; }
    public string? AccommodationPreference { get; set; }
    public string? RoomPreference { get; set; }
    public string? DietaryRequirementsJson { get; set; }
    public string? ActivityPreferencesJson { get; set; }
    public string? AccessibilityRequirementsJson { get; set; }
    public TravelPace PaceOfTravel { get; set; }
    public TravelValueLeaning ValueLeaning { get; set; }
    public string? TransportPreferencesJson { get; set; }
    public string? SpecialOccasionsJson { get; set; }
    public string? DislikedExperiencesJson { get; set; }
    public string? PreferredDestinationsJson { get; set; }
    public string? AvoidedDestinationsJson { get; set; }
    public string? OperatorNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Customer Customer { get; set; } = null!;
}
