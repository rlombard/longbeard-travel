using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models.Customers;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services;

public class CustomerService(
    ICustomerRepository customerRepository,
    IQuoteRepository quoteRepository,
    IItineraryRepository itineraryRepository,
    IBookingRepository bookingRepository,
    ICurrentUserContext currentUserContext) : ICustomerService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<CustomerModel> CreateCustomerAsync(CustomerCreateModel request, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeCustomer(request);
        var now = DateTime.UtcNow;
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = normalized.FirstName,
            LastName = normalized.LastName,
            Email = normalized.Email,
            Phone = normalized.Phone,
            Nationality = normalized.Nationality,
            CountryOfResidence = normalized.CountryOfResidence,
            DateOfBirth = normalized.DateOfBirth,
            PreferredContactMethod = normalized.PreferredContactMethod,
            Notes = normalized.Notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        var kyc = new CustomerKycProfile
        {
            CustomerId = customer.Id,
            VerificationStatus = CustomerVerificationStatus.NotStarted,
            CreatedAt = now,
            UpdatedAt = now
        };

        var preferences = new CustomerPreferenceProfile
        {
            CustomerId = customer.Id,
            BudgetBand = CustomerBudgetBand.Unknown,
            PaceOfTravel = TravelPace.Unknown,
            ValueLeaning = TravelValueLeaning.Unknown,
            CreatedAt = now,
            UpdatedAt = now
        };

        await customerRepository.AddAsync(customer, kyc, preferences, cancellationToken);
        await WriteAuditAsync(customer.Id, "CustomerCreated", "Customer profile created.", new
        {
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Phone
        }, cancellationToken);

        var created = await customerRepository.GetByIdAsync(customer.Id, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found after creation.");

        return MapCustomer(created);
    }

    public async Task<CustomerModel?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        return customer is null ? null : MapCustomer(customer);
    }

    public async Task<IReadOnlyList<CustomerListItemModel>> SearchCustomersAsync(CustomerSearchQueryModel query, CancellationToken cancellationToken = default)
    {
        var customers = await customerRepository.SearchAsync(query, cancellationToken);
        return customers.Select(MapListItem).ToList();
    }

    public async Task<CustomerModel> UpdateCustomerAsync(Guid customerId, CustomerUpdateModel request, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found.");

        var normalized = NormalizeCustomer(request);
        customer.FirstName = normalized.FirstName;
        customer.LastName = normalized.LastName;
        customer.Email = normalized.Email;
        customer.Phone = normalized.Phone;
        customer.Nationality = normalized.Nationality;
        customer.CountryOfResidence = normalized.CountryOfResidence;
        customer.DateOfBirth = normalized.DateOfBirth;
        customer.PreferredContactMethod = normalized.PreferredContactMethod;
        customer.Notes = normalized.Notes;
        customer.UpdatedAt = DateTime.UtcNow;

        await customerRepository.UpdateAsync(customer, cancellationToken);
        await WriteAuditAsync(customer.Id, "CustomerUpdated", "Customer profile updated.", new
        {
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Phone,
            customer.CountryOfResidence
        }, cancellationToken);

        var updated = await customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found after update.");

        return MapCustomer(updated);
    }

    public async Task<CustomerModel> UpdateKycAsync(Guid customerId, CustomerKycUpdateModel request, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found.");

        var kyc = customer.KycProfile ?? throw new InvalidOperationException("Customer KYC profile not found.");
        kyc.PassportNumber = NormalizeOptional(request.PassportNumber, 128);
        kyc.DocumentReference = NormalizeOptional(request.DocumentReference, 128);
        kyc.PassportExpiry = request.PassportExpiry;
        kyc.IssuingCountry = NormalizeOptional(request.IssuingCountry, 128);
        kyc.VisaNotes = NormalizeOptional(request.VisaNotes, 4000);
        kyc.EmergencyContactName = NormalizeOptional(request.EmergencyContactName, 200);
        kyc.EmergencyContactPhone = NormalizeOptional(request.EmergencyContactPhone, 50);
        kyc.EmergencyContactRelationship = NormalizeOptional(request.EmergencyContactRelationship, 128);
        kyc.VerificationStatus = request.VerificationStatus;
        kyc.VerificationNotes = NormalizeOptional(request.VerificationNotes, 4000);
        kyc.ProfileDataConsentGranted = request.ProfileDataConsentGranted;
        kyc.KycDataConsentGranted = request.KycDataConsentGranted;
        kyc.UpdatedAt = DateTime.UtcNow;

        if (kyc.PassportExpiry.HasValue && kyc.PassportExpiry.Value < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            kyc.VerificationStatus = CustomerVerificationStatus.Expired;
        }

        await customerRepository.UpdateKycAsync(kyc, cancellationToken);
        await WriteAuditAsync(customerId, "CustomerKycUpdated", "Customer KYC profile updated.", new
        {
            changedFields = new[]
            {
                nameof(request.PassportNumber),
                nameof(request.DocumentReference),
                nameof(request.PassportExpiry),
                nameof(request.IssuingCountry),
                nameof(request.VerificationStatus),
                nameof(request.KycDataConsentGranted)
            },
            kyc.VerificationStatus
        }, cancellationToken);

        var updated = await customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found after KYC update.");

        return MapCustomer(updated);
    }

    public async Task<CustomerModel> UpdatePreferencesAsync(Guid customerId, CustomerPreferenceUpdateModel request, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found.");

        var preference = customer.PreferenceProfile ?? throw new InvalidOperationException("Customer preference profile not found.");
        preference.BudgetBand = request.BudgetBand;
        preference.AccommodationPreference = NormalizeOptional(request.AccommodationPreference, 256);
        preference.RoomPreference = NormalizeOptional(request.RoomPreference, 256);
        preference.DietaryRequirementsJson = SerializeList(request.DietaryRequirements);
        preference.ActivityPreferencesJson = SerializeList(request.ActivityPreferences);
        preference.AccessibilityRequirementsJson = SerializeList(request.AccessibilityRequirements);
        preference.PaceOfTravel = request.PaceOfTravel;
        preference.ValueLeaning = request.ValueLeaning;
        preference.TransportPreferencesJson = SerializeList(request.TransportPreferences);
        preference.SpecialOccasionsJson = SerializeList(request.SpecialOccasions);
        preference.DislikedExperiencesJson = SerializeList(request.DislikedExperiences);
        preference.PreferredDestinationsJson = SerializeList(request.PreferredDestinations);
        preference.AvoidedDestinationsJson = SerializeList(request.AvoidedDestinations);
        preference.OperatorNotes = NormalizeOptional(request.OperatorNotes, 4000);
        preference.UpdatedAt = DateTime.UtcNow;

        await customerRepository.UpdatePreferenceProfileAsync(preference, cancellationToken);
        await WriteAuditAsync(customerId, "CustomerPreferencesUpdated", "Customer preference profile updated.", new
        {
            request.BudgetBand,
            request.PaceOfTravel,
            request.ValueLeaning
        }, cancellationToken);

        var updated = await customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found after preference update.");

        return MapCustomer(updated);
    }

    public async Task<CustomerLinkResultModel> AttachCustomerToQuoteAsync(Guid customerId, Guid quoteId, CancellationToken cancellationToken = default)
    {
        await EnsureCustomerExists(customerId, cancellationToken);
        var quote = await quoteRepository.GetByIdAsync(quoteId, cancellationToken)
            ?? throw new InvalidOperationException("Quote not found.");

        await quoteRepository.UpdateLeadCustomerAsync(quote.Id, customerId, cancellationToken);
        await WriteAuditAsync(customerId, "CustomerAttachedToQuote", "Customer attached to quote.", new { quoteId }, cancellationToken);

        return new CustomerLinkResultModel
        {
            CustomerId = customerId,
            TargetId = quoteId,
            TargetType = nameof(Quote)
        };
    }

    public async Task<CustomerLinkResultModel> AttachCustomerToItineraryAsync(Guid customerId, Guid itineraryId, CancellationToken cancellationToken = default)
    {
        await EnsureCustomerExists(customerId, cancellationToken);
        var itinerary = await itineraryRepository.GetByIdAsync(itineraryId, cancellationToken)
            ?? throw new InvalidOperationException("Itinerary not found.");

        await itineraryRepository.UpdateLeadCustomerAsync(itinerary.Id, customerId, cancellationToken);
        await WriteAuditAsync(customerId, "CustomerAttachedToItinerary", "Customer attached to itinerary.", new { itineraryId }, cancellationToken);

        return new CustomerLinkResultModel
        {
            CustomerId = customerId,
            TargetId = itineraryId,
            TargetType = nameof(Itinerary)
        };
    }

    public async Task<CustomerLinkResultModel> AttachCustomerToBookingAsync(Guid customerId, Guid bookingId, CancellationToken cancellationToken = default)
    {
        await EnsureCustomerExists(customerId, cancellationToken);
        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        await bookingRepository.UpdateLeadCustomerAsync(booking.Id, customerId, cancellationToken);
        await WriteAuditAsync(customerId, "CustomerAttachedToBooking", "Customer attached to booking.", new { bookingId }, cancellationToken);

        return new CustomerLinkResultModel
        {
            CustomerId = customerId,
            TargetId = bookingId,
            TargetType = nameof(Booking)
        };
    }

    public async Task<CustomerLinkResultModel> AddTravellerToBookingAsync(Guid customerId, Guid bookingId, BookingTravellerUpsertModel request, CancellationToken cancellationToken = default)
    {
        await EnsureCustomerExists(customerId, cancellationToken);
        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        if (booking.LeadCustomerId == customerId)
        {
            throw new InvalidOperationException("Lead customer is already attached to the booking.");
        }

        var traveller = new BookingTraveller
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            CustomerId = customerId,
            RelationshipToLeadCustomer = NormalizeOptional(request.RelationshipToLeadCustomer, 128),
            Notes = NormalizeOptional(request.Notes, 2000),
            CreatedAt = DateTime.UtcNow
        };

        await customerRepository.UpsertBookingTravellerAsync(traveller, cancellationToken);
        await WriteAuditAsync(customerId, "BookingTravellerUpserted", "Customer added or updated as a booking traveller.", new
        {
            bookingId,
            traveller.RelationshipToLeadCustomer
        }, cancellationToken);

        return new CustomerLinkResultModel
        {
            CustomerId = customerId,
            TargetId = bookingId,
            TargetType = nameof(BookingTraveller)
        };
    }

    public async Task RemoveTravellerFromBookingAsync(Guid customerId, Guid bookingId, CancellationToken cancellationToken = default)
    {
        await EnsureCustomerExists(customerId, cancellationToken);
        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        if (booking.LeadCustomerId == customerId)
        {
            throw new InvalidOperationException("Lead customer cannot be removed as a party member.");
        }

        await customerRepository.RemoveBookingTravellerAsync(bookingId, customerId, cancellationToken);
        await WriteAuditAsync(customerId, "BookingTravellerRemoved", "Customer removed from booking traveller party.", new { bookingId }, cancellationToken);
    }

    private async Task EnsureCustomerExists(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException("Customer not found.");
        }
    }

    private async Task WriteAuditAsync(Guid customerId, string actionType, string summary, object changedFields, CancellationToken cancellationToken)
    {
        await customerRepository.AddAuditLogAsync(new CustomerAuditLog
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            ActionType = actionType,
            ChangedByUserId = currentUserContext.GetRequiredUserId(),
            Summary = summary,
            ChangedFieldsJson = JsonSerializer.Serialize(changedFields, JsonOptions),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
    }

    private static CustomerCreateModel NormalizeCustomer(CustomerCreateModel request) => new()
    {
        FirstName = NormalizeRequired(request.FirstName, "First name is required.", 128),
        LastName = NormalizeRequired(request.LastName, "Last name is required.", 128),
        Email = NormalizeOptional(request.Email, 256),
        Phone = NormalizeOptional(request.Phone, 50),
        Nationality = NormalizeOptional(request.Nationality, 128),
        CountryOfResidence = NormalizeOptional(request.CountryOfResidence, 128),
        DateOfBirth = NormalizeDateOfBirth(request.DateOfBirth),
        PreferredContactMethod = request.PreferredContactMethod,
        Notes = NormalizeOptional(request.Notes, 4000)
    };

    private static CustomerUpdateModel NormalizeCustomer(CustomerUpdateModel request) => new()
    {
        FirstName = NormalizeRequired(request.FirstName, "First name is required.", 128),
        LastName = NormalizeRequired(request.LastName, "Last name is required.", 128),
        Email = NormalizeOptional(request.Email, 256),
        Phone = NormalizeOptional(request.Phone, 50),
        Nationality = NormalizeOptional(request.Nationality, 128),
        CountryOfResidence = NormalizeOptional(request.CountryOfResidence, 128),
        DateOfBirth = NormalizeDateOfBirth(request.DateOfBirth),
        PreferredContactMethod = request.PreferredContactMethod,
        Notes = NormalizeOptional(request.Notes, 4000)
    };

    private static CustomerModel MapCustomer(Customer customer) => new()
    {
        Id = customer.Id,
        FirstName = customer.FirstName,
        LastName = customer.LastName,
        FullName = $"{customer.FirstName} {customer.LastName}".Trim(),
        Email = customer.Email,
        Phone = customer.Phone,
        Nationality = customer.Nationality,
        CountryOfResidence = customer.CountryOfResidence,
        DateOfBirth = customer.DateOfBirth,
        PreferredContactMethod = customer.PreferredContactMethod,
        Notes = customer.Notes,
        CreatedAt = customer.CreatedAt,
        UpdatedAt = customer.UpdatedAt,
        Kyc = customer.KycProfile is null
            ? new CustomerKycModel()
            : new CustomerKycModel
            {
                PassportNumber = customer.KycProfile.PassportNumber,
                DocumentReference = customer.KycProfile.DocumentReference,
                PassportExpiry = customer.KycProfile.PassportExpiry,
                IssuingCountry = customer.KycProfile.IssuingCountry,
                VisaNotes = customer.KycProfile.VisaNotes,
                EmergencyContactName = customer.KycProfile.EmergencyContactName,
                EmergencyContactPhone = customer.KycProfile.EmergencyContactPhone,
                EmergencyContactRelationship = customer.KycProfile.EmergencyContactRelationship,
                VerificationStatus = customer.KycProfile.VerificationStatus,
                VerificationNotes = customer.KycProfile.VerificationNotes,
                ProfileDataConsentGranted = customer.KycProfile.ProfileDataConsentGranted,
                KycDataConsentGranted = customer.KycProfile.KycDataConsentGranted,
                UpdatedAt = customer.KycProfile.UpdatedAt
            },
        Preferences = customer.PreferenceProfile is null
            ? new CustomerPreferenceModel()
            : new CustomerPreferenceModel
            {
                BudgetBand = customer.PreferenceProfile.BudgetBand,
                AccommodationPreference = customer.PreferenceProfile.AccommodationPreference,
                RoomPreference = customer.PreferenceProfile.RoomPreference,
                DietaryRequirements = DeserializeList(customer.PreferenceProfile.DietaryRequirementsJson),
                ActivityPreferences = DeserializeList(customer.PreferenceProfile.ActivityPreferencesJson),
                AccessibilityRequirements = DeserializeList(customer.PreferenceProfile.AccessibilityRequirementsJson),
                PaceOfTravel = customer.PreferenceProfile.PaceOfTravel,
                ValueLeaning = customer.PreferenceProfile.ValueLeaning,
                TransportPreferences = DeserializeList(customer.PreferenceProfile.TransportPreferencesJson),
                SpecialOccasions = DeserializeList(customer.PreferenceProfile.SpecialOccasionsJson),
                DislikedExperiences = DeserializeList(customer.PreferenceProfile.DislikedExperiencesJson),
                PreferredDestinations = DeserializeList(customer.PreferenceProfile.PreferredDestinationsJson),
                AvoidedDestinations = DeserializeList(customer.PreferenceProfile.AvoidedDestinationsJson),
                OperatorNotes = customer.PreferenceProfile.OperatorNotes,
                UpdatedAt = customer.PreferenceProfile.UpdatedAt
            },
        LeadQuoteIds = customer.LeadQuotes.Select(x => x.Id).OrderBy(x => x).ToList(),
        LeadItineraryIds = customer.LeadItineraries.Select(x => x.Id).OrderBy(x => x).ToList(),
        LeadBookingIds = customer.LeadBookings.Select(x => x.Id).OrderBy(x => x).ToList(),
        TravellerBookings = customer.BookingTravellers
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new BookingTravellerModel
            {
                BookingId = x.BookingId,
                RelationshipToLeadCustomer = x.RelationshipToLeadCustomer,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .ToList()
    };

    private static CustomerListItemModel MapListItem(Customer customer) => new()
    {
        Id = customer.Id,
        FullName = $"{customer.FirstName} {customer.LastName}".Trim(),
        Email = customer.Email,
        Phone = customer.Phone,
        Nationality = customer.Nationality,
        CountryOfResidence = customer.CountryOfResidence,
        PreferredContactMethod = customer.PreferredContactMethod,
        UpdatedAt = customer.UpdatedAt
    };

    private static string SerializeList(IEnumerable<string> values)
    {
        var normalized = values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return JsonSerializer.Serialize(normalized, JsonOptions);
    }

    private static IReadOnlyList<string> DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
    }

    private static string NormalizeRequired(string? value, string message, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(message);
        }

        if (normalized.Length > maxLength)
        {
            throw new InvalidOperationException($"Value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized is { Length: > 0 } && normalized.Length > maxLength)
        {
            throw new InvalidOperationException($"Value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static DateOnly? NormalizeDateOfBirth(DateOnly? value)
    {
        if (value.HasValue && value.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new InvalidOperationException("Date of birth cannot be in the future.");
        }

        return value;
    }
}
