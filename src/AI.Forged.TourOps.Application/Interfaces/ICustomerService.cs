using AI.Forged.TourOps.Application.Models.Customers;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerModel> CreateCustomerAsync(CustomerCreateModel request, CancellationToken cancellationToken = default);
    Task<CustomerModel?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerListItemModel>> SearchCustomersAsync(CustomerSearchQueryModel query, CancellationToken cancellationToken = default);
    Task<CustomerModel> UpdateCustomerAsync(Guid customerId, CustomerUpdateModel request, CancellationToken cancellationToken = default);
    Task<CustomerModel> UpdateKycAsync(Guid customerId, CustomerKycUpdateModel request, CancellationToken cancellationToken = default);
    Task<CustomerModel> UpdatePreferencesAsync(Guid customerId, CustomerPreferenceUpdateModel request, CancellationToken cancellationToken = default);
    Task<CustomerLinkResultModel> AttachCustomerToQuoteAsync(Guid customerId, Guid quoteId, CancellationToken cancellationToken = default);
    Task<CustomerLinkResultModel> AttachCustomerToItineraryAsync(Guid customerId, Guid itineraryId, CancellationToken cancellationToken = default);
    Task<CustomerLinkResultModel> AttachCustomerToBookingAsync(Guid customerId, Guid bookingId, CancellationToken cancellationToken = default);
    Task<CustomerLinkResultModel> AddTravellerToBookingAsync(Guid customerId, Guid bookingId, BookingTravellerUpsertModel request, CancellationToken cancellationToken = default);
    Task RemoveTravellerFromBookingAsync(Guid customerId, Guid bookingId, CancellationToken cancellationToken = default);
}
