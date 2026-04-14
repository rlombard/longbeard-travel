using AI.Forged.TourOps.Application.Models;
using AI.Forged.TourOps.Application.Models.Customers;
using AI.Forged.TourOps.Application.Models.Itineraries;
using AI.Forged.TourOps.Application.Models.Invoices;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Api.Models;

public static class ModelMappings
{
    public static IngestionPropertyBundlePayload ToPayload(this IngestionPropertyBundleRequest request) => new()
    {
        Owner = request.Owner,
        PropertyRoomMeta = request.PropertyRoomMeta,
        PropertyDetails = request.PropertyDetails.Select(x => new IngestionPropertyDetailPayload
        {
            Name = x.Name,
            Currency = x.Currency,
            ValidityPeriod = x.ValidityPeriod,
            Commission = x.Commission,
            PhysicalAddress = new IngestionAddressPayload
            {
                StreetAddress = x.PhysicalAddress.StreetAddress,
                Suburb = x.PhysicalAddress.Suburb,
                TownOrCity = x.PhysicalAddress.TownOrCity,
                StateOrProvince = x.PhysicalAddress.StateOrProvince,
                Country = x.PhysicalAddress.Country,
                PostCode = x.PhysicalAddress.PostCode
            },
            MailingAddress = new IngestionAddressPayload
            {
                StreetAddress = x.MailingAddress.StreetAddress,
                Suburb = x.MailingAddress.Suburb,
                TownOrCity = x.MailingAddress.TownOrCity,
                StateOrProvince = x.MailingAddress.StateOrProvince,
                Country = x.MailingAddress.Country,
                PostCode = x.MailingAddress.PostCode
            },
            CheckInTime = x.CheckInTime,
            CheckOutTime = x.CheckOutTime,
            BlockOutDates = x.BlockOutDates,
            TourismLevy = new IngestionTourismLevyPayload
            {
                Amount = x.TourismLevy.Amount,
                Currency = x.TourismLevy.Currency,
                Unit = x.TourismLevy.Unit,
                AgeApplicability = x.TourismLevy.AgeApplicability,
                EffectiveDates = x.TourismLevy.EffectiveDates,
                Conditions = x.TourismLevy.Conditions,
                RawText = x.TourismLevy.RawText
            },
            TourismLevyIncluded = x.TourismLevyIncluded,
            Contacts = x.Contacts.Select(c => new IngestionContactPayload
            {
                ContactType = c.ContactType,
                ContactName = c.ContactName,
                ContactEmail = c.ContactEmail,
                ContactPhoneNumber = c.ContactPhoneNumber
            }).ToList()
        }).ToList(),
        RoomPricingDetails = request.RoomPricingDetails.Select(x => new IngestionRoomPricingPayload
        {
            PropertyName = x.PropertyName,
            Rooms = x.Rooms.Select(r => new IngestionRoomPayload
            {
                Name = r.Name,
                MinimumOccupancy = r.MinimumOccupancy,
                MaximumOccupancy = r.MaximumOccupancy,
                AdditionalNotes = r.AdditionalNotes,
                RateConditions = r.RateConditions,
                RateAvailability = r.RateAvailability.Select(a => new IngestionRoomAvailabilityPayload
                {
                    ValidityPeriod = a.ValidityPeriod,
                    ValidityPeriodDescription = a.ValidityPeriodDescription,
                    Rates = a.Rates.Select(rr => new IngestionRoomRatePayload
                    {
                        RateVariation = rr.RateVariation,
                        RateTypeName = rr.RateTypeName,
                        RateValue = rr.RateValue,
                        RateBasis = rr.RateBasis,
                        OccupancyType = rr.OccupancyType,
                        MealBasis = rr.MealBasis,
                        MinimumStay = rr.MinimumStay
                    }).ToList()
                }).ToList()
            }).ToList()
        }).ToList(),
        PropertyContentDetails = request.PropertyContentDetails.Select(x => new IngestionPropertyContentPayload
        {
            PropertyName = x.PropertyName,
            Policies = new IngestionPoliciesPayload
            {
                RoomPolicies = x.Policies.RoomPolicies,
                RatePolicies = x.Policies.RatePolicies,
                ChildPolicies = x.Policies.ChildPolicies,
                CancellationPolicies = x.Policies.CancellationPolicies,
                Inclusions = x.Policies.Inclusions,
                Exclusions = x.Policies.Exclusions
            },
            Extras = x.Extras.Select(e => new IngestionExtraPayload
            {
                Description = e.Description,
                ChargeUnit = e.ChargeUnit,
                Charge = e.Charge
            }).ToList(),
            Specials = x.Specials
        }).ToList()
    };

    public static IngestionPropertyBundleResponse ToResponse(this IngestionPropertyBundleResult result) => new()
    {
        SupplierId = result.SupplierId,
        SupplierName = result.SupplierName,
        ProductCount = result.ProductCount,
        RoomCount = result.RoomCount,
        RateCount = result.RateCount,
        Products = result.Products.Select(x => new IngestionProductResponse
        {
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            RoomCount = x.RoomCount,
            RateCount = x.RateCount,
            RateTypeCount = x.RateTypeCount,
            RateBasisCount = x.RateBasisCount,
            MealBasisCount = x.MealBasisCount,
            ValidityPeriodCount = x.ValidityPeriodCount
        }).ToList()
    };

