using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Api.Models;

public sealed class AddressRequest
{
    public string? StreetAddress { get; set; }
    public string? Suburb { get; set; }
    public string? TownOrCity { get; set; }
    public string? StateOrProvince { get; set; }
    public string? Country { get; set; }
    public string? PostCode { get; set; }
}

public sealed class AddressResponse
{
    public string? StreetAddress { get; set; }
    public string? Suburb { get; set; }
    public string? TownOrCity { get; set; }
    public string? StateOrProvince { get; set; }
    public string? Country { get; set; }
    public string? PostCode { get; set; }
}

public sealed class TourismLevyRequest
{
    public string? Amount { get; set; }
    public string? Currency { get; set; }
    public string? Unit { get; set; }
    public string? AgeApplicability { get; set; }
    public string? EffectiveDates { get; set; }
    public string? Conditions { get; set; }
    public string? RawText { get; set; }
    public bool Included { get; set; }
}

public sealed class TourismLevyResponse
{
    public string? Amount { get; set; }
    public string? Currency { get; set; }
    public string? Unit { get; set; }
    public string? AgeApplicability { get; set; }
    public string? EffectiveDates { get; set; }
    public string? Conditions { get; set; }
    public string? RawText { get; set; }
    public bool Included { get; set; }
}

public sealed class ProductContactRequest
{
    public Guid? Id { get; set; }
    public string ContactType { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
}

public sealed class ProductContactResponse
{
    public Guid Id { get; set; }
    public string ContactType { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
}

public sealed class ProductExtraRequest
{
    public Guid? Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ChargeUnit { get; set; } = string.Empty;
    public string Charge { get; set; } = string.Empty;
}

public sealed class ProductExtraResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ChargeUnit { get; set; } = string.Empty;
    public string Charge { get; set; } = string.Empty;
}

public sealed class ProductRoomRequest
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MinimumOccupancy { get; set; }
    public string? MaximumOccupancy { get; set; }
    public string? AdditionalNotes { get; set; }
    public string? RateConditions { get; set; }
}

public sealed class ProductRoomResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MinimumOccupancy { get; set; }
    public string? MaximumOccupancy { get; set; }
    public string? AdditionalNotes { get; set; }
    public string? RateConditions { get; set; }
}

public sealed class ProductLookupValueRequest
{
    public Guid? Id { get; set; }
    public string Value { get; set; } = string.Empty;
}

public sealed class ProductLookupValueResponse
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
}

public sealed class ProductRequest
{
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductType Type { get; set; }
    public string? ContractValidityPeriod { get; set; }
    public string? Commission { get; set; }
    public AddressRequest PhysicalAddress { get; set; } = new();
    public AddressRequest MailingAddress { get; set; } = new();
    public string? CheckInTime { get; set; }
    public string? CheckOutTime { get; set; }
    public string? BlockOutDates { get; set; }
    public TourismLevyRequest TourismLevy { get; set; } = new();
    public string? RoomPolicies { get; set; }
    public string? RatePolicies { get; set; }
    public string? ChildPolicies { get; set; }
    public string? CancellationPolicies { get; set; }
    public string? Inclusions { get; set; }
    public string? Exclusions { get; set; }
    public string? Specials { get; set; }
    public List<ProductContactRequest> Contacts { get; set; } = [];
    public List<ProductExtraRequest> Extras { get; set; } = [];
    public List<ProductRoomRequest> Rooms { get; set; } = [];
    public List<ProductLookupValueRequest> RateTypes { get; set; } = [];
    public List<ProductLookupValueRequest> RateBases { get; set; } = [];
    public List<ProductLookupValueRequest> MealBases { get; set; } = [];
    public List<ProductLookupValueRequest> ValidityPeriods { get; set; } = [];
}

public sealed class ProductResponse
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductType Type { get; set; }
    public string? ContractValidityPeriod { get; set; }
    public string? Commission { get; set; }
    public AddressResponse PhysicalAddress { get; set; } = new();
    public AddressResponse MailingAddress { get; set; } = new();
    public string? CheckInTime { get; set; }
    public string? CheckOutTime { get; set; }
    public string? BlockOutDates { get; set; }
    public TourismLevyResponse TourismLevy { get; set; } = new();
    public string? RoomPolicies { get; set; }
    public string? RatePolicies { get; set; }
    public string? ChildPolicies { get; set; }
    public string? CancellationPolicies { get; set; }
    public string? Inclusions { get; set; }
    public string? Exclusions { get; set; }
    public string? Specials { get; set; }
    public List<ProductContactResponse> Contacts { get; set; } = [];
    public List<ProductExtraResponse> Extras { get; set; } = [];
    public List<ProductRoomResponse> Rooms { get; set; } = [];
    public List<ProductLookupValueResponse> RateTypes { get; set; } = [];
    public List<ProductLookupValueResponse> RateBases { get; set; } = [];
    public List<ProductLookupValueResponse> MealBases { get; set; } = [];
    public List<ProductLookupValueResponse> ValidityPeriods { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public sealed class ProductListItemResponse
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductType Type { get; set; }
    public int RoomCount { get; set; }
    public string? ContractValidityPeriod { get; set; }
    public string? CheckInTime { get; set; }
    public string? CheckOutTime { get; set; }
}
