using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Supplier> Suppliers => Set<Supplier>();
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
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteLineItem> QuoteLineItems => Set<QuoteLineItem>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<OperationalTask> Tasks => Set<OperationalTask>();
    public DbSet<OperationalTaskSuggestion> TaskSuggestions => Set<OperationalTaskSuggestion>();
    public DbSet<EmailThread> EmailThreads => Set<EmailThread>();
    public DbSet<EmailMessage> EmailMessages => Set<EmailMessage>();
    public DbSet<EmailDraft> EmailDrafts => Set<EmailDraft>();
    public DbSet<LlmAuditLog> LlmAuditLogs => Set<LlmAuditLog>();
    public DbSet<HumanApprovalRequest> HumanApprovalRequests => Set<HumanApprovalRequest>();

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
        modelBuilder.HasPostgresEnum<HumanApprovalStatus>();

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.Property(x => x.CreatedAt).IsRequired();
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
    }
}
