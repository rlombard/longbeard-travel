using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class SignupSession
{
    public Guid Id { get; set; }
    public string AccessTokenHash { get; set; } = string.Empty;
    public SignupSessionStatus Status { get; set; }
    public string CurrentStep { get; set; } = "welcome";
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public Guid? SelectedPlanId { get; set; }
    public string? SelectedPlanCode { get; set; }
    public SignupBillingStatus BillingStatus { get; set; }
    public Guid? BillingIntentId { get; set; }
    public Guid? TenantId { get; set; }
    public bool TermsAccepted { get; set; }
    public DateTime? TermsAcceptedAt { get; set; }
    public string? OrganizationName { get; set; }
    public string? OrganizationLegalName { get; set; }
    public string? TenantSlug { get; set; }
    public string? BillingEmail { get; set; }
    public string? DefaultCurrency { get; set; }
    public string? TimeZone { get; set; }
    public string? OrganizationProfileJson { get; set; }
    public string? AdminEmail { get; set; }
    public string? AdminFirstName { get; set; }
    public string? AdminLastName { get; set; }
    public string? AdminUsername { get; set; }
    public string? AdminBootstrapJson { get; set; }
    public string? ActivationResultJson { get; set; }
    public string? LastError { get; set; }
    public int ProvisioningAttemptCount { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public LicensePlan? SelectedPlan { get; set; }
    public Tenant? Tenant { get; set; }
    public SignupEmailVerification? EmailVerification { get; set; }
    public SignupBillingIntent? BillingIntent { get; set; }
}
