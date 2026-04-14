using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models.Customers;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class CustomerRepository(AppDbContext dbContext) : ICustomerRepository
{
    public async Task<Customer> AddAsync(Customer customer, CustomerKycProfile kycProfile, CustomerPreferenceProfile preferenceProfile, CancellationToken cancellationToken = default)
    {
        dbContext.Customers.Add(customer);
        dbContext.CustomerKycProfiles.Add(kycProfile);
        dbContext.CustomerPreferenceProfiles.Add(preferenceProfile);
        await dbContext.SaveChangesAsync(cancellationToken);
        customer.KycProfile = kycProfile;
        customer.PreferenceProfile = preferenceProfile;
        return customer;
    }

    public async Task AddAuditLogAsync(CustomerAuditLog auditLog, CancellationToken cancellationToken = default)
    {
        dbContext.CustomerAuditLogs.Add(auditLog);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<BookingTraveller?> GetBookingTravellerAsync(Guid bookingId, Guid customerId, CancellationToken cancellationToken = default) =>
        await dbContext.BookingTravellers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BookingId == bookingId && x.CustomerId == customerId, cancellationToken);

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await BuildCustomerQuery()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Customer>> SearchAsync(CustomerSearchQueryModel query, CancellationToken cancellationToken = default)
    {
        var customers = BuildCustomerQuery();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var pattern = $"%{query.SearchTerm.Trim()}%";
            customers = customers.Where(x =>
                EF.Functions.ILike(x.FirstName, pattern) ||
                EF.Functions.ILike(x.LastName, pattern) ||
                (x.Email != null && EF.Functions.ILike(x.Email, pattern)) ||
                (x.Phone != null && EF.Functions.ILike(x.Phone, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(query.CountryOfResidence))
        {
            var pattern = $"%{query.CountryOfResidence.Trim()}%";
            customers = customers.Where(x => x.CountryOfResidence != null && EF.Functions.ILike(x.CountryOfResidence, pattern));
        }

        if (!string.IsNullOrWhiteSpace(query.Nationality))
        {
            var pattern = $"%{query.Nationality.Trim()}%";
            customers = customers.Where(x => x.Nationality != null && EF.Functions.ILike(x.Nationality, pattern));
        }

        return await customers
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found.");

        existing.FirstName = customer.FirstName;
        existing.LastName = customer.LastName;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;
        existing.Nationality = customer.Nationality;
        existing.CountryOfResidence = customer.CountryOfResidence;
        existing.DateOfBirth = customer.DateOfBirth;
        existing.PreferredContactMethod = customer.PreferredContactMethod;
        existing.Notes = customer.Notes;
        existing.UpdatedAt = customer.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateKycAsync(CustomerKycProfile kycProfile, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.CustomerKycProfiles.FirstOrDefaultAsync(x => x.CustomerId == kycProfile.CustomerId, cancellationToken)
            ?? throw new InvalidOperationException("Customer KYC profile not found.");

        existing.PassportNumber = kycProfile.PassportNumber;
        existing.DocumentReference = kycProfile.DocumentReference;
        existing.PassportExpiry = kycProfile.PassportExpiry;
        existing.IssuingCountry = kycProfile.IssuingCountry;
        existing.VisaNotes = kycProfile.VisaNotes;
        existing.EmergencyContactName = kycProfile.EmergencyContactName;
        existing.EmergencyContactPhone = kycProfile.EmergencyContactPhone;
        existing.EmergencyContactRelationship = kycProfile.EmergencyContactRelationship;
        existing.VerificationStatus = kycProfile.VerificationStatus;
        existing.VerificationNotes = kycProfile.VerificationNotes;
        existing.ProfileDataConsentGranted = kycProfile.ProfileDataConsentGranted;
        existing.KycDataConsentGranted = kycProfile.KycDataConsentGranted;
        existing.UpdatedAt = kycProfile.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePreferenceProfileAsync(CustomerPreferenceProfile preferenceProfile, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.CustomerPreferenceProfiles.FirstOrDefaultAsync(x => x.CustomerId == preferenceProfile.CustomerId, cancellationToken)
            ?? throw new InvalidOperationException("Customer preference profile not found.");

        existing.BudgetBand = preferenceProfile.BudgetBand;
        existing.AccommodationPreference = preferenceProfile.AccommodationPreference;
        existing.RoomPreference = preferenceProfile.RoomPreference;
        existing.DietaryRequirementsJson = preferenceProfile.DietaryRequirementsJson;
        existing.ActivityPreferencesJson = preferenceProfile.ActivityPreferencesJson;
        existing.AccessibilityRequirementsJson = preferenceProfile.AccessibilityRequirementsJson;
        existing.PaceOfTravel = preferenceProfile.PaceOfTravel;
        existing.ValueLeaning = preferenceProfile.ValueLeaning;
        existing.TransportPreferencesJson = preferenceProfile.TransportPreferencesJson;
        existing.SpecialOccasionsJson = preferenceProfile.SpecialOccasionsJson;
        existing.DislikedExperiencesJson = preferenceProfile.DislikedExperiencesJson;
        existing.PreferredDestinationsJson = preferenceProfile.PreferredDestinationsJson;
        existing.AvoidedDestinationsJson = preferenceProfile.AvoidedDestinationsJson;
        existing.OperatorNotes = preferenceProfile.OperatorNotes;
        existing.UpdatedAt = preferenceProfile.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertBookingTravellerAsync(BookingTraveller traveller, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.BookingTravellers
            .FirstOrDefaultAsync(x => x.BookingId == traveller.BookingId && x.CustomerId == traveller.CustomerId, cancellationToken);

        if (existing is null)
        {
            dbContext.BookingTravellers.Add(traveller);
        }
        else
        {
            existing.RelationshipToLeadCustomer = traveller.RelationshipToLeadCustomer;
            existing.Notes = traveller.Notes;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveBookingTravellerAsync(Guid bookingId, Guid customerId, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.BookingTravellers
            .FirstOrDefaultAsync(x => x.BookingId == bookingId && x.CustomerId == customerId, cancellationToken)
            ?? throw new InvalidOperationException("Booking traveller not found.");

        dbContext.BookingTravellers.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Customer> BuildCustomerQuery() =>
        dbContext.Customers
            .Include(x => x.KycProfile)
            .Include(x => x.PreferenceProfile)
            .Include(x => x.LeadQuotes)
            .Include(x => x.LeadItineraries)
            .Include(x => x.LeadBookings)
            .Include(x => x.BookingTravellers)
            .AsNoTracking();
}
