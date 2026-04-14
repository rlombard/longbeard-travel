using AI.Forged.TourOps.Application.Models.Customers;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer> AddAsync(Customer customer, CustomerKycProfile kycProfile, CustomerPreferenceProfile preferenceProfile, CancellationToken cancellationToken = default);
    Task AddAuditLogAsync(CustomerAuditLog auditLog, CancellationToken cancellationToken = default);
    Task<BookingTraveller?> GetBookingTravellerAsync(Guid bookingId, Guid customerId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> SearchAsync(CustomerSearchQueryModel query, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task UpdateKycAsync(CustomerKycProfile kycProfile, CancellationToken cancellationToken = default);
    Task UpdatePreferenceProfileAsync(CustomerPreferenceProfile preferenceProfile, CancellationToken cancellationToken = default);
    Task UpsertBookingTravellerAsync(BookingTraveller traveller, CancellationToken cancellationToken = default);
    Task RemoveBookingTravellerAsync(Guid bookingId, Guid customerId, CancellationToken cancellationToken = default);
}
