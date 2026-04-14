using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models.Customers;
using AI.Forged.TourOps.Application.Models.Itineraries;
using AI.Forged.TourOps.Application.Services;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using Xunit;

namespace AI.Forged.TourOps.Application.Tests;

public class CustomerServiceTests
{
    [Fact]
    public async Task CreateCustomerAsync_CreatesDefaultProfilesAndAudit()
    {
        var fixture = new CustomerFixture();
        var service = fixture.CreateService();

        var customer = await service.CreateCustomerAsync(new CustomerCreateModel
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            PreferredContactMethod = PreferredContactMethod.Email
        });

        Assert.Equal("Jane Doe", customer.FullName);
        Assert.Equal(CustomerVerificationStatus.NotStarted, customer.Kyc.VerificationStatus);
        Assert.Equal(CustomerBudgetBand.Unknown, customer.Preferences.BudgetBand);
        Assert.Single(fixture.CustomerRepository.AuditLogs);
    }

    [Fact]
    public async Task UpdateKycAsync_WritesAuditWithoutNeedingNewCustomerEntity()
    {
        var fixture = new CustomerFixture();
        var service = fixture.CreateService();
        var customerId = await fixture.AddCustomerAsync();

        var updated = await service.UpdateKycAsync(customerId, new CustomerKycUpdateModel
        {
            PassportNumber = "P123456",
            IssuingCountry = "ZA",
            VerificationStatus = CustomerVerificationStatus.Verified,
            KycDataConsentGranted = true,
            ProfileDataConsentGranted = true
        });

        Assert.Equal("P123456", updated.Kyc.PassportNumber);
        Assert.Equal(CustomerVerificationStatus.Verified, updated.Kyc.VerificationStatus);
        Assert.Contains(fixture.CustomerRepository.AuditLogs, x => x.ActionType == "CustomerKycUpdated");
    }

    [Fact]
    public async Task AttachCustomerToQuoteAsync_UpdatesLeadCustomerLink()
    {
        var fixture = new CustomerFixture();
        var service = fixture.CreateService();
        var customerId = await fixture.AddCustomerAsync();

        var result = await service.AttachCustomerToQuoteAsync(customerId, fixture.Quote.Id);

        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(customerId, fixture.Quote.LeadCustomerId);
    }

    [Fact]
    public async Task AddTravellerToBookingAsync_RejectsLeadCustomerAsPartyMember()
    {
        var fixture = new CustomerFixture();
        var service = fixture.CreateService();
        var customerId = await fixture.AddCustomerAsync();
        fixture.Booking.LeadCustomerId = customerId;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddTravellerToBookingAsync(customerId, fixture.Booking.Id, new BookingTravellerUpsertModel()));
    }

    [Fact]
    public async Task UpdateCustomerAsync_RejectsFutureDateOfBirth()
    {
        var fixture = new CustomerFixture();
        var service = fixture.CreateService();
        var customerId = await fixture.AddCustomerAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateCustomerAsync(customerId, new CustomerUpdateModel
            {
                FirstName = "Jane",
                LastName = "Doe",
                PreferredContactMethod = PreferredContactMethod.Email,
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            }));
    }

    private sealed class CustomerFixture
    {
        public FakeCustomerRepository CustomerRepository { get; } = new();
        public FakeQuoteRepository QuoteRepository { get; } = new();
        public FakeItineraryRepository ItineraryRepository { get; } = new();
        public FakeBookingRepository BookingRepository { get; } = new();

        public Quote Quote { get; } = new()
        {
            Id = Guid.NewGuid(),
            ItineraryId = Guid.NewGuid(),
            Currency = "USD",
            Status = QuoteStatus.Generated,
            CreatedAt = DateTime.UtcNow
        };

        public Itinerary Itinerary { get; } = new()
        {
            Id = Guid.NewGuid(),
            StartDate = new DateOnly(2026, 1, 1),
            Duration = 3,
            CreatedAt = DateTime.UtcNow
        };

        public Booking Booking { get; } = new()
        {
            Id = Guid.NewGuid(),
            QuoteId = Guid.NewGuid(),
            Status = BookingStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        public CustomerFixture()
        {
            QuoteRepository.Quotes[Quote.Id] = Quote;
            ItineraryRepository.Itineraries[Itinerary.Id] = Itinerary;
            BookingRepository.Bookings[Booking.Id] = Booking;
        }

        public CustomerService CreateService() =>
            new(
                CustomerRepository,
                QuoteRepository,
                ItineraryRepository,
                BookingRepository,
                new FakeCurrentUserContext());

        public async Task<Guid> AddCustomerAsync()
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Doe",
                PreferredContactMethod = PreferredContactMethod.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                KycProfile = new CustomerKycProfile
                {
                    CustomerId = Guid.Empty,
                    VerificationStatus = CustomerVerificationStatus.NotStarted,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                PreferenceProfile = new CustomerPreferenceProfile
                {
                    CustomerId = Guid.Empty,
                    BudgetBand = CustomerBudgetBand.Unknown,
                    PaceOfTravel = TravelPace.Unknown,
                    ValueLeaning = TravelValueLeaning.Unknown,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            customer.KycProfile!.CustomerId = customer.Id;
            customer.PreferenceProfile!.CustomerId = customer.Id;
            await CustomerRepository.AddAsync(customer, customer.KycProfile, customer.PreferenceProfile, CancellationToken.None);
            return customer.Id;
        }
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public Dictionary<Guid, Customer> Customers { get; } = [];
        public List<CustomerAuditLog> AuditLogs { get; } = [];
        public List<BookingTraveller> Travellers { get; } = [];

        public Task<Customer> AddAsync(Customer customer, CustomerKycProfile kycProfile, CustomerPreferenceProfile preferenceProfile, CancellationToken cancellationToken = default)
        {
            customer.KycProfile = kycProfile;
            customer.PreferenceProfile = preferenceProfile;
            Customers[customer.Id] = customer;
            return Task.FromResult(customer);
        }

        public Task AddAuditLogAsync(CustomerAuditLog auditLog, CancellationToken cancellationToken = default)
        {
            AuditLogs.Add(auditLog);
            return Task.CompletedTask;
        }

        public Task<BookingTraveller?> GetBookingTravellerAsync(Guid bookingId, Guid customerId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Travellers.FirstOrDefault(x => x.BookingId == bookingId && x.CustomerId == customerId));

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Customers.TryGetValue(id, out var customer) ? customer : null);

        public Task<IReadOnlyList<Customer>> SearchAsync(CustomerSearchQueryModel query, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Customer>>(Customers.Values.ToList());

        public Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            Customers[customer.Id] = customer;
            return Task.CompletedTask;
        }

        public Task UpdateKycAsync(CustomerKycProfile kycProfile, CancellationToken cancellationToken = default)
        {
            Customers[kycProfile.CustomerId].KycProfile = kycProfile;
            return Task.CompletedTask;
        }

        public Task UpdatePreferenceProfileAsync(CustomerPreferenceProfile preferenceProfile, CancellationToken cancellationToken = default)
        {
            Customers[preferenceProfile.CustomerId].PreferenceProfile = preferenceProfile;
            return Task.CompletedTask;
        }

        public Task UpsertBookingTravellerAsync(BookingTraveller traveller, CancellationToken cancellationToken = default)
        {
            var existing = Travellers.FirstOrDefault(x => x.BookingId == traveller.BookingId && x.CustomerId == traveller.CustomerId);
            if (existing is null)
            {
                Travellers.Add(traveller);
            }
            else
            {
                existing.RelationshipToLeadCustomer = traveller.RelationshipToLeadCustomer;
                existing.Notes = traveller.Notes;
            }

            return Task.CompletedTask;
        }

        public Task RemoveBookingTravellerAsync(Guid bookingId, Guid customerId, CancellationToken cancellationToken = default)
        {
            var existing = Travellers.First(x => x.BookingId == bookingId && x.CustomerId == customerId);
            Travellers.Remove(existing);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeQuoteRepository : IQuoteRepository
    {
        public Dictionary<Guid, Quote> Quotes { get; } = [];

        public Task<Quote> AddAsync(Quote quote, CancellationToken cancellationToken = default) => Task.FromResult(quote);

        public Task<Quote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Quotes.TryGetValue(id, out var quote) ? quote : null);

        public Task<Quote?> GetByIdForBookingAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Quotes.TryGetValue(id, out var quote) ? quote : null);

        public Task UpdateLeadCustomerAsync(Guid quoteId, Guid? customerId, CancellationToken cancellationToken = default)
        {
            Quotes[quoteId].LeadCustomerId = customerId;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeItineraryRepository : IItineraryRepository
    {
        public Dictionary<Guid, Itinerary> Itineraries { get; } = [];

        public Task<Itinerary> AddAsync(Itinerary itinerary, IEnumerable<ItineraryItem> items, CancellationToken cancellationToken = default) => Task.FromResult(itinerary);
        public Task<ItineraryDraft> AddDraftAsync(ItineraryDraft draft, IEnumerable<ItineraryDraftItem> items, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ItineraryDraft?> GetDraftByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Itinerary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Itineraries.TryGetValue(id, out var itinerary) ? itinerary : null);
        public Task UpdateDraftAsync(ItineraryDraft draft, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task UpdateLeadCustomerAsync(Guid id, Guid? customerId, CancellationToken cancellationToken = default)
        {
            Itineraries[id].LeadCustomerId = customerId;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeBookingRepository : IBookingRepository
    {
        public Dictionary<Guid, Booking> Bookings { get; } = [];

        public Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken = default) => Task.FromResult(booking);
        public Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Booking>>(Bookings.Values.ToList());
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Bookings.TryGetValue(id, out var booking) ? booking : null);
        public Task<Booking?> GetByQuoteIdAsync(Guid quoteId, CancellationToken cancellationToken = default) => Task.FromResult(Bookings.Values.FirstOrDefault(x => x.QuoteId == quoteId));
        public Task UpdateStatusAsync(Guid id, BookingStatus status, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateLeadCustomerAsync(Guid id, Guid? customerId, CancellationToken cancellationToken = default)
        {
            Bookings[id].LeadCustomerId = customerId;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public string GetRequiredUserId() => "operator-1";
    }
}
