using AI.Forged.TourOps.Application.Models;
using AI.Forged.TourOps.Domain.Entities;

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
}
