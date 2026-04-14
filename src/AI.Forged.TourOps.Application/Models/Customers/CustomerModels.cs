using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Models.Customers;

public sealed class CustomerCreateModel
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Nationality { get; init; }
    public string? CountryOfResidence { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public PreferredContactMethod PreferredContactMethod { get; init; }
    public string? Notes { get; init; }
}

public sealed class CustomerUpdateModel
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Nationality { get; init; }
    public string? CountryOfResidence { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public PreferredContactMethod PreferredContactMethod { get; init; }
    public string? Notes { get; init; }
}

public sealed class CustomerKycUpdateModel
{
    public string? PassportNumber { get; init; }
    public string? DocumentReference { get; init; }
    public DateOnly? PassportExpiry { get; init; }
    public string? IssuingCountry { get; init; }
    public string? VisaNotes { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? EmergencyContactRelationship { get; init; }
    public CustomerVerificationStatus VerificationStatus { get; init; }
    public string? VerificationNotes { get; init; }
    public bool ProfileDataConsentGranted { get; init; }
    public bool KycDataConsentGranted { get; init; }
}

public sealed class CustomerPreferenceUpdateModel
{
    public CustomerBudgetBand BudgetBand { get; init; }
    public string? AccommodationPreference { get; init; }
    public string? RoomPreference { get; init; }
    public IReadOnlyList<string> DietaryRequirements { get; init; } = [];
    public IReadOnlyList<string> ActivityPreferences { get; init; } = [];
    public IReadOnlyList<string> AccessibilityRequirements { get; init; } = [];
    public TravelPace PaceOfTravel { get; init; }
    public TravelValueLeaning ValueLeaning { get; init; }
    public IReadOnlyList<string> TransportPreferences { get; init; } = [];
    public IReadOnlyList<string> SpecialOccasions { get; init; } = [];
    public IReadOnlyList<string> DislikedExperiences { get; init; } = [];
    public IReadOnlyList<string> PreferredDestinations { get; init; } = [];
    public IReadOnlyList<string> AvoidedDestinations { get; init; } = [];
    public string? OperatorNotes { get; init; }
}

public sealed class CustomerSearchQueryModel
{
    public string? SearchTerm { get; init; }
    public string? CountryOfResidence { get; init; }
    public string? Nationality { get; init; }
}

public sealed class BookingTravellerUpsertModel
{
    public string? RelationshipToLeadCustomer { get; init; }
    public string? Notes { get; init; }
}

public sealed class CustomerListItemModel
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Nationality { get; init; }
    public string? CountryOfResidence { get; init; }
    public PreferredContactMethod PreferredContactMethod { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class CustomerModel
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Nationality { get; init; }
    public string? CountryOfResidence { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public PreferredContactMethod PreferredContactMethod { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public CustomerKycModel Kyc { get; init; } = new();
    public CustomerPreferenceModel Preferences { get; init; } = new();
    public IReadOnlyList<Guid> LeadQuoteIds { get; init; } = [];
    public IReadOnlyList<Guid> LeadItineraryIds { get; init; } = [];
    public IReadOnlyList<Guid> LeadBookingIds { get; init; } = [];
    public IReadOnlyList<BookingTravellerModel> TravellerBookings { get; init; } = [];
}

public sealed class CustomerKycModel
{
    public string? PassportNumber { get; init; }
    public string? DocumentReference { get; init; }
    public DateOnly? PassportExpiry { get; init; }
    public string? IssuingCountry { get; init; }
    public string? VisaNotes { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? EmergencyContactRelationship { get; init; }
    public CustomerVerificationStatus VerificationStatus { get; init; }
    public string? VerificationNotes { get; init; }
    public bool ProfileDataConsentGranted { get; init; }
    public bool KycDataConsentGranted { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class CustomerPreferenceModel
{
    public CustomerBudgetBand BudgetBand { get; init; }
    public string? AccommodationPreference { get; init; }
    public string? RoomPreference { get; init; }
    public IReadOnlyList<string> DietaryRequirements { get; init; } = [];
    public IReadOnlyList<string> ActivityPreferences { get; init; } = [];
    public IReadOnlyList<string> AccessibilityRequirements { get; init; } = [];
    public TravelPace PaceOfTravel { get; init; }
    public TravelValueLeaning ValueLeaning { get; init; }
    public IReadOnlyList<string> TransportPreferences { get; init; } = [];
    public IReadOnlyList<string> SpecialOccasions { get; init; } = [];
    public IReadOnlyList<string> DislikedExperiences { get; init; } = [];
    public IReadOnlyList<string> PreferredDestinations { get; init; } = [];
    public IReadOnlyList<string> AvoidedDestinations { get; init; } = [];
    public string? OperatorNotes { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class BookingTravellerModel
{
    public Guid BookingId { get; init; }
    public string? RelationshipToLeadCustomer { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class CustomerLinkResultModel
{
    public Guid CustomerId { get; init; }
    public Guid TargetId { get; init; }
    public string TargetType { get; init; } = string.Empty;
}
