using System.Text.Json.Serialization;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Api.Models;

public sealed class IngestionRatePayloadRequest
{
    public Guid ProductId { get; set; }
    public DateOnly SeasonStart { get; set; }
    public DateOnly SeasonEnd { get; set; }
    public PricingModel PricingModel { get; set; }
    public decimal BaseCost { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int? MinPax { get; set; }
    public int? MaxPax { get; set; }
    public decimal? ChildDiscount { get; set; }
    public decimal? SingleSupplement { get; set; }
    public int? Capacity { get; set; }
}

public sealed class IngestionPropertyBundleRequest
{
    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;

    [JsonPropertyName("propertyRoomMeta")]
    public List<string> PropertyRoomMeta { get; set; } = [];

    [JsonPropertyName("propertyDetails")]
    public List<IngestionPropertyDetailRequest> PropertyDetails { get; set; } = [];

    [JsonPropertyName("roomPricingDetails")]
    public List<IngestionRoomPricingRequest> RoomPricingDetails { get; set; } = [];

    [JsonPropertyName("propertyContentDetails")]
    public List<IngestionPropertyContentRequest> PropertyContentDetails { get; set; } = [];
}

public sealed class IngestionPropertyDetailRequest
{
    [JsonPropertyName("n")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("cr")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("vp")]
    public string ValidityPeriod { get; set; } = string.Empty;

    [JsonPropertyName("co")]
    public string Commission { get; set; } = string.Empty;

    [JsonPropertyName("pa")]
    public IngestionAddressRequest PhysicalAddress { get; set; } = new();

    [JsonPropertyName("ma")]
    public IngestionAddressRequest MailingAddress { get; set; } = new();

    [JsonPropertyName("cit")]
    public string CheckInTime { get; set; } = string.Empty;

    [JsonPropertyName("cot")]
    public string CheckOutTime { get; set; } = string.Empty;

    [JsonPropertyName("bo")]
    public string BlockOutDates { get; set; } = string.Empty;

    [JsonPropertyName("cl")]
    public IngestionTourismLevyRequest TourismLevy { get; set; } = new();

    [JsonPropertyName("cli")]
    public bool TourismLevyIncluded { get; set; }

    [JsonPropertyName("c")]
    public List<IngestionContactRequest> Contacts { get; set; } = [];
}

public sealed class IngestionAddressRequest
{
    [JsonPropertyName("sa")]
    public string StreetAddress { get; set; } = string.Empty;

    [JsonPropertyName("su")]
    public string Suburb { get; set; } = string.Empty;

    [JsonPropertyName("tc")]
    public string TownOrCity { get; set; } = string.Empty;

    [JsonPropertyName("sp")]
    public string StateOrProvince { get; set; } = string.Empty;

    [JsonPropertyName("co")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("pc")]
    public string PostCode { get; set; } = string.Empty;
}

public sealed class IngestionTourismLevyRequest
{
    [JsonPropertyName("a")]
    public string Amount { get; set; } = string.Empty;

    [JsonPropertyName("cr")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("u")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("aa")]
    public string AgeApplicability { get; set; } = string.Empty;

    [JsonPropertyName("ed")]
    public string EffectiveDates { get; set; } = string.Empty;

    [JsonPropertyName("cond")]
    public string Conditions { get; set; } = string.Empty;

    [JsonPropertyName("rt")]
    public string RawText { get; set; } = string.Empty;
}

public sealed class IngestionContactRequest
{
    [JsonPropertyName("ct")]
    public string ContactType { get; set; } = string.Empty;

    [JsonPropertyName("cn")]
    public string ContactName { get; set; } = string.Empty;

    [JsonPropertyName("ce")]
    public string ContactEmail { get; set; } = string.Empty;

    [JsonPropertyName("ctn")]
    public string ContactPhoneNumber { get; set; } = string.Empty;
}

public sealed class IngestionRoomPricingRequest
{
    [JsonPropertyName("n")]
    public string PropertyName { get; set; } = string.Empty;

    [JsonPropertyName("r")]
    public List<IngestionRoomRequest> Rooms { get; set; } = [];
}

public sealed class IngestionRoomRequest
{
    [JsonPropertyName("rn")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ra")]
    public List<IngestionRoomAvailabilityRequest> RateAvailability { get; set; } = [];

    [JsonPropertyName("mip")]
    public string MinimumOccupancy { get; set; } = string.Empty;

    [JsonPropertyName("map")]
    public string MaximumOccupancy { get; set; } = string.Empty;

    [JsonPropertyName("an")]
    public string AdditionalNotes { get; set; } = string.Empty;

    [JsonPropertyName("rc")]
    public string RateConditions { get; set; } = string.Empty;
}

public sealed class IngestionRoomAvailabilityRequest
{
    [JsonPropertyName("vp")]
    public string ValidityPeriod { get; set; } = string.Empty;

    [JsonPropertyName("vpd")]
    public string ValidityPeriodDescription { get; set; } = string.Empty;

    [JsonPropertyName("rp")]
    public List<IngestionRoomRateRequest> Rates { get; set; } = [];
}

public sealed class IngestionRoomRateRequest
{
    [JsonPropertyName("rv")]
    public string RateVariation { get; set; } = string.Empty;

    [JsonPropertyName("rnp")]
    public string RateTypeName { get; set; } = string.Empty;

    [JsonPropertyName("rnv")]
    public decimal RateValue { get; set; }

    [JsonPropertyName("pb")]
    public string RateBasis { get; set; } = string.Empty;

    [JsonPropertyName("cu")]
    public string OccupancyType { get; set; } = string.Empty;

    [JsonPropertyName("mb")]
    public string MealBasis { get; set; } = string.Empty;

    [JsonPropertyName("ms")]
    public string MinimumStay { get; set; } = string.Empty;
}

public sealed class IngestionPropertyContentRequest
{
    [JsonPropertyName("n")]
    public string PropertyName { get; set; } = string.Empty;

    [JsonPropertyName("policies")]
    public IngestionPoliciesRequest Policies { get; set; } = new();

    [JsonPropertyName("extras")]
    public List<IngestionExtraRequest> Extras { get; set; } = [];

    [JsonPropertyName("specials")]
    public string Specials { get; set; } = string.Empty;
}

public sealed class IngestionPoliciesRequest
{
    [JsonPropertyName("rmp")]
    public string RoomPolicies { get; set; } = string.Empty;

    [JsonPropertyName("rap")]
    public string RatePolicies { get; set; } = string.Empty;

    [JsonPropertyName("chp")]
    public string ChildPolicies { get; set; } = string.Empty;

    [JsonPropertyName("cp")]
    public string CancellationPolicies { get; set; } = string.Empty;

    [JsonPropertyName("i")]
    public string Inclusions { get; set; } = string.Empty;

    [JsonPropertyName("e")]
    public string Exclusions { get; set; } = string.Empty;
}

public sealed class IngestionExtraRequest
{
    [JsonPropertyName("ed")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("ecu")]
    public string ChargeUnit { get; set; } = string.Empty;

    [JsonPropertyName("ec")]
    public string Charge { get; set; } = string.Empty;
}

public sealed class IngestionPropertyBundleResponse
{
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int RoomCount { get; set; }
    public int RateCount { get; set; }
    public List<IngestionProductResponse> Products { get; set; } = [];
}

public sealed class IngestionProductResponse
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