    public static Product ToEntity(this ProductRequest request) => new()
    {
        SupplierId = request.SupplierId,
        Name = request.Name,
        Type = request.Type,
        ContractValidityPeriod = request.ContractValidityPeriod,
        Commission = request.Commission,
        PhysicalStreetAddress = request.PhysicalAddress.StreetAddress,
        PhysicalSuburb = request.PhysicalAddress.Suburb,
        PhysicalTownOrCity = request.PhysicalAddress.TownOrCity,
        PhysicalStateOrProvince = request.PhysicalAddress.StateOrProvince,
        PhysicalCountry = request.PhysicalAddress.Country,
        PhysicalPostCode = request.PhysicalAddress.PostCode,
        MailingStreetAddress = request.MailingAddress.StreetAddress,
        MailingSuburb = request.MailingAddress.Suburb,
        MailingTownOrCity = request.MailingAddress.TownOrCity,
        MailingStateOrProvince = request.MailingAddress.StateOrProvince,
        MailingCountry = request.MailingAddress.Country,
        MailingPostCode = request.MailingAddress.PostCode,
        CheckInTime = request.CheckInTime,
        CheckOutTime = request.CheckOutTime,
        BlockOutDates = request.BlockOutDates,
        TourismLevyAmount = request.TourismLevy.Amount,
        TourismLevyCurrency = request.TourismLevy.Currency,
        TourismLevyUnit = request.TourismLevy.Unit,
        TourismLevyAgeApplicability = request.TourismLevy.AgeApplicability,
        TourismLevyEffectiveDates = request.TourismLevy.EffectiveDates,
        TourismLevyConditions = request.TourismLevy.Conditions,
        TourismLevyRawText = request.TourismLevy.RawText,
        TourismLevyIncluded = request.TourismLevy.Included,
        RoomPolicies = request.RoomPolicies,
        RatePolicies = request.RatePolicies,
        ChildPolicies = request.ChildPolicies,
        CancellationPolicies = request.CancellationPolicies,
        Inclusions = request.Inclusions,
        Exclusions = request.Exclusions,
        Specials = request.Specials,
        Contacts = request.Contacts.Select(x => new ProductContact
        {
            Id = x.Id ?? Guid.Empty,
            ContactType = x.ContactType,
            ContactName = x.ContactName,
            ContactEmail = x.ContactEmail,
            ContactPhoneNumber = x.ContactPhoneNumber
        }).ToList(),
        Extras = request.Extras.Select(x => new ProductExtra
        {
            Id = x.Id ?? Guid.Empty,
            Description = x.Description,
            ChargeUnit = x.ChargeUnit,
            Charge = x.Charge
        }).ToList(),
        Rooms = request.Rooms.Select(x => new ProductRoom
        {
            Id = x.Id ?? Guid.Empty,
            Name = x.Name,
            MinimumOccupancy = x.MinimumOccupancy,
            MaximumOccupancy = x.MaximumOccupancy,
            AdditionalNotes = x.AdditionalNotes,
            RateConditions = x.RateConditions
        }).ToList(),
        RateTypes = request.RateTypes.Select(x => new ProductRateType
        {
            Id = x.Id ?? Guid.Empty,
            Name = x.Value
        }).ToList(),
        RateBases = request.RateBases.Select(x => new ProductRateBasis
        {
            Id = x.Id ?? Guid.Empty,
            Name = x.Value
        }).ToList(),
        MealBases = request.MealBases.Select(x => new ProductMealBasis
        {
            Id = x.Id ?? Guid.Empty,
            Name = x.Value
        }).ToList(),
        ValidityPeriods = request.ValidityPeriods.Select(x => new ProductValidityPeriod
        {
            Id = x.Id ?? Guid.Empty,
            Value = x.Value
        }).ToList()
    };

    public static SupplierListItemResponse ToListItemResponse(this Supplier supplier) => new()
    {
        Id = supplier.Id,
        Name = supplier.Name,
        Email = supplier.Email,
        Phone = supplier.Phone
    };

    public static SupplierResponse ToResponse(this Supplier supplier) => new()
    {
        Id = supplier.Id,
        Name = supplier.Name,
        Email = supplier.Email,
        Phone = supplier.Phone,
        CreatedAt = supplier.CreatedAt
    };

