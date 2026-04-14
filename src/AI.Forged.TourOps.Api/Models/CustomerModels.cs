using System.Text.Json.Serialization;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Api.Models;

public sealed class CustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Nationality { get; set; }
    public string? CountryOfResidence { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PreferredContactMethod PreferredContactMethod { get; set; }
    public string? Notes { get; set; }
}

public sealed class CustomerKycRequest
{
    public string? PassportNumber { get; set; }
    public string? DocumentReference { get; set; }
    public DateOnly? PassportExpiry { get; set; }
    public string? IssuingCountry { get; set; }
    public string? VisaNotes { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CustomerVerificationStatus VerificationStatus { get; set; }
    public string? VerificationNotes { get; set; }
    public bool ProfileDataConsentGranted { get; set; }
    public bool KycDataConsentGranted { get; set; }
}

public sealed class CustomerPreferenceRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CustomerBudgetBand BudgetBand { get; set; }
    public string? AccommodationPreference { get; set; }
    public string? RoomPreference { get; set; }
    public List<string> DietaryRequirements { get; set; } = [];
    public List<string> ActivityPreferences { get; set; } = [];
    public List<string> AccessibilityRequirements { get; set; } = [];
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TravelPace PaceOfTravel { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TravelValueLeaning ValueLeaning { get; set; }
    public List<string> TransportPreferences { get; set; } = [];
    public List<string> SpecialOccasions { get; set; } = [];
    public List<string> DislikedExperiences { get; set; } = [];
    public List<string> PreferredDestinations { get; set; } = [];
    public List<string> AvoidedDestinations { get; set; } = [];
    public string? OperatorNotes { get; set; }
}

public sealed class BookingTravellerRequest
{
    public string? RelationshipToLeadCustomer { get; set; }
    public string? Notes { get; set; }
}

public sealed class CustomerListItemResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Nationality { get; set; }
    public string? CountryOfResidence { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PreferredContactMethod PreferredContactMethod { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CustomerResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Nationality { get; set; }
    public string? CountryOfResidence { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PreferredContactMethod PreferredContactMethod { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public CustomerKycResponse Kyc { get; set; } = new();
    public CustomerPreferenceResponse Preferences { get; set; } = new();
    public List<Guid> LeadQuoteIds { get; set; } = [];
    public List<Guid> LeadItineraryIds { get; set; } = [];
    public List<Guid> LeadBookingIds { get; set; } = [];
    public List<BookingTravellerLinkResponse> TravellerBookings { get; set; } = [];
}

public sealed class CustomerKycResponse
{
    public string? PassportNumber { get; set; }
    public string? DocumentReference { get; set; }
    public DateOnly? PassportExpiry { get; set; }
    public string? IssuingCountry { get; set; }
    public string? VisaNotes { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CustomerVerificationStatus VerificationStatus { get; set; }
    public string? VerificationNotes { get; set; }
    public bool ProfileDataConsentGranted { get; set; }
    public bool KycDataConsentGranted { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class CustomerPreferenceResponse
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CustomerBudgetBand BudgetBand { get; set; }
    public string? AccommodationPreference { get; set; }
    public string? RoomPreference { get; set; }
    public List<string> DietaryRequirements { get; set; } = [];
    public List<string> ActivityPreferences { get; set; } = [];
    public List<string> AccessibilityRequirements { get; set; } = [];
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TravelPace PaceOfTravel { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TravelValueLeaning ValueLeaning { get; set; }
    public List<string> TransportPreferences { get; set; } = [];
    public List<string> SpecialOccasions { get; set; } = [];
    public List<string> DislikedExperiences { get; set; } = [];
    public List<string> PreferredDestinations { get; set; } = [];
    public List<string> AvoidedDestinations { get; set; } = [];
    public string? OperatorNotes { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class CustomerLinkResponse
{
    public Guid CustomerId { get; set; }
    public Guid TargetId { get; set; }
    public string TargetType { get; set; } = string.Empty;
}

public sealed class BookingTravellerLinkResponse
{
    public Guid BookingId { get; set; }
    public string? RelationshipToLeadCustomer { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
