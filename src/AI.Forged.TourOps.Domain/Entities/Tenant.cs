using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? BillingEmail { get; set; }
    public string DefaultCurrency { get; set; } = "USD";
    public string TimeZone { get; set; } = "UTC";
    public TenantStatus Status { get; set; }
    public bool IsStandaloneTenant { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TenantLicense? License { get; set; }
    public TenantOnboardingState? Onboarding { get; set; }
    public ICollection<TenantUserMembership> UserMemberships { get; set; } = new List<TenantUserMembership>();
    public ICollection<TenantConfigEntry> ConfigEntries { get; set; } = new List<TenantConfigEntry>();
    public ICollection<TenantIdentityMapping> IdentityMappings { get; set; } = new List<TenantIdentityMapping>();
    public ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
    public ICollection<MonetizationTransaction> MonetizationTransactions { get; set; } = new List<MonetizationTransaction>();
    public ICollection<AuditEvent> AuditEvents { get; set; } = new List<AuditEvent>();
    public ICollection<SignupSession> SignupSessions { get; set; } = new List<SignupSession>();
}