    public static ProductResponse ToResponse(this Product product) => new()
    {
        Id = product.Id,
        SupplierId = product.SupplierId,
        Name = product.Name,
        Type = product.Type,
        ContractValidityPeriod = product.ContractValidityPeriod,
        Commission = product.Commission,
        PhysicalAddress = new AddressResponse
        {
            StreetAddress = product.PhysicalStreetAddress,
            Suburb = product.PhysicalSuburb,
            TownOrCity = product.PhysicalTownOrCity,
            StateOrProvince = product.PhysicalStateOrProvince,
            Country = product.PhysicalCountry,
            PostCode = product.PhysicalPostCode
        },
        MailingAddress = new AddressResponse
        {
            StreetAddress = product.MailingStreetAddress,
            Suburb = product.MailingSuburb,
            TownOrCity = product.MailingTownOrCity,
            StateOrProvince = product.MailingStateOrProvince,
            Country = product.MailingCountry,
            PostCode = product.MailingPostCode
        },
        CheckInTime = product.CheckInTime,
        CheckOutTime = product.CheckOutTime,
        BlockOutDates = product.BlockOutDates,
        TourismLevy = new TourismLevyResponse
        {
            Amount = product.TourismLevyAmount,
            Currency = product.TourismLevyCurrency,
            Unit = product.TourismLevyUnit,
            AgeApplicability = product.TourismLevyAgeApplicability,
            EffectiveDates = product.TourismLevyEffectiveDates,
            Conditions = product.TourismLevyConditions,
            RawText = product.TourismLevyRawText,
            Included = product.TourismLevyIncluded
        },
        RoomPolicies = product.RoomPolicies,
        RatePolicies = product.RatePolicies,
        ChildPolicies = product.ChildPolicies,
        CancellationPolicies = product.CancellationPolicies,
        Inclusions = product.Inclusions,
        Exclusions = product.Exclusions,
        Specials = product.Specials,
        Contacts = product.Contacts.Select(x => new ProductContactResponse
        {
            Id = x.Id,
            ContactType = x.ContactType,
            ContactName = x.ContactName,
            ContactEmail = x.ContactEmail,
            ContactPhoneNumber = x.ContactPhoneNumber
        }).ToList(),
        Extras = product.Extras.Select(x => new ProductExtraResponse
        {
            Id = x.Id,
            Description = x.Description,
            ChargeUnit = x.ChargeUnit,
            Charge = x.Charge
        }).ToList(),
        Rooms = product.Rooms.Select(x => new ProductRoomResponse
        {
            Id = x.Id,
            Name = x.Name,
            MinimumOccupancy = x.MinimumOccupancy,
            MaximumOccupancy = x.MaximumOccupancy,
            AdditionalNotes = x.AdditionalNotes,
            RateConditions = x.RateConditions
        }).ToList(),
        RateTypes = product.RateTypes.Select(x => new ProductLookupValueResponse
        {
            Id = x.Id,
            Value = x.Name
        }).ToList(),
        RateBases = product.RateBases.Select(x => new ProductLookupValueResponse
        {
            Id = x.Id,
            Value = x.Name
        }).ToList(),
        MealBases = product.MealBases.Select(x => new ProductLookupValueResponse
        {
            Id = x.Id,
            Value = x.Name
        }).ToList(),
        ValidityPeriods = product.ValidityPeriods.Select(x => new ProductLookupValueResponse
        {
            Id = x.Id,
            Value = x.Value
        }).ToList(),
        CreatedAt = product.CreatedAt
    };

    public static ProductListItemResponse ToListItemResponse(this Product product) => new()
    {
        Id = product.Id,
        SupplierId = product.SupplierId,
        Name = product.Name,
        Type = product.Type,
        RoomCount = product.Rooms.Count,
        ContractValidityPeriod = product.ContractValidityPeriod,
        CheckInTime = product.CheckInTime,
        CheckOutTime = product.CheckOutTime
    };

    public static Rate ToEntity(this RateRequest request) => new()
    {
        ProductId = request.ProductId,
        ProductRoomId = request.ProductRoomId,
        SeasonStart = request.SeasonStart,
        SeasonEnd = request.SeasonEnd,
        PricingModel = request.PricingModel,
        BaseCost = request.BaseCost,
        Currency = request.Currency,
        MinPax = request.MinPax,
        MaxPax = request.MaxPax,
        ChildDiscount = request.ChildDiscount,
        SingleSupplement = request.SingleSupplement,
        Capacity = request.Capacity,
        ValidityPeriod = request.ValidityPeriod,
        ValidityPeriodDescription = request.ValidityPeriodDescription,
        RateVariation = request.RateVariation,
        RateTypeName = request.RateTypeName,
        RateBasis = request.RateBasis,
        OccupancyType = request.OccupancyType,
        MealBasis = request.MealBasis,
        MinimumStay = request.MinimumStay
    };

    public static RateResponse ToResponse(this Rate rate) => new()
    {
        Id = rate.Id,
        ProductId = rate.ProductId,
        ProductRoomId = rate.ProductRoomId,
        ProductRoomName = rate.ProductRoom?.Name,
        IsActive = rate.IsActive,
        PreviousRateId = rate.PreviousRateId,
        SupersededAt = rate.SupersededAt,
        SeasonStart = rate.SeasonStart,
        SeasonEnd = rate.SeasonEnd,
        PricingModel = rate.PricingModel,
        BaseCost = rate.BaseCost,
        Currency = rate.Currency,
        MinPax = rate.MinPax,
        MaxPax = rate.MaxPax,
        ChildDiscount = rate.ChildDiscount,
        SingleSupplement = rate.SingleSupplement,
        Capacity = rate.Capacity,
        ValidityPeriod = rate.ValidityPeriod,
        ValidityPeriodDescription = rate.ValidityPeriodDescription,
        RateVariation = rate.RateVariation,
        RateTypeName = rate.RateTypeName,
        RateBasis = rate.RateBasis,
        OccupancyType = rate.OccupancyType,
        MealBasis = rate.MealBasis,
        MinimumStay = rate.MinimumStay,
        CreatedAt = rate.CreatedAt
    };

