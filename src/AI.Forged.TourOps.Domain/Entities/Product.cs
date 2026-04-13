using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductType Type { get; set; }
    public string? ContractValidityPeriod { get; set; }
    public string? Commission { get; set; }
    public string? PhysicalStreetAddress { get; set; }
    public string? PhysicalSuburb { get; set; }
    public string? PhysicalTownOrCity { get; set; }
    public string? PhysicalStateOrProvince { get; set; }
    public string? PhysicalCountry { get; set; }
    public string? PhysicalPostCode { get; set; }
    public string? MailingStreetAddress { get; set; }
    public string? MailingSuburb { get; set; }
    public string? MailingTownOrCity { get; set; }
    public string? MailingStateOrProvince { get; set; }
    public string? MailingCountry { get; set; }
    public string? MailingPostCode { get; set; }
    public string? CheckInTime { get; set; }
    public string? CheckOutTime { get; set; }
    public string? BlockOutDates { get; set; }
    public string? TourismLevyAmount { get; set; }
    public string? TourismLevyCurrency { get; set; }
    public string? TourismLevyUnit { get; set; }
    public string? TourismLevyAgeApplicability { get; set; }
    public string? TourismLevyEffectiveDates { get; set; }
    public string? TourismLevyConditions { get; set; }
    public string? TourismLevyRawText { get; set; }
    public bool TourismLevyIncluded { get; set; }
    public string? RoomPolicies { get; set; }
    public string? RatePolicies { get; set; }
    public string? ChildPolicies { get; set; }
    public string? CancellationPolicies { get; set; }
    public string? Inclusions { get; set; }
    public string? Exclusions { get; set; }
    public string? Specials { get; set; }
    public DateTime CreatedAt { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public ICollection<Rate> Rates { get; set; } = new List<Rate>();
    public ICollection<ProductContact> Contacts { get; set; } = new List<ProductContact>();
    public ICollection<ProductExtra> Extras { get; set; } = new List<ProductExtra>();
    public ICollection<ProductRoom> Rooms { get; set; } = new List<ProductRoom>();
    public ICollection<ProductRateType> RateTypes { get; set; } = new List<ProductRateType>();
    public ICollection<ProductRateBasis> RateBases { get; set; } = new List<ProductRateBasis>();
    public ICollection<ProductMealBasis> MealBases { get; set; } = new List<ProductMealBasis>();
    public ICollection<ProductValidityPeriod> ValidityPeriods { get; set; } = new List<ProductValidityPeriod>();
}
