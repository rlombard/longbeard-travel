using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Models.Platform;

public sealed class SignupBootstrapModel
{
    public bool Enabled { get; init; }
    public string DisabledReason { get; init; } = string.Empty;
    public bool AllowTestPaymentConfirmation { get; init; }
    public string SupportEmail { get; init; } = string.Empty;
}

public sealed class SignupPlanModel
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public LicenseSignupKind SignupKind { get; init; }
    public int TrialDays { get; init; }
    public decimal MonthlyPrice { get; init; }
    public string Currency { get; init; } = "USD";
    public bool RequiresTermsAcceptance { get; init; }
    public IReadOnlyList<string> IncludedFeatures { get; init; } = [];
    public Dictionary<string, int> Limits { get; init; } = [];
}

public sealed class SignupSessionEnvelopeModel
{
    public SignupSessionModel Session { get; init; } = new();
    public string? AccessToken { get; init; }
    public string? DebugVerificationToken { get; init; }
}

public sealed class SignupSessionModel
{
    public Guid Id { get; init; }
    public SignupSessionStatus Status { get; init; }
    public string CurrentStep { get; init; } = string.Empty;
    public string? Email { get; init; }
    public bool EmailVerified { get; init; }
    public DateTime? EmailVerifiedAt { get; init; }
    public DateTime? VerificationSentAt { get; init; }
    public DateTime? VerificationExpiresAt { get; init; }
    public Guid? SelectedPlanId { get; init; }
    public string? SelectedPlanCode { get; init; }
    public SignupBillingStatus BillingStatus { get; init; }
    public Guid? BillingIntentId { get; init; }
    public Guid? TenantId { get; init; }
    public bool TermsAccepted { get; init; }
    public string? OrganizationName { get; init; }
    public string? OrganizationLegalName { get; init; }
    public string? TenantSlug { get; init; }
    public string? BillingEmail { get; init; }
    public string? DefaultCurrency { get; init; }
    public string? TimeZone { get; init; }
    public string? AdminEmail { get; init; }
    public string? AdminFirstName { get; init; }
    public string? AdminLastName { get; init; }
    public string? AdminUsername { get; init; }
    public string? TemporaryPassword { get; init; }
    public string? LaunchPath { get; init; }
    public string? LastError { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class SignupSessionSummaryModel
{
    public Guid Id { get; init; }
    public SignupSessionStatus Status { get; init; }
    public string CurrentStep { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? OrganizationName { get; init; }
    public string? TenantSlug { get; init; }
    public string? SelectedPlanCode { get; init; }
    public SignupBillingStatus BillingStatus { get; init; }
    public Guid? TenantId { get; init; }
    public string? LastError { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class SignupEmailUpdateModel
{
    public string Email { get; init; } = string.Empty;
}

public sealed class SignupVerifyEmailModel
{
    public string Token { get; init; } = string.Empty;
}

public sealed class SignupOrganizationModel
{
    public string OrganizationName { get; init; } = string.Empty;
    public string? OrganizationLegalName { get; init; }
    public string TenantSlug { get; init; } = string.Empty;
    public string BillingEmail { get; init; } = string.Empty;
    public string DefaultCurrency { get; init; } = "USD";
    public string TimeZone { get; init; } = "UTC";
}

public sealed class SignupPlanSelectionModel
{
    public string PlanCode { get; init; } = string.Empty;
}

public sealed class SignupTermsAcceptanceModel
{
    public bool Accepted { get; init; }
}

public sealed class SignupAdminSetupModel
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
}

public sealed class SignupBillingGatewayRequestModel
{
    public Guid SessionId { get; init; }
    public Guid PlanId { get; init; }
    public string PlanCode { get; init; } = string.Empty;
    public string PlanName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Email { get; init; } = string.Empty;
    public string OrganizationName { get; init; } = string.Empty;
}

public sealed class SignupBillingGatewayResultModel
{
    public SignupBillingStatus Status { get; init; }
    public string ProviderName { get; init; } = string.Empty;
    public string? ExternalReference { get; init; }
    public string? CheckoutUrl { get; init; }
    public string? MetadataJson { get; init; }
}