    public static ItineraryResponse ToResponse(this Itinerary itinerary) => new()
    {
        Id = itinerary.Id,
        LeadCustomerId = itinerary.LeadCustomerId,
        LeadCustomerName = itinerary.LeadCustomer is null ? null : $"{itinerary.LeadCustomer.FirstName} {itinerary.LeadCustomer.LastName}".Trim(),
        StartDate = itinerary.StartDate,
        Duration = itinerary.Duration,
        CreatedAt = itinerary.CreatedAt,
        Items = itinerary.Items.Select(i => new ItineraryItemResponse
        {
            Id = i.Id,
            DayNumber = i.DayNumber,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            Notes = i.Notes
        }).ToList()
    };

    public static QuoteResponse ToResponse(this Quote quote) => new()
    {
        Id = quote.Id,
        ItineraryId = quote.ItineraryId,
        LeadCustomerId = quote.LeadCustomerId,
        LeadCustomerName = quote.LeadCustomer is null ? null : $"{quote.LeadCustomer.FirstName} {quote.LeadCustomer.LastName}".Trim(),
        TotalCost = quote.TotalCost,
        TotalPrice = quote.TotalPrice,
        Margin = quote.Margin,
        Currency = quote.Currency,
        Status = quote.Status,
        CreatedAt = quote.CreatedAt,
        LineItems = quote.LineItems.Select(li => new QuoteLineItemResponse
        {
            ProductId = li.ProductId,
            BaseCost = li.BaseCost,
            AdjustedCost = li.AdjustedCost,
            FinalPrice = li.FinalPrice,
            MarkupPercentage = li.MarkupPercentage,
            Currency = li.Currency
        }).ToList()
    };

    public static BookingListItemResponse ToListItemResponse(this Booking booking) => new()
    {
        Id = booking.Id,
        QuoteId = booking.QuoteId,
        LeadCustomerId = booking.LeadCustomerId,
        LeadCustomerName = booking.LeadCustomer is null ? null : $"{booking.LeadCustomer.FirstName} {booking.LeadCustomer.LastName}".Trim(),
        Status = booking.Status,
        CreatedAt = booking.CreatedAt,
        ItemCount = booking.Items.Count
    };

    public static BookingResponse ToResponse(this Booking booking) => new()
    {
        Id = booking.Id,
        QuoteId = booking.QuoteId,
        LeadCustomerId = booking.LeadCustomerId,
        LeadCustomerName = booking.LeadCustomer is null ? null : $"{booking.LeadCustomer.FirstName} {booking.LeadCustomer.LastName}".Trim(),
        Status = booking.Status,
        CreatedAt = booking.CreatedAt,
        Items = booking.Items.Select(x => x.ToResponse()).ToList(),
        Travellers = booking.Travellers.Select(x => new BookingTravellerResponse
        {
            CustomerId = x.CustomerId,
            CustomerName = $"{x.Customer.FirstName} {x.Customer.LastName}".Trim(),
            RelationshipToLeadCustomer = x.RelationshipToLeadCustomer,
            Notes = x.Notes,
            CreatedAt = x.CreatedAt
        }).ToList()
    };

    public static BookingItemResponse ToResponse(this BookingItem bookingItem) => new()
    {
        Id = bookingItem.Id,
        BookingId = bookingItem.BookingId,
        ProductId = bookingItem.ProductId,
        ProductName = bookingItem.Product?.Name ?? string.Empty,
        SupplierId = bookingItem.SupplierId,
        SupplierName = bookingItem.Supplier?.Name ?? string.Empty,
        Status = bookingItem.Status,
        Notes = bookingItem.Notes,
        CreatedAt = bookingItem.CreatedAt
    };

    public static TaskResponse ToResponse(this OperationalTask task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        AssignedToUserId = task.AssignedToUserId,
        CreatedByUserId = task.CreatedByUserId,
        DueDate = task.DueDate,
        BookingId = task.BookingId,
        BookingItemId = task.BookingItemId,
        RelatedBookingId = task.BookingId ?? task.BookingItem?.BookingId,
        ProductName = task.BookingItem?.Product?.Name,
        SupplierName = task.BookingItem?.Supplier?.Name,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt
    };

    public static TaskSuggestionResponse ToResponse(this OperationalTaskSuggestion suggestion) => new()
    {
        Id = suggestion.Id,
        BookingId = suggestion.BookingId,
        BookingItemId = suggestion.BookingItemId,
        Title = suggestion.Title,
        Description = suggestion.Description,
        SuggestedStatus = suggestion.SuggestedStatus,
        SuggestedDueDate = suggestion.SuggestedDueDate,
        Reason = suggestion.Reason,
        Confidence = suggestion.Confidence,
        RequiresHumanReview = suggestion.RequiresHumanReview,
        State = suggestion.State,
        Source = suggestion.Source,
        AcceptedTaskId = suggestion.AcceptedTaskId,
        ReviewedByUserId = suggestion.ReviewedByUserId,
        CreatedAt = suggestion.CreatedAt,
        ReviewedAt = suggestion.ReviewedAt,
        ProductName = suggestion.BookingItem?.Product?.Name,
        SupplierName = suggestion.BookingItem?.Supplier?.Name
    };

    public static EmailThreadResponse ToResponse(this EmailThread thread) => new()
    {
        Id = thread.Id,
        BookingId = thread.BookingId,
        BookingItemId = thread.BookingItemId,
        RelatedBookingId = thread.BookingId ?? thread.BookingItem?.BookingId,
        Subject = thread.Subject,
        SupplierEmail = thread.SupplierEmail,
        LastMessageAt = thread.LastMessageAt,
        CreatedAt = thread.CreatedAt,
        Messages = thread.Messages.OrderByDescending(x => x.SentAt).Select(x => x.ToResponse()).ToList(),
        Drafts = thread.Drafts.OrderByDescending(x => x.UpdatedAt).Select(x => x.ToResponse()).ToList()
    };

