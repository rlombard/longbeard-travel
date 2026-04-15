using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class SignupBillingIntent
{
    public Guid Id { get; set; }
    public Guid SignupSessionId { get; set; }
    public Guid LicensePlanId { get; set; }
    public SignupBillingStatus Status { get; set; }
    public BillingMode BillingMode { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public SignupSession SignupSession { get; set; } = null!;
    public LicensePlan LicensePlan { get; set; } = null!;
}
