using System.Linq.Expressions;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private const string TenantIdPropertyName = "TenantId";
    private const string TenantScopedAnnotationName = "TourOps:TenantScoped";
    private readonly ITenantExecutionContextAccessor tenantExecutionContextAccessor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantExecutionContextAccessor tenantExecutionContextAccessor) : base(options)
    {
        this.tenantExecutionContextAccessor = tenantExecutionContextAccessor;
    }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerKycProfile> CustomerKycProfiles => Set<CustomerKycProfile>();
    public DbSet<CustomerPreferenceProfile> CustomerPreferenceProfiles => Set<CustomerPreferenceProfile>();
    public DbSet<CustomerAuditLog> CustomerAuditLogs => Set<CustomerAuditLog>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductContact> ProductContacts => Set<ProductContact>();
    public DbSet<ProductExtra> ProductExtras => Set<ProductExtra>();
    public DbSet<ProductRoom> ProductRooms => Set<ProductRoom>();
    public DbSet<ProductRateType> ProductRateTypes => Set<ProductRateType>();
    public DbSet<ProductRateBasis> ProductRateBases => Set<ProductRateBasis>();
    public DbSet<ProductMealBasis> ProductMealBases => Set<ProductMealBasis>();
    public DbSet<ProductValidityPeriod> ProductValidityPeriods => Set<ProductValidityPeriod>();
    public DbSet<Rate> Rates => Set<Rate>();
    public DbSet<Itinerary> Itineraries => Set<Itinerary>();
    public DbSet<ItineraryItem> ItineraryItems => Set<ItineraryItem>();
    public DbSet<ItineraryDraft> ItineraryDrafts => Set<ItineraryDraft>();
    public DbSet<ItineraryDraftItem> ItineraryDraftItems => Set<ItineraryDraftItem>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteLineItem> QuoteLineItems => Set<QuoteLineItem>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<BookingTraveller> BookingTravellers => Set<BookingTraveller>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<InvoiceAttachment> InvoiceAttachments => Set<InvoiceAttachment>();
    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();
    public DbSet<OperationalTask> Tasks => Set<OperationalTask>();
    public DbSet<OperationalTaskSuggestion> TaskSuggestions => Set<OperationalTaskSuggestion>();
    public DbSet<EmailThread> EmailThreads => Set<EmailThread>();
    public DbSet<EmailMessage> EmailMessages => Set<EmailMessage>();
    public DbSet<EmailDraft> EmailDrafts => Set<EmailDraft>();
    public DbSet<EmailProviderConnection> EmailProviderConnections => Set<EmailProviderConnection>();
    public DbSet<EmailProviderMessageLink> EmailProviderMessageLinks => Set<EmailProviderMessageLink>();
    public DbSet<LlmAuditLog> LlmAuditLogs => Set<LlmAuditLog>();
    public DbSet<HumanApprovalRequest> HumanApprovalRequests => Set<HumanApprovalRequest>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<LicensePlan> LicensePlans => Set<LicensePlan>();
    public DbSet<TenantLicense> TenantLicenses => Set<TenantLicense>();
    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();
    public DbSet<MonetizationTransaction> MonetizationTransactions => Set<MonetizationTransaction>();
    public DbSet<TenantOnboardingState> TenantOnboardingStates => Set<TenantOnboardingState>();
    public DbSet<TenantConfigEntry> TenantConfigEntries => Set<TenantConfigEntry>();
    public DbSet<TenantIdentityMapping> TenantIdentityMappings => Set<TenantIdentityMapping>();
    public DbSet<TenantUserMembership> TenantUserMemberships => Set<TenantUserMembership>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<SignupSession> SignupSessions => Set<SignupSession>();
    public DbSet<SignupEmailVerification> SignupEmailVerifications => Set<SignupEmailVerification>();
    public DbSet<SignupBillingIntent> SignupBillingIntents => Set<SignupBillingIntent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<ProductType>();
        modelBuilder.HasPostgresEnum<PricingModel>();
        modelBuilder.HasPostgresEnum<QuoteStatus>();
        modelBuilder.HasPostgresEnum<BookingStatus>();
        modelBuilder.HasPostgresEnum<BookingItemStatus>();
        modelBuilder.HasPostgresEnum<AI.Forged.TourOps.Domain.Enums.TaskStatus>();
        modelBuilder.HasPostgresEnum<TaskSuggestionState>();
        modelBuilder.HasPostgresEnum<EmailDirection>();
        modelBuilder.HasPostgresEnum<EmailDraftStatus>();
        modelBuilder.HasPostgresEnum<EmailDraftGeneratedBy>();
        modelBuilder.HasPostgresEnum<EmailClassificationType>();
        modelBuilder.HasPostgresEnum<EmailIntegrationProviderType>();
        modelBuilder.HasPostgresEnum<EmailIntegrationAuthMethod>();
        modelBuilder.HasPostgresEnum<EmailIntegrationStatus>();
        modelBuilder.HasPostgresEnum<HumanApprovalStatus>();
        modelBuilder.HasPostgresEnum<ItineraryDraftStatus>();
        modelBuilder.HasPostgresEnum<InvoiceStatus>();
        modelBuilder.HasPostgresEnum<PreferredContactMethod>();
        modelBuilder.HasPostgresEnum<CustomerVerificationStatus>();
        modelBuilder.HasPostgresEnum<CustomerBudgetBand>();
        modelBuilder.HasPostgresEnum<TravelPace>();
        modelBuilder.HasPostgresEnum<TravelValueLeaning>();
        modelBuilder.HasPostgresEnum<DeploymentMode>();
        modelBuilder.HasPostgresEnum<TenantStatus>();
        modelBuilder.HasPostgresEnum<LicenseStatus>();
        modelBuilder.HasPostgresEnum<BillingMode>();
        modelBuilder.HasPostgresEnum<IdentityIsolationMode>();
        modelBuilder.HasPostgresEnum<IdentityProvisioningStatus>();
        modelBuilder.HasPostgresEnum<OnboardingStatus>();
        modelBuilder.HasPostgresEnum<TenantUserRole>();
        modelBuilder.HasPostgresEnum<TenantUserStatus>();
        modelBuilder.HasPostgresEnum<MonetizationTransactionType>();
        modelBuilder.HasPostgresEnum<MonetizationTransactionStatus>();
        modelBuilder.HasPostgresEnum<LicenseSignupKind>();
        modelBuilder.HasPostgresEnum<SignupSessionStatus>();
        modelBuilder.HasPostgresEnum<SignupBillingStatus>();

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.Property(x => x.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.Property(x => x.Nationality).HasMaxLength(128);
            entity.Property(x => x.CountryOfResidence).HasMaxLength(128);
            entity.Property(x => x.PreferredContactMethod).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.Email);
            entity.HasIndex(x => x.Phone);
            entity.HasIndex(x => new { x.LastName, x.FirstName });
            entity.HasIndex(x => x.CountryOfResidence);
        });

        modelBuilder.Entity<CustomerKycProfile>(entity =>
        {
            entity.ToTable("CustomerKycProfiles");
            entity.HasKey(x => x.CustomerId);
            entity.Property(x => x.PassportNumber).HasMaxLength(128);
            entity.Property(x => x.DocumentReference).HasMaxLength(128);
            entity.Property(x => x.IssuingCountry).HasMaxLength(128);
            entity.Property(x => x.VisaNotes).HasMaxLength(4000);
            entity.Property(x => x.EmergencyContactName).HasMaxLength(200);
            entity.Property(x => x.EmergencyContactPhone).HasMaxLength(50);
            entity.Property(x => x.EmergencyContactRelationship).HasMaxLength(128);
            entity.Property(x => x.VerificationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.VerificationNotes).HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasOne(x => x.Customer)
                .WithOne(x => x.KycProfile)
                .HasForeignKey<CustomerKycProfile>(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerPreferenceProfile>(entity =>
        {
            entity.ToTable("CustomerPreferenceProfiles");
            entity.HasKey(x => x.CustomerId);
            entity.Property(x => x.BudgetBand).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.AccommodationPreference).HasMaxLength(256);
            entity.Property(x => x.RoomPreference).HasMaxLength(256);
            entity.Property(x => x.DietaryRequirementsJson).HasMaxLength(4000);
            entity.Property(x => x.ActivityPreferencesJson).HasMaxLength(4000);
            entity.Property(x => x.AccessibilityRequirementsJson).HasMaxLength(4000);
            entity.Property(x => x.PaceOfTravel).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.ValueLeaning).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.TransportPreferencesJson).HasMaxLength(4000);
            entity.Property(x => x.SpecialOccasionsJson).HasMaxLength(4000);
            entity.Property(x => x.DislikedExperiencesJson).HasMaxLength(4000);
            entity.Property(x => x.PreferredDestinationsJson).HasMaxLength(4000);
            entity.Property(x => x.AvoidedDestinationsJson).HasMaxLength(4000);
            entity.Property(x => x.OperatorNotes).HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasOne(x => x.Customer)
                .WithOne(x => x.PreferenceProfile)
                .HasForeignKey<CustomerPreferenceProfile>(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerAuditLog>(entity =>
        {
            entity.ToTable("CustomerAuditLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ActionType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ChangedByUserId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(2000);
            entity.Property(x => x.ChangedFieldsJson).HasMaxLength(8000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.CustomerId);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.AuditLogs)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.ContractValidityPeriod).HasMaxLength(128);
            entity.Property(x => x.Commission).HasMaxLength(256);
            entity.Property(x => x.PhysicalStreetAddress).HasMaxLength(256);
            entity.Property(x => x.PhysicalSuburb).HasMaxLength(128);
            entity.Property(x => x.PhysicalTownOrCity).HasMaxLength(128);
            entity.Property(x => x.PhysicalStateOrProvince).HasMaxLength(128);
            entity.Property(x => x.PhysicalCountry).HasMaxLength(128);
            entity.Property(x => x.PhysicalPostCode).HasMaxLength(32);
            entity.Property(x => x.MailingStreetAddress).HasMaxLength(256);
            entity.Property(x => x.MailingSuburb).HasMaxLength(128);
            entity.Property(x => x.MailingTownOrCity).HasMaxLength(128);
            entity.Property(x => x.MailingStateOrProvince).HasMaxLength(128);
            entity.Property(x => x.MailingCountry).HasMaxLength(128);
            entity.Property(x => x.MailingPostCode).HasMaxLength(32);
            entity.Property(x => x.CheckInTime).HasMaxLength(64);
            entity.Property(x => x.CheckOutTime).HasMaxLength(64);
            entity.Property(x => x.BlockOutDates).HasMaxLength(512);
            entity.Property(x => x.TourismLevyAmount).HasMaxLength(64);
            entity.Property(x => x.TourismLevyCurrency).HasMaxLength(32);
            entity.Property(x => x.TourismLevyUnit).HasMaxLength(128);
            entity.Property(x => x.TourismLevyAgeApplicability).HasMaxLength(128);
            entity.Property(x => x.TourismLevyEffectiveDates).HasMaxLength(256);
            entity.Property(x => x.TourismLevyConditions).HasMaxLength(2000);
            entity.Property(x => x.TourismLevyRawText).HasMaxLength(4000);
            entity.Property(x => x.RoomPolicies).HasMaxLength(4000);
            entity.Property(x => x.RatePolicies).HasMaxLength(4000);
            entity.Property(x => x.ChildPolicies).HasMaxLength(4000);
            entity.Property(x => x.CancellationPolicies).HasMaxLength(4000);
            entity.Property(x => x.Inclusions).HasMaxLength(4000);
            entity.Property(x => x.Exclusions).HasMaxLength(4000);
            entity.Property(x => x.Specials).HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasOne(x => x.Supplier)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductContact>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ContactType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ContactName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ContactEmail).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ContactPhoneNumber).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.Product)
                .WithMany(x => x.Contacts)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductExtra>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
            entity.Property(x => x.ChargeUnit).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Charge).HasMaxLength(128).IsRequired();
            entity.HasOne(x => x.Product)
                .WithMany(x => x.Extras)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductRoom>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.MinimumOccupancy).HasMaxLength(128);
            entity.Property(x => x.MaximumOccupancy).HasMaxLength(128);
            entity.Property(x => x.AdditionalNotes).HasMaxLength(4000);
            entity.Property(x => x.RateConditions).HasMaxLength(4000);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.Rooms)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductRateType>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.HasOne(x => x.Product)
                .WithMany(x => x.RateTypes)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductRateBasis>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.HasOne(x => x.Product)
                .WithMany(x => x.RateBases)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductMealBasis>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.HasOne(x => x.Product)
                .WithMany(x => x.MealBases)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductValidityPeriod>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Value).HasMaxLength(256).IsRequired();
            entity.HasOne(x => x.Product)
                .WithMany(x => x.ValidityPeriods)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Rate>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PricingModel).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.BaseCost).HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.ChildDiscount).HasPrecision(8, 4);
            entity.Property(x => x.SingleSupplement).HasPrecision(18, 2);
            entity.Property(x => x.ValidityPeriod).HasMaxLength(128);
            entity.Property(x => x.ValidityPeriodDescription).HasMaxLength(256);
            entity.Property(x => x.RateVariation).HasMaxLength(128);
            entity.Property(x => x.RateTypeName).HasMaxLength(128);
            entity.Property(x => x.RateBasis).HasMaxLength(128);
            entity.Property(x => x.OccupancyType).HasMaxLength(128);
            entity.Property(x => x.MealBasis).HasMaxLength(128);
            entity.Property(x => x.MinimumStay).HasMaxLength(256);
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasOne(x => x.Product)
                .WithMany(x => x.Rates)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProductRoom)
                .WithMany(x => x.Rates)
                .HasForeignKey(x => x.ProductRoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Itinerary>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasOne(x => x.LeadCustomer)
                .WithMany(x => x.LeadItineraries)
                .HasForeignKey(x => x.LeadCustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ItineraryItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasOne(x => x.Itinerary)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ItineraryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ItineraryDraft>(entity =>
        {
            entity.ToTable("ItineraryDrafts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequestedByUserId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.InputJson).HasMaxLength(8000).IsRequired();
            entity.Property(x => x.CustomerBrief).HasMaxLength(4000);
            entity.Property(x => x.AssumptionsJson).HasMaxLength(8000);
            entity.Property(x => x.CaveatsJson).HasMaxLength(8000);
            entity.Property(x => x.DataGapsJson).HasMaxLength(8000);
            entity.Property(x => x.LlmProvider).HasMaxLength(128);
            entity.Property(x => x.LlmModel).HasMaxLength(128);
            entity.Property(x => x.AuditMetadataJson).HasMaxLength(8000);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.ApprovedByUserId).HasMaxLength(256);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.RequestedByUserId);
            entity.HasOne(x => x.PersistedItinerary)
                .WithMany()
                .HasForeignKey(x => x.PersistedItineraryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ItineraryDraftItem>(entity =>
        {
            entity.ToTable("ItineraryDraftItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ProductName).HasMaxLength(200);
            entity.Property(x => x.SupplierName).HasMaxLength(200);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.Confidence).HasPrecision(5, 4);
            entity.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.WarningFlagsJson).HasMaxLength(4000);
            entity.Property(x => x.MissingDataJson).HasMaxLength(4000);
            entity.HasIndex(x => new { x.ItineraryDraftId, x.DayNumber, x.Sequence });
            entity.HasOne(x => x.ItineraryDraft)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ItineraryDraftId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TotalCost).HasPrecision(18, 2);
            entity.Property(x => x.TotalPrice).HasPrecision(18, 2);
            entity.Property(x => x.Margin).HasPrecision(18, 2);
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasOne(x => x.Itinerary)
                .WithMany(x => x.Quotes)
                .HasForeignKey(x => x.ItineraryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.LeadCustomer)
                .WithMany(x => x.LeadQuotes)
                .HasForeignKey(x => x.LeadCustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<QuoteLineItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BaseCost).HasPrecision(18, 2);
            entity.Property(x => x.AdjustedCost).HasPrecision(18, 2);
            entity.Property(x => x.FinalPrice).HasPrecision(18, 2);
            entity.Property(x => x.MarkupPercentage).HasPrecision(8, 4);
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.HasOne(x => x.Quote)
                .WithMany(x => x.LineItems)
                .HasForeignKey(x => x.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.QuoteId).IsUnique();
            entity.HasOne(x => x.Quote)
                .WithOne(x => x.Booking)
                .HasForeignKey<Booking>(x => x.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.LeadCustomer)
                .WithMany(x => x.LeadBookings)
                .HasForeignKey(x => x.LeadCustomerId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(x => x.Tasks)
                .WithOne(x => x.Booking)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.SuggestedTasks)
                .WithOne(x => x.Booking)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.EmailThreads)
                .WithOne(x => x.Booking)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.EmailDrafts)
                .WithOne(x => x.Booking)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingTraveller>(entity =>
        {
            entity.ToTable("BookingTravellers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RelationshipToLeadCustomer).HasMaxLength(128);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => new { x.BookingId, x.CustomerId }).IsUnique();
            entity.HasOne(x => x.Booking)
                .WithMany(x => x.Travellers)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.BookingTravellers)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasOne(x => x.Booking)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Tasks)
                .WithOne(x => x.BookingItem)
                .HasForeignKey(x => x.BookingItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.SuggestedTasks)
                .WithOne(x => x.BookingItem)
                .HasForeignKey(x => x.BookingItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.EmailThreads)
                .WithOne(x => x.BookingItem)
                .HasForeignKey(x => x.BookingItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.EmailDrafts)
                .WithOne(x => x.BookingItem)
                .HasForeignKey(x => x.BookingItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceSystem).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExternalSourceReference).HasMaxLength(256);
            entity.Property(x => x.InvoiceNumber).HasMaxLength(128);
            entity.Property(x => x.SupplierName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.SubtotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.RebateAmount).HasPrecision(18, 2);
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.Property(x => x.RawExtractionPayloadJson).HasMaxLength(16000);
            entity.Property(x => x.NormalizedSnapshotJson).HasMaxLength(16000);
            entity.Property(x => x.ExtractionConfidence).HasPrecision(5, 4);
            entity.Property(x => x.ExtractionIssuesJson).HasMaxLength(8000);
            entity.Property(x => x.UnresolvedFieldsJson).HasMaxLength(8000);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.ReceivedAt).IsRequired();
            entity.HasIndex(x => new { x.SourceSystem, x.ExternalSourceReference }).IsUnique();
            entity.HasIndex(x => x.InvoiceNumber);
            entity.HasIndex(x => x.SupplierId);
            entity.HasIndex(x => x.BookingId);
            entity.HasIndex(x => x.BookingItemId);
            entity.HasIndex(x => x.QuoteId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.DueDate);
            entity.HasOne(x => x.Supplier)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Booking)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.BookingItem)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.BookingItemId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Quote)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.QuoteId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.EmailThread)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.EmailThreadId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ReviewTask)
                .WithMany()
                .HasForeignKey(x => x.ReviewTaskId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InvoiceLineItem>(entity =>
        {
            entity.ToTable("InvoiceLineItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExternalLineReference).HasMaxLength(128);
            entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 2);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.HasIndex(x => x.InvoiceId);
            entity.HasIndex(x => x.BookingItemId);
            entity.HasOne(x => x.Invoice)
                .WithMany(x => x.LineItems)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.BookingItem)
                .WithMany(x => x.InvoiceLineItems)
                .HasForeignKey(x => x.BookingItemId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InvoiceAttachment>(entity =>
        {
            entity.ToTable("InvoiceAttachments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExternalFileReference).HasMaxLength(256);
            entity.Property(x => x.FileName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(128);
            entity.Property(x => x.SourceUrl).HasMaxLength(2000);
            entity.Property(x => x.MetadataJson).HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.InvoiceId);
            entity.HasOne(x => x.Invoice)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentRecord>(entity =>
        {
            entity.ToTable("PaymentRecords");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExternalPaymentReference).HasMaxLength(256);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.PaymentMethod).HasMaxLength(128);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.RecordedByUserId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.MetadataJson).HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.InvoiceId);
            entity.HasIndex(x => x.PaidAt);
            entity.HasOne(x => x.Invoice)
                .WithMany(x => x.PaymentRecords)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OperationalTask>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.AssignedToUserId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CreatedByUserId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.BookingId);
            entity.HasIndex(x => x.BookingItemId);
            entity.HasIndex(x => x.AssignedToUserId);
        });

        modelBuilder.Entity<OperationalTaskSuggestion>(entity =>
        {
            entity.ToTable("TaskSuggestions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.SuggestedStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.Confidence).HasPrecision(5, 4);
            entity.Property(x => x.State).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(128).IsRequired();
            entity.Property(x => x.LlmProvider).HasMaxLength(128);
            entity.Property(x => x.LlmModel).HasMaxLength(128);
            entity.Property(x => x.AuditMetadataJson).HasMaxLength(8000);
            entity.Property(x => x.ReviewedByUserId).HasMaxLength(256);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.BookingId);
            entity.HasIndex(x => x.BookingItemId);
            entity.HasIndex(x => x.State);
            entity.HasOne(x => x.AcceptedTask)
                .WithMany(x => x.AcceptedSuggestions)
                .HasForeignKey(x => x.AcceptedTaskId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EmailThread>(entity =>
        {
            entity.ToTable("EmailThreads");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExternalThreadId).HasMaxLength(256);
            entity.Property(x => x.Subject).HasMaxLength(512).IsRequired();
            entity.Property(x => x.SupplierEmail).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.BookingId);
            entity.HasIndex(x => x.BookingItemId);
            entity.HasIndex(x => x.SupplierEmail);
        });

        modelBuilder.Entity<EmailMessage>(entity =>
        {
            entity.ToTable("EmailMessages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Direction).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(512).IsRequired();
            entity.Property(x => x.BodyText).HasMaxLength(16000).IsRequired();
            entity.Property(x => x.BodyHtml).HasMaxLength(32000);
            entity.Property(x => x.Sender).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Recipients).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.AiSummary).HasMaxLength(4000);
            entity.Property(x => x.AiClassification).HasConversion<string>().HasMaxLength(64);
            entity.Property(x => x.AiConfidence).HasPrecision(5, 4);
            entity.Property(x => x.AiExtractedSignalsJson).HasMaxLength(8000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasOne(x => x.Thread)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.EmailThreadId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.EmailThreadId);
            entity.HasIndex(x => x.SentAt);
        });

        modelBuilder.Entity<EmailDraft>(entity =>
        {
            entity.ToTable("EmailDrafts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Subject).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Body).HasMaxLength(16000).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.GeneratedBy).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.ApprovedByUserId).HasMaxLength(256);
            entity.Property(x => x.LlmProvider).HasMaxLength(128);
            entity.Property(x => x.LlmModel).HasMaxLength(128);
            entity.Property(x => x.AuditMetadataJson).HasMaxLength(8000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.BookingId);
            entity.HasIndex(x => x.BookingItemId);
            entity.HasIndex(x => x.EmailThreadId);
            entity.HasIndex(x => x.Status);
            entity.HasOne(x => x.EmailThread)
                .WithMany(x => x.Drafts)
                .HasForeignKey(x => x.EmailThreadId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EmailProviderConnection>(entity =>
        {
            entity.ToTable("EmailProviderConnections");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.OwnerUserId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ConnectionName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ProviderType).HasConversion<string>().HasMaxLength(64).IsRequired();
            entity.Property(x => x.AuthMethod).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.MailboxAddress).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(256);
            entity.Property(x => x.ExternalAccountId).HasMaxLength(256);
            entity.Property(x => x.ConnectionSettingsJson).HasMaxLength(16000);
            entity.Property(x => x.EncryptedCredentialsJson).HasMaxLength(16000);
            entity.Property(x => x.OAuthState).HasMaxLength(256);
            entity.Property(x => x.OAuthReturnUrl).HasMaxLength(2000);
            entity.Property(x => x.SyncCursorJson).HasMaxLength(16000);
            entity.Property(x => x.LastError).HasMaxLength(4000);
            entity.Property(x => x.WebhookSubscriptionId).HasMaxLength(256);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.OwnerUserId);
            entity.HasIndex(x => new { x.TenantId, x.OwnerUserId, x.IsDefaultConnection });
            entity.HasIndex(x => new { x.TenantId, x.OwnerUserId, x.MailboxAddress });
            entity.HasIndex(x => new { x.Status, x.NextSyncAt });
            entity.HasIndex(x => x.OAuthState);
        });

        modelBuilder.Entity<EmailProviderMessageLink>(entity =>
        {
            entity.ToTable("EmailProviderMessageLinks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProviderMessageId).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ProviderThreadId).HasMaxLength(512);
            entity.Property(x => x.FolderName).HasMaxLength(256);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => new { x.EmailProviderConnectionId, x.ProviderMessageId }).IsUnique();
            entity.HasIndex(x => new { x.EmailProviderConnectionId, x.ProviderThreadId });
            entity.HasOne(x => x.EmailProviderConnection)
                .WithMany(x => x.MessageLinks)
                .HasForeignKey(x => x.EmailProviderConnectionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.EmailThread)
                .WithMany()
                .HasForeignKey(x => x.EmailThreadId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.EmailMessage)
                .WithMany()
                .HasForeignKey(x => x.EmailMessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LlmAuditLog>(entity =>
        {
            entity.ToTable("LlmAuditLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Category).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Operation).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Provider).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Model).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PromptSummary).HasMaxLength(4000);
            entity.Property(x => x.ResponseSummary).HasMaxLength(4000);
            entity.Property(x => x.StructuredResultJson).HasMaxLength(16000);
            entity.Property(x => x.MetadataJson).HasMaxLength(8000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.Category);
            entity.HasIndex(x => x.Provider);
            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<HumanApprovalRequest>(entity =>
        {
            entity.ToTable("HumanApprovalRequests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ActionType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RequestedByUserId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ReviewedByUserId).HasMaxLength(256);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.PayloadJson).HasMaxLength(8000);
            entity.Property(x => x.DecisionNotes).HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => new { x.EntityType, x.EntityId });
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Slug).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.LegalName).HasMaxLength(256);
            entity.Property(x => x.BillingEmail).HasMaxLength(256);
            entity.Property(x => x.DefaultCurrency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.TimeZone).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => x.IsStandaloneTenant);
        });

        modelBuilder.Entity<LicensePlan>(entity =>
        {
            entity.ToTable("LicensePlans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.SignupKind).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.MonthlyPrice).HasPrecision(18, 2);
            entity.Property(x => x.IncludedFeaturesJson).HasMaxLength(8000).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.IsPublicSignupEnabled, x.SignupSortOrder });
        });

        modelBuilder.Entity<TenantLicense>(entity =>
        {
            entity.ToTable("TenantLicenses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.BillingMode).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.FeatureOverridesJson).HasMaxLength(8000).IsRequired();
            entity.Property(x => x.BillingCustomerReference).HasMaxLength(256);
            entity.Property(x => x.SubscriptionReference).HasMaxLength(256);
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.HasIndex(x => x.TenantId).IsUnique();
            entity.HasOne(x => x.Tenant)
                .WithOne(x => x.License)
                .HasForeignKey<TenantLicense>(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.LicensePlan)
                .WithMany(x => x.TenantLicenses)
                .HasForeignKey(x => x.LicensePlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UsageRecord>(entity =>
        {
            entity.ToTable("UsageRecords");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Category).HasMaxLength(128).IsRequired();
            entity.Property(x => x.MetricKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 4);
            entity.Property(x => x.Unit).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(128);
            entity.Property(x => x.ReferenceEntityType).HasMaxLength(128);
            entity.Property(x => x.MetadataJson).HasMaxLength(8000);
            entity.HasIndex(x => new { x.TenantId, x.MetricKey, x.OccurredAt });
            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.UsageRecords)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MonetizationTransaction>(entity =>
        {
            entity.ToTable("MonetizationTransactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TransactionType).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.ExternalReference).HasMaxLength(256);
            entity.Property(x => x.MetadataJson).HasMaxLength(8000);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.MonetizationTransactions)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.UsageRecord)
                .WithMany(x => x.MonetizationTransactions)
                .HasForeignKey(x => x.UsageRecordId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TenantOnboardingState>(entity =>
        {
            entity.ToTable("TenantOnboardingStates");
            entity.HasKey(x => x.TenantId);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.CurrentStep).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CompletedStepsJson).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.OrganizationProfileJson).HasMaxLength(8000);
            entity.Property(x => x.AdminBootstrapJson).HasMaxLength(8000);
            entity.Property(x => x.EmailSetupJson).HasMaxLength(8000);
            entity.Property(x => x.BillingSetupJson).HasMaxLength(8000);
            entity.Property(x => x.LastError).HasMaxLength(2000);
            entity.HasOne(x => x.Tenant)
                .WithOne(x => x.Onboarding)
                .HasForeignKey<TenantOnboardingState>(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantConfigEntry>(entity =>
        {
            entity.ToTable("TenantConfigEntries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ConfigDomain).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ConfigKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.JsonValue).HasMaxLength(16000).IsRequired();
            entity.Property(x => x.UpdatedByUserId).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.ConfigDomain, x.ConfigKey }).IsUnique();
            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.ConfigEntries)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantIdentityMapping>(entity =>
        {
            entity.ToTable("TenantIdentityMappings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IsolationMode).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProvisioningStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.RealmName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ClientId).HasMaxLength(128);
            entity.Property(x => x.IssuerUrl).HasMaxLength(2000);
            entity.Property(x => x.MetadataJson).HasMaxLength(8000);
            entity.Property(x => x.LastError).HasMaxLength(2000);
            entity.HasIndex(x => new { x.TenantId, x.RealmName }).IsUnique();
            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.IdentityMappings)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantUserMembership>(entity =>
        {
            entity.ToTable("TenantUserMemberships");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.Status });
            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.UserMemberships)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.ToTable("AuditEvents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Result).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ActorUserId).HasMaxLength(256);
            entity.Property(x => x.ActorDisplayName).HasMaxLength(256);
            entity.Property(x => x.TargetEntityType).HasMaxLength(128);
            entity.Property(x => x.IpAddress).HasMaxLength(128);
            entity.Property(x => x.MetadataJson).HasMaxLength(8000);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
            entity.HasIndex(x => x.Action);
            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.AuditEvents)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SignupSession>(entity =>
        {
            entity.ToTable("SignupSessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AccessTokenHash).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.CurrentStep).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.NormalizedEmail).HasMaxLength(256);
            entity.Property(x => x.SelectedPlanCode).HasMaxLength(128);
            entity.Property(x => x.BillingStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.OrganizationName).HasMaxLength(200);
            entity.Property(x => x.OrganizationLegalName).HasMaxLength(256);
            entity.Property(x => x.TenantSlug).HasMaxLength(128);
            entity.Property(x => x.BillingEmail).HasMaxLength(256);
            entity.Property(x => x.DefaultCurrency).HasMaxLength(8);
            entity.Property(x => x.TimeZone).HasMaxLength(128);
            entity.Property(x => x.OrganizationProfileJson).HasMaxLength(8000);
            entity.Property(x => x.AdminEmail).HasMaxLength(256);
            entity.Property(x => x.AdminFirstName).HasMaxLength(128);
            entity.Property(x => x.AdminLastName).HasMaxLength(128);
            entity.Property(x => x.AdminUsername).HasMaxLength(128);
            entity.Property(x => x.AdminBootstrapJson).HasMaxLength(8000);
            entity.Property(x => x.ActivationResultJson).HasMaxLength(8000);
            entity.Property(x => x.LastError).HasMaxLength(2000);
            entity.HasIndex(x => x.NormalizedEmail);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.ExpiresAt);
            entity.HasOne(x => x.SelectedPlan)
                .WithMany(x => x.SignupSessions)
                .HasForeignKey(x => x.SelectedPlanId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.SignupSessions)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SignupEmailVerification>(entity =>
        {
            entity.ToTable("SignupEmailVerifications");
            entity.HasKey(x => x.SignupSessionId);
            entity.Property(x => x.TokenHash).HasMaxLength(256).IsRequired();
            entity.Property(x => x.LastSentEmail).HasMaxLength(256);
            entity.Property(x => x.LastAttemptIpAddress).HasMaxLength(128);
            entity.HasIndex(x => x.ExpiresAt);
            entity.HasOne(x => x.SignupSession)
                .WithOne(x => x.EmailVerification)
                .HasForeignKey<SignupEmailVerification>(x => x.SignupSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SignupBillingIntent>(entity =>
        {
            entity.ToTable("SignupBillingIntents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.BillingMode).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.ProviderName).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExternalReference).HasMaxLength(256);
            entity.Property(x => x.CheckoutUrl).HasMaxLength(2000);
            entity.Property(x => x.MetadataJson).HasMaxLength(8000);
            entity.HasIndex(x => x.SignupSessionId).IsUnique();
            entity.HasIndex(x => x.Status);
            entity.HasOne(x => x.SignupSession)
                .WithOne(x => x.BillingIntent)
                .HasForeignKey<SignupBillingIntent>(x => x.SignupSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.LicensePlan)
                .WithMany(x => x.SignupBillingIntents)
                .HasForeignKey(x => x.LicensePlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        ConfigureTenantScopedEntities(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyTenantScope();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantScope();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ConfigureTenantScopedEntities(ModelBuilder modelBuilder)
    {
        ConfigureTenantScopedEntity<Supplier>(modelBuilder);
        ConfigureTenantScopedEntity<Customer>(modelBuilder);
        ConfigureTenantScopedEntity<CustomerKycProfile>(modelBuilder);
        ConfigureTenantScopedEntity<CustomerPreferenceProfile>(modelBuilder);
        ConfigureTenantScopedEntity<CustomerAuditLog>(modelBuilder);
        ConfigureTenantScopedEntity<Product>(modelBuilder);
        ConfigureTenantScopedEntity<ProductContact>(modelBuilder);
        ConfigureTenantScopedEntity<ProductExtra>(modelBuilder);
        ConfigureTenantScopedEntity<ProductRoom>(modelBuilder);
        ConfigureTenantScopedEntity<ProductRateType>(modelBuilder);
        ConfigureTenantScopedEntity<ProductRateBasis>(modelBuilder);
        ConfigureTenantScopedEntity<ProductMealBasis>(modelBuilder);
        ConfigureTenantScopedEntity<ProductValidityPeriod>(modelBuilder);
        ConfigureTenantScopedEntity<Rate>(modelBuilder);
        ConfigureTenantScopedEntity<Itinerary>(modelBuilder);
        ConfigureTenantScopedEntity<ItineraryItem>(modelBuilder);
        ConfigureTenantScopedEntity<ItineraryDraft>(modelBuilder);
        ConfigureTenantScopedEntity<ItineraryDraftItem>(modelBuilder);
        ConfigureTenantScopedEntity<Quote>(modelBuilder);
        ConfigureTenantScopedEntity<QuoteLineItem>(modelBuilder);
        ConfigureTenantScopedEntity<Booking>(modelBuilder);
        ConfigureTenantScopedEntity<BookingItem>(modelBuilder);
        ConfigureTenantScopedEntity<BookingTraveller>(modelBuilder);
        ConfigureTenantScopedEntity<Invoice>(modelBuilder);
        ConfigureTenantScopedEntity<InvoiceLineItem>(modelBuilder);
        ConfigureTenantScopedEntity<InvoiceAttachment>(modelBuilder);
        ConfigureTenantScopedEntity<PaymentRecord>(modelBuilder);
        ConfigureTenantScopedEntity<OperationalTask>(modelBuilder);
        ConfigureTenantScopedEntity<OperationalTaskSuggestion>(modelBuilder);
        ConfigureTenantScopedEntity<EmailThread>(modelBuilder);
        ConfigureTenantScopedEntity<EmailMessage>(modelBuilder);
        ConfigureTenantScopedEntity<EmailDraft>(modelBuilder);
        ConfigureTenantScopedEntity<EmailProviderConnection>(modelBuilder);
        ConfigureTenantScopedEntity<EmailProviderMessageLink>(modelBuilder);
        ConfigureTenantScopedEntity<LlmAuditLog>(modelBuilder);
        ConfigureTenantScopedEntity<HumanApprovalRequest>(modelBuilder);
    }

    private void ConfigureTenantScopedEntity<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class
    {
        var entity = modelBuilder.Entity<TEntity>();
        entity.HasAnnotation(TenantScopedAnnotationName, true);
        entity.Property<Guid>(TenantIdPropertyName);
        entity.HasIndex(TenantIdPropertyName);
        entity.HasQueryFilter(BuildTenantFilter<TEntity>());
    }

    private Expression<Func<TEntity, bool>> BuildTenantFilter<TEntity>()
        where TEntity : class =>
        entity => tenantExecutionContextAccessor.IsPlatformScope
            || (tenantExecutionContextAccessor.CurrentTenantId.HasValue
                && EF.Property<Guid>(entity, TenantIdPropertyName) == tenantExecutionContextAccessor.CurrentTenantId.Value);

    private void ApplyTenantScope()
    {
        foreach (var entry in ChangeTracker.Entries().Where(x =>
                     x.Metadata.FindAnnotation(TenantScopedAnnotationName)?.Value as bool? == true
                     &&
                     x.Metadata.FindProperty(TenantIdPropertyName) is not null
                     && x.State is EntityState.Added or EntityState.Modified))
        {
            var property = entry.Property(TenantIdPropertyName);
            var currentValue = property.CurrentValue is Guid guidValue ? guidValue : Guid.Empty;

            if (entry.State == EntityState.Added)
            {
                if (currentValue != Guid.Empty)
                {
                    continue;
                }

                if (!tenantExecutionContextAccessor.CurrentTenantId.HasValue)
                {
                    throw new InvalidOperationException("Tenant context is required for writes.");
                }

                property.CurrentValue = tenantExecutionContextAccessor.CurrentTenantId.Value;
                continue;
            }

            if (tenantExecutionContextAccessor.IsPlatformScope || !tenantExecutionContextAccessor.CurrentTenantId.HasValue)
            {
                continue;
            }

            var originalValue = property.OriginalValue is Guid originalGuid ? originalGuid : currentValue;
            if (originalValue != tenantExecutionContextAccessor.CurrentTenantId.Value)
            {
                throw new InvalidOperationException("Cross-tenant write was blocked.");
            }
        }
    }
}