    public static EmailMessageResponse ToResponse(this EmailMessage message) => new()
    {
        Id = message.Id,
        EmailThreadId = message.EmailThreadId,
        Direction = message.Direction,
        Subject = message.Subject,
        BodyText = message.BodyText,
        BodyHtml = message.BodyHtml,
        Sender = message.Sender,
        Recipients = message.Recipients,
        SentAt = message.SentAt,
        RequiresHumanReview = message.RequiresHumanReview,
        AiSummary = message.AiSummary,
        AiClassification = message.AiClassification,
        AiConfidence = message.AiConfidence,
        CreatedAt = message.CreatedAt
    };

    public static EmailDraftResponse ToResponse(this EmailDraft draft) => new()
    {
        Id = draft.Id,
        BookingId = draft.BookingId,
        BookingItemId = draft.BookingItemId,
        EmailThreadId = draft.EmailThreadId,
        Subject = draft.Subject,
        Body = draft.Body,
        Status = draft.Status,
        GeneratedBy = draft.GeneratedBy,
        ApprovedByUserId = draft.ApprovedByUserId,
        ApprovedAt = draft.ApprovedAt,
        SentAt = draft.SentAt,
        GeneratedByAi = draft.GeneratedBy == EmailDraftGeneratedBy.AI,
        LlmProvider = draft.LlmProvider,
        LlmModel = draft.LlmModel,
        CreatedAt = draft.CreatedAt,
        UpdatedAt = draft.UpdatedAt
    };

    public static IngestionRatePayload ToPayload(this IngestionRatePayloadRequest request) => new()
    {
        ProductId = request.ProductId,
        SeasonStart = request.SeasonStart,
        SeasonEnd = request.SeasonEnd,
        PricingModel = request.PricingModel,
        BaseCost = request.BaseCost,
        Currency = request.Currency,
        MinPax = request.MinPax,
        MaxPax = request.MaxPax,
        ChildDiscount = request.ChildDiscount,
        SingleSupplement = request.SingleSupplement,
        Capacity = request.Capacity
    };

    public static CreateItineraryModel ToModel(this CreateItineraryRequest request) => new()
    {
        StartDate = request.StartDate,
        Duration = request.Duration,
        Items = request.Items.Select(x => new CreateItineraryItemModel
        {
            DayNumber = x.DayNumber,
            ProductId = x.ProductId,
            Quantity = x.Quantity,
            Notes = x.Notes
        }).ToList()
    };

    public static ItineraryResponse ToResponse(this ItineraryModel itinerary) => new()
    {
        Id = itinerary.Id,
        LeadCustomerId = itinerary.LeadCustomerId,
        LeadCustomerName = itinerary.LeadCustomerName,
        StartDate = itinerary.StartDate,
        Duration = itinerary.Duration,
        CreatedAt = itinerary.CreatedAt,
        Items = itinerary.Items.Select(item => new ItineraryItemResponse
        {
            Id = item.Id,
            DayNumber = item.DayNumber,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Notes = item.Notes
        }).ToList()
    };

    public static ItineraryProductAssistRequest ToModel(this ProductAssistRequest request) => new()
    {
        Destination = request.Destination,
        Region = request.Region,
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        Season = request.Season,
        TravellerCount = request.TravellerCount,
        BudgetLevel = request.BudgetLevel,
        PreferredCurrency = request.PreferredCurrency,
        TravelStyle = request.TravelStyle,
        Interests = request.Interests,
        AccommodationPreference = request.AccommodationPreference,
        SpecialConstraints = request.SpecialConstraints,
        ProductTypes = request.ProductTypes,
        CustomerBrief = request.CustomerBrief,
        MaxResults = request.MaxResults
    };

    public static ProductAssistResponse ToResponse(this ItineraryProductAssistResult result) => new()
    {
        CandidateCount = result.CandidateCount,
        ReturnedCount = result.ReturnedCount,
        Assumptions = result.Assumptions.ToList(),
        Recommendations = result.Recommendations.Select(x => new ProductRecommendationResponse
        {
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            SupplierName = x.SupplierName,
            ProductType = x.ProductType,
            MatchScore = x.MatchScore,
            Reason = x.Reason,
            Warnings = x.Warnings.ToList(),
            AssumptionFlags = x.AssumptionFlags.ToList(),
            MissingData = x.MissingData.ToList()
        }).ToList()
    };

    public static GenerateItineraryDraftRequest ToModel(this GenerateItineraryDraftRequestDto request) => new()
    {
        Destination = request.Destination,
        Region = request.Region,
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        Duration = request.Duration,
        Season = request.Season,
        TravellerCount = request.TravellerCount,
        BudgetLevel = request.BudgetLevel,
        PreferredCurrency = request.PreferredCurrency,
        TravelStyle = request.TravelStyle,
        Interests = request.Interests,
        AccommodationPreference = request.AccommodationPreference,
        SpecialConstraints = request.SpecialConstraints,
        CustomerBrief = request.CustomerBrief
    };

