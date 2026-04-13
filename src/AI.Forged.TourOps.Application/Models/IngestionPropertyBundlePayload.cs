namespace AI.Forged.TourOps.Application.Models;

public sealed class IngestionPropertyBundlePayload
{
    public string Owner { get; set; } = string.Empty;
    public List<string> PropertyRoomMeta { get; set; } = [];
    public List<IngestionPropertyDetailPayload> PropertyDetails { get; set; } = [];
    public List<IngestionRoomPricingPayload> RoomPricingDetails { get; set; } = [];
    public List<IngestionPropertyContentPayload> PropertyContentDetails { get; set; } = [];
}

public sealed class IngestionPropertyDetailPayload
{
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string ValidityPeriod { get; set; } = string.Empty;
    public string Commission { get; set; } = string.Empty;
    public IngestionAddressPayload PhysicalAddress { get; set; } = new();
    public IngestionAddressPayload MailingAddress { get; set; } = new();
    public string CheckInTime { get; set; } = string.Empty;
    public string CheckOutTime { get; set; } = string.Empty;
    public string BlockOutDates { get; set; } = string.Empty;
    public IngestionTourismLevyPayload TourismLevy { get; set; } = new();
    public bool TourismLevyIncluded { get; set; }
    public List<IngestionContactPayload> Contacts { get; set; } = [];
}

public sealed class IngestionAddressPayload
{
    public string StreetAddress { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public string TownOrCity { get; set; } = string.Empty;
    public string StateOrProvince { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostCode { get; set; } = string.Empty;
}

public sealed class IngestionTourismLevyPayload
{
    public string Amount { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string AgeApplicability { get; set; } = string.Empty;
    public string EffectiveDates { get; set; } = string.Empty;
    public string Conditions { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
}

public sealed class IngestionContactPayload
{
    public string ContactType { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
}

public sealed class IngestionRoomPricingPayload
{
    public string PropertyName { get; set; } = string.Empty;
    public List<IngestionRoomPayload> Rooms { get; set; } = [];
}

public sealed class IngestionRoomPayload
{
    public string Name { get; set; } = string.Empty;
    public List<IngestionRoomAvailabilityPayload> RateAvailability { get; set; } = [];
    public string MinimumOccupancy { get; set; } = string.Empty;
    public string MaximumOccupancy { get; set; } = string.Empty;
    public string AdditionalNotes { get; set; } = string.Empty;
    public string RateConditions { get; set; } = string.Empty;
}

public sealed class IngestionRoomAvailabilityPayload
{
    public string ValidityPeriod { get; set; } = string.Empty;
    public string ValidityPeriodDescription { get; set; } = string.Empty;
    public List<IngestionRoomRatePayload> Rates { get; set; } = [];
}

public sealed class IngestionRoomRatePayload
{
    public string RateVariation { get; set; } = string.Empty;
    public string RateTypeName { get; set; } = string.Empty;
    public decimal RateValue { get; set; }
    public string RateBasis { get; set; } = string.Empty;
    public string OccupancyType { get; set; } = string.Empty;
    public string MealBasis { get; set; } = string.Empty;
    public string MinimumStay { get; set; } = string.Empty;
}

public sealed class IngestionPropertyContentPayload
{
    public string PropertyName { get; set; } = string.Empty;
    public IngestionPoliciesPayload Policies { get; set; } = new();
    public List<IngestionExtraPayload> Extras { get; set; } = [];
    public string Specials { get; set; } = string.Empty;
}

public sealed class IngestionPoliciesPayload
{
    public string RoomPolicies { get; set; } = string.Empty;
    public string RatePolicies { get; set; } = string.Empty;
    public string ChildPolicies { get; set; } = string.Empty;
    public string CancellationPolicies { get; set; } = string.Empty;
    public string Inclusions { get; set; } = string.Empty;
    public string Exclusions { get; set; } = string.Empty;
}

public sealed class IngestionExtraPayload
{
    public string Description { get; set; } = string.Empty;
    public string ChargeUnit { get; set; } = string.Empty;
    public string Charge { get; set; } = string.Empty;
}

public sealed class IngestionPropertyBundleResult
{
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int RoomCount { get; set; }
    public int RateCount { get; set; }
    public List<IngestionProductResult> Products { get; set; } = [];
}

public sealed class IngestionProductResult
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RoomCount { get; set; }
    public int RateCount { get; set; }
    public int RateTypeCount { get; set; }
    public int RateBasisCount { get; set; }
    public int MealBasisCount { get; set; }
    public int ValidityPeriodCount { get; set; }
}
