using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class TenantLicense
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid LicensePlanId { get; set; }
    public LicenseStatus Status { get; set; }
    public BillingMode BillingMode { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? MaxUsersOverride { get; set; }
    public int? MaxIntegrationsOverride { get; set; }
    public int? MaxEmailAccountsOverride { get; set; }
    public int? MaxMonthlyAiJobsOverride { get; set; }
    public int? MaxMonthlyEmailSendsOverride { get; set; }
    public int? MaxMonthlySyncOperationsOverride { get; set; }
    public int? MaxStorageMbOverride { get; set; }
    public string FeatureOverridesJson { get; set; } = "[]";
    public string? BillingCustomerReference { get; set; }
    public string? SubscriptionReference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public LicensePlan LicensePlan { get; set; } = null!;
}