    public static ItineraryDraftResponse ToResponse(this ItineraryDraftModel draft) => new()
    {
        Id = draft.Id,
        Status = draft.Status,
        ProposedStartDate = draft.ProposedStartDate,
        Duration = draft.Duration,
        CustomerBrief = draft.CustomerBrief,
        LlmProvider = draft.LlmProvider,
        LlmModel = draft.LlmModel,
        PersistedItineraryId = draft.PersistedItineraryId,
        CreatedAt = draft.CreatedAt,
        UpdatedAt = draft.UpdatedAt,
        ApprovedAt = draft.ApprovedAt,
        Assumptions = draft.Assumptions.ToList(),
        Caveats = draft.Caveats.ToList(),
        DataGaps = draft.DataGaps.ToList(),
        Items = draft.Items.Select(x => new ItineraryDraftItemResponse
        {
            Id = x.Id,
            DayNumber = x.DayNumber,
            Sequence = x.Sequence,
            Title = x.Title,
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            SupplierName = x.SupplierName,
            Quantity = x.Quantity,
            Notes = x.Notes,
            Confidence = x.Confidence,
            Reason = x.Reason,
            IsUnresolved = x.IsUnresolved,
            Warnings = x.Warnings.ToList(),
            MissingData = x.MissingData.ToList()
        }).ToList()
    };

    public static ApproveItineraryDraftRequest ToModel(this ApproveItineraryDraftRequestDto request) => new()
    {
        StartDate = request.StartDate,
        Duration = request.Duration,
        DecisionNotes = request.DecisionNotes,
        Items = request.Items.Select(x => new ApproveItineraryDraftItemModel
        {
            DayNumber = x.DayNumber,
            ProductId = x.ProductId,
            Quantity = x.Quantity,
            Notes = x.Notes
        }).ToList()
    };

    public static ItineraryDraftApprovalResponse ToResponse(this ItineraryDraftApprovalResult result) => new()
    {
        DraftId = result.DraftId,
        ApprovalRequestId = result.ApprovalRequestId,
        ApprovedAt = result.ApprovedAt,
        Itinerary = result.Itinerary.ToResponse()
    };

    public static InvoiceIngestionRequestModel ToModel(this InvoiceIngestionRequest request) => new()
    {
        SourceSystem = request.SourceSystem,
        ExternalSourceReference = request.ExternalSourceReference,
        InvoiceNumber = request.InvoiceNumber,
        SupplierId = request.SupplierId,
        SupplierReference = request.SupplierReference,
        SupplierName = request.SupplierName,
        BookingId = request.BookingId,
        BookingReference = request.BookingReference,
        BookingItemId = request.BookingItemId,
        BookingItemReference = request.BookingItemReference,
        QuoteId = request.QuoteId,
        QuoteReference = request.QuoteReference,
        EmailThreadId = request.EmailThreadId,
        InvoiceDate = request.InvoiceDate,
        DueDate = request.DueDate,
        Currency = request.Currency,
        SubtotalAmount = request.SubtotalAmount,
        TaxAmount = request.TaxAmount,
        TotalAmount = request.TotalAmount,
        RebateAmount = request.RebateAmount,
        Notes = request.Notes,
        RawExtractionPayloadJson = request.RawExtractionPayloadJson,
        SourceSnapshotJson = request.SourceSnapshotJson,
        ExtractionConfidence = request.ExtractionConfidence,
        ExtractionIssues = request.ExtractionIssues,
        UnresolvedFields = request.UnresolvedFields,
        LineItems = request.LineItems.Select(x => new InvoiceLineItemInputModel
        {
            ExternalLineReference = x.ExternalLineReference,
            BookingItemId = x.BookingItemId,
            BookingItemReference = x.BookingItemReference,
            Description = x.Description,
            ServiceDate = x.ServiceDate,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            TaxAmount = x.TaxAmount,
            TotalAmount = x.TotalAmount,
            Notes = x.Notes
        }).ToList(),
        Attachments = request.Attachments.Select(x => new InvoiceAttachmentInputModel
        {
            ExternalFileReference = x.ExternalFileReference,
            FileName = x.FileName,
            ContentType = x.ContentType,
            SourceUrl = x.SourceUrl,
            MetadataJson = x.MetadataJson
        }).ToList()
    };

    public static InvoiceIngestionResponse ToResponse(this InvoiceIngestionResultModel result) => new()
    {
        InvoiceId = result.InvoiceId,
        WasExisting = result.WasExisting,
        SupplierId = result.SupplierId,
        BookingId = result.BookingId,
        BookingItemId = result.BookingItemId,
        QuoteId = result.QuoteId,
        EmailThreadId = result.EmailThreadId,
        ReviewTaskId = result.ReviewTaskId,
        FinalStatus = result.FinalStatus,
        UnresolvedFields = result.UnresolvedFields.ToList(),
        Warnings = result.Warnings.ToList()
    };

