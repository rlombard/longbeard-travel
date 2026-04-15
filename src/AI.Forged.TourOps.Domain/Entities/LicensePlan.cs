using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class LicensePlan
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsStandalonePlan { get; set; }
    public bool IsPublicSignupEnabled { get; set; }
    public LicenseSignupKind SignupKind { get; set; }
    public int SignupSortOrder { get; set; }
    public int TrialDays { get; set; }
    public decimal MonthlyPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public bool RequiresTermsAcceptance { get; set; }
    public int MaxUsers { get; set; }
    public int MaxIntegrations { get; set; }
    public int MaxEmailAccounts { get; set; }
    public int MaxMonthlyAiJobs { get; set; }
    public int MaxMonthlyEmailSends { get; set; }
    public int MaxMonthlySyncOperations { get; set; }
    public int MaxStorageMb { get; set; }
    public string IncludedFeaturesJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<TenantLicense> TenantLicenses { get; set; } = new List<TenantLicense>();
    public ICollection<SignupSession> SignupSessions { get; set; } = new List<SignupSession>();
    public ICollection<SignupBillingIntent> SignupBillingIntents { get; set; } = new List<SignupBillingIntent>();
}
