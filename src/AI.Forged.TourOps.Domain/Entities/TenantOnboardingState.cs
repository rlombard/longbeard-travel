using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class TenantOnboardingState
{
    public Guid TenantId { get; set; }
    public OnboardingStatus Status { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public string CompletedStepsJson { get; set; } = "[]";
    public string? OrganizationProfileJson { get; set; }
    public string? AdminBootstrapJson { get; set; }
    public string? EmailSetupJson { get; set; }
    public string? BillingSetupJson { get; set; }
    public string? LastError { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