    public static InvoiceResponse ToResponse(this InvoiceModel invoice) => new()
    {
        Id = invoice.Id,
        SourceSystem = invoice.SourceSystem,
        ExternalSourceReference = invoice.ExternalSourceReference,
        InvoiceNumber = invoice.InvoiceNumber,
        SupplierId = invoice.SupplierId,
        SupplierName = invoice.SupplierName,
        BookingId = invoice.BookingId,
        BookingItemId = invoice.BookingItemId,
        QuoteId = invoice.QuoteId,
        EmailThreadId = invoice.EmailThreadId,
        ReviewTaskId = invoice.ReviewTaskId,
        InvoiceDate = invoice.InvoiceDate,
        DueDate = invoice.DueDate,
        Currency = invoice.Currency,
        SubtotalAmount = invoice.SubtotalAmount,
        TaxAmount = invoice.TaxAmount,
        TotalAmount = invoice.TotalAmount,
        RebateAmount = invoice.RebateAmount,
        AmountPaid = invoice.AmountPaid,
        OutstandingAmount = invoice.OutstandingAmount,
        Notes = invoice.Notes,
        ExtractionConfidence = invoice.ExtractionConfidence,
        ExtractionIssues = invoice.ExtractionIssues.ToList(),
        UnresolvedFields = invoice.UnresolvedFields.ToList(),
        RequiresHumanReview = invoice.RequiresHumanReview,
        Status = invoice.Status,
        ReceivedAt = invoice.ReceivedAt,
        CreatedAt = invoice.CreatedAt,
        UpdatedAt = invoice.UpdatedAt,
        LineItems = invoice.LineItems.Select(x => new InvoiceLineItemResponse
        {
            Id = x.Id,
            BookingItemId = x.BookingItemId,
            Description = x.Description,
            ServiceDate = x.ServiceDate,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            TaxAmount = x.TaxAmount,
            TotalAmount = x.TotalAmount,
            Notes = x.Notes
        }).ToList(),
        Attachments = invoice.Attachments.Select(x => new InvoiceAttachmentResponse
        {
            Id = x.Id,
            ExternalFileReference = x.ExternalFileReference,
            FileName = x.FileName,
            ContentType = x.ContentType,
            SourceUrl = x.SourceUrl,
            CreatedAt = x.CreatedAt
        }).ToList(),
        PaymentRecords = invoice.PaymentRecords.Select(x => new PaymentRecordResponse
        {
            Id = x.Id,
            ExternalPaymentReference = x.ExternalPaymentReference,
            Amount = x.Amount,
            Currency = x.Currency,
            PaidAt = x.PaidAt,
            PaymentMethod = x.PaymentMethod,
            Notes = x.Notes,
            RecordedByUserId = x.RecordedByUserId,
            CreatedAt = x.CreatedAt
        }).ToList()
    };

    public static InvoiceListResponse ToResponse(this InvoiceListItemModel invoice) => new()
    {
        Id = invoice.Id,
        InvoiceNumber = invoice.InvoiceNumber,
        SupplierName = invoice.SupplierName,
        SupplierId = invoice.SupplierId,
        BookingId = invoice.BookingId,
        BookingItemId = invoice.BookingItemId,
        InvoiceDate = invoice.InvoiceDate,
        DueDate = invoice.DueDate,
        Currency = invoice.Currency,
        TotalAmount = invoice.TotalAmount,
        AmountPaid = invoice.AmountPaid,
        OutstandingAmount = invoice.OutstandingAmount,
        RequiresHumanReview = invoice.RequiresHumanReview,
        Status = invoice.Status
    };

    public static UpdateInvoiceStatusModel ToModel(this UpdateInvoiceStatusRequest request) => new()
    {
        Status = request.Status,
        Notes = request.Notes
    };

    public static RelinkInvoiceModel ToModel(this RelinkInvoiceRequest request) => new()
    {
        SupplierId = request.SupplierId,
        SupplierName = request.SupplierName,
        BookingId = request.BookingId,
        BookingItemId = request.BookingItemId,
        QuoteId = request.QuoteId,
        EmailThreadId = request.EmailThreadId,
        Notes = request.Notes
    };

    public static RecordInvoicePaymentModel ToModel(this RecordInvoicePaymentRequest request) => new()
    {
        ExternalPaymentReference = request.ExternalPaymentReference,
        Amount = request.Amount,
        Currency = request.Currency,
        PaidAt = request.PaidAt,
        PaymentMethod = request.PaymentMethod,
        Notes = request.Notes,
        MetadataJson = request.MetadataJson
    };

    public static ApplyInvoiceRebateModel ToModel(this ApplyInvoiceRebateRequest request) => new()
    {
        Notes = request.Notes
    };

    public static CustomerCreateModel ToCreateModel(this CustomerRequest request) => new()
    {
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email,
        Phone = request.Phone,
        Nationality = request.Nationality,
        CountryOfResidence = request.CountryOfResidence,
        DateOfBirth = request.DateOfBirth,
        PreferredContactMethod = request.PreferredContactMethod,
        Notes = request.Notes
    };

    public static CustomerUpdateModel ToUpdateModel(this CustomerRequest request) => new()
    {
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email,
        Phone = request.Phone,
        Nationality = request.Nationality,
        CountryOfResidence = request.CountryOfResidence,
        DateOfBirth = request.DateOfBirth,
        PreferredContactMethod = request.PreferredContactMethod,
        Notes = request.Notes
    };

    public static CustomerKycUpdateModel ToModel(this CustomerKycRequest request) => new()
    {
        PassportNumber = request.PassportNumber,
        DocumentReference = request.DocumentReference,
        PassportExpiry = request.PassportExpiry,
        IssuingCountry = request.IssuingCountry,
        VisaNotes = request.VisaNotes,
        EmergencyContactName = request.EmergencyContactName,
        EmergencyContactPhone = request.EmergencyContactPhone,
        EmergencyContactRelationship = request.EmergencyContactRelationship,
        VerificationStatus = request.VerificationStatus,
        VerificationNotes = request.VerificationNotes,
        ProfileDataConsentGranted = request.ProfileDataConsentGranted,
        KycDataConsentGranted = request.KycDataConsentGranted
    };

    public static CustomerPreferenceUpdateModel ToModel(this CustomerPreferenceRequest request) => new()
    {
        BudgetBand = request.BudgetBand,
        AccommodationPreference = request.AccommodationPreference,
        RoomPreference = request.RoomPreference,
        DietaryRequirements = request.DietaryRequirements,
        ActivityPreferences = request.ActivityPreferences,
        AccessibilityRequirements = request.AccessibilityRequirements,
        PaceOfTravel = request.PaceOfTravel,
        ValueLeaning = request.ValueLeaning,
        TransportPreferences = request.TransportPreferences,
        SpecialOccasions = request.SpecialOccasions,
        DislikedExperiences = request.DislikedExperiences,
        PreferredDestinations = request.PreferredDestinations,
        AvoidedDestinations = request.AvoidedDestinations,
        OperatorNotes = request.OperatorNotes
    };

    public static BookingTravellerUpsertModel ToModel(this BookingTravellerRequest request) => new()
    {
        RelationshipToLeadCustomer = request.RelationshipToLeadCustomer,
        Notes = request.Notes
    };

    public static CustomerListItemResponse ToResponse(this CustomerListItemModel customer) => new()
    {
        Id = customer.Id,
        FullName = customer.FullName,
        Email = customer.Email,
        Phone = customer.Phone,
        Nationality = customer.Nationality,
        CountryOfResidence = customer.CountryOfResidence,
        PreferredContactMethod = customer.PreferredContactMethod,
        UpdatedAt = customer.UpdatedAt
    };

    public static CustomerResponse ToResponse(this CustomerModel customer) => new()
    {
        Id = customer.Id,
        FirstName = customer.FirstName,
        LastName = customer.LastName,
        FullName = customer.FullName,
        Email = customer.Email,
        Phone = customer.Phone,
        Nationality = customer.Nationality,
        CountryOfResidence = customer.CountryOfResidence,
        DateOfBirth = customer.DateOfBirth,
        PreferredContactMethod = customer.PreferredContactMethod,
        Notes = customer.Notes,
        CreatedAt = customer.CreatedAt,
        UpdatedAt = customer.UpdatedAt,
        Kyc = new CustomerKycResponse
        {
            PassportNumber = customer.Kyc.PassportNumber,
            DocumentReference = customer.Kyc.DocumentReference,
            PassportExpiry = customer.Kyc.PassportExpiry,
            IssuingCountry = customer.Kyc.IssuingCountry,
            VisaNotes = customer.Kyc.VisaNotes,
            EmergencyContactName = customer.Kyc.EmergencyContactName,
            EmergencyContactPhone = customer.Kyc.EmergencyContactPhone,
            EmergencyContactRelationship = customer.Kyc.EmergencyContactRelationship,
            VerificationStatus = customer.Kyc.VerificationStatus,
            VerificationNotes = customer.Kyc.VerificationNotes,
            ProfileDataConsentGranted = customer.Kyc.ProfileDataConsentGranted,
            KycDataConsentGranted = customer.Kyc.KycDataConsentGranted,
            UpdatedAt = customer.Kyc.UpdatedAt
        },
        Preferences = new CustomerPreferenceResponse
        {
            BudgetBand = customer.Preferences.BudgetBand,
            AccommodationPreference = customer.Preferences.AccommodationPreference,
            RoomPreference = customer.Preferences.RoomPreference,
            DietaryRequirements = customer.Preferences.DietaryRequirements.ToList(),
            ActivityPreferences = customer.Preferences.ActivityPreferences.ToList(),
            AccessibilityRequirements = customer.Preferences.AccessibilityRequirements.ToList(),
            PaceOfTravel = customer.Preferences.PaceOfTravel,
            ValueLeaning = customer.Preferences.ValueLeaning,
            TransportPreferences = customer.Preferences.TransportPreferences.ToList(),
            SpecialOccasions = customer.Preferences.SpecialOccasions.ToList(),
            DislikedExperiences = customer.Preferences.DislikedExperiences.ToList(),
            PreferredDestinations = customer.Preferences.PreferredDestinations.ToList(),
            AvoidedDestinations = customer.Preferences.AvoidedDestinations.ToList(),
            OperatorNotes = customer.Preferences.OperatorNotes,
            UpdatedAt = customer.Preferences.UpdatedAt
        },
        LeadQuoteIds = customer.LeadQuoteIds.ToList(),
        LeadItineraryIds = customer.LeadItineraryIds.ToList(),
        LeadBookingIds = customer.LeadBookingIds.ToList(),
        TravellerBookings = customer.TravellerBookings.Select(x => new BookingTravellerLinkResponse
        {
            BookingId = x.BookingId,
            RelationshipToLeadCustomer = x.RelationshipToLeadCustomer,
            Notes = x.Notes,
            CreatedAt = x.CreatedAt
        }).ToList()
    };

    public static CustomerLinkResponse ToResponse(this CustomerLinkResultModel result) => new()
    {
        CustomerId = result.CustomerId,
        TargetId = result.TargetId,
        TargetType = result.TargetType
    };
}
