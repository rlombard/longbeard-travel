using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Models.Platform;

public sealed class TenantSummaryModel
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? BillingEmail { get; init; }
    public string DefaultCurrency { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public TenantStatus Status { get; init; }
    public bool IsStandaloneTenant { get; init; }
    public string? LicensePlanCode { get; init; }
    public LicenseStatus? LicenseStatus { get; init; }
    public OnboardingStatus OnboardingStatus { get; init; }
    public string CurrentOnboardingStep { get; init; } = string.Empty;
    public int ActiveUsers { get; init; }
    public int ConnectedEmailAccounts { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class TenantDetailModel
{
    public TenantSummaryModel Tenant { get; init; } = new();
    public TenantLicenseModel? License { get; init; }
    public TenantOnboardingModel? Onboarding { get; init; }
    public IReadOnlyList<TenantUserMembershipModel> Users { get; init; } = [];
    public IReadOnlyList<TenantConfigEntryModel> ConfigEntries { get; init; } = [];
    public IReadOnlyList<TenantIdentityModel> IdentityMappings { get; init; } = [];
    public IReadOnlyList<UsageMetricSummaryModel> Usage { get; init; } = [];
    public IReadOnlyList<MonetizationTransactionModel> Transactions { get; init; } = [];
    public IReadOnlyList<AuditEventModel> AuditEvents { get; init; } = [];
}

public sealed class TenantLicenseModel
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid LicensePlanId { get; init; }
    public string PlanCode { get; init; } = string.Empty;
    public string PlanName { get; init; } = string.Empty;
    public LicenseStatus Status { get; init; }
    public BillingMode BillingMode { get; init; }
    public DateTime StartsAt { get; init; }
    public DateTime? TrialEndsAt { get; init; }
    public DateTime? EndsAt { get; init; }
    public IReadOnlyList<string> IncludedFeatures { get; init; } = [];
    public IReadOnlyDictionary<string, int> Limits { get; init; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> CurrentUsage { get; init; } = new Dictionary<string, int>();
}

public sealed class TenantOnboardingModel
{
    public Guid TenantId { get; init; }
    public OnboardingStatus Status { get; init; }
    public string CurrentStep { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSteps { get; init; } = [];
    public string? LastError { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class TenantUserMembershipModel
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public TenantUserRole Role { get; init; }
    public TenantUserStatus Status { get; init; }
    public DateTime InvitedAt { get; init; }
    public DateTime? JoinedAt { get; init; }
    public DateTime? LastSeenAt { get; init; }
}

public sealed class TenantConfigEntryModel
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string ConfigDomain { get; init; } = string.Empty;
    public string ConfigKey { get; init; } = string.Empty;
    public string JsonValue { get; init; } = string.Empty;
    public bool IsEncrypted { get; init; }
    public string UpdatedByUserId { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}

public sealed class TenantIdentityModel
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public IdentityIsolationMode IsolationMode { get; init; }
    public IdentityProvisioningStatus ProvisioningStatus { get; init; }
    public string RealmName { get; init; } = string.Empty;
    public string? ClientId { get; init; }
    public string? IssuerUrl { get; init; }
    public string? LastError { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class UsageMetricSummaryModel
{
    public string MetricKey { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public string Unit { get; init; } = string.Empty;
    public bool IsBillable { get; init; }
}

public sealed class MonetizationTransactionModel
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public MonetizationTransactionType TransactionType { get; init; }
    public MonetizationTransactionStatus Status { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public string? ExternalReference { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class AuditEventModel
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public string ScopeType { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
    public string? ActorUserId { get; init; }
    public string? ActorDisplayName { get; init; }
    public string? TargetEntityType { get; init; }
    public Guid? TargetEntityId { get; init; }
    public string? MetadataJson { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class CreateTenantModel
{
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? LegalName { get; init; }
    public string? BillingEmail { get; init; }
    public string DefaultCurrency { get; init; } = "USD";
    public string TimeZone { get; init; } = "UTC";
    public string LicensePlanCode { get; init; } = string.Empty;
    public bool IsStandaloneTenant { get; init; }
    public BootstrapTenantAdminModel? BootstrapAdmin { get; init; }
}

public sealed class BootstrapTenantAdminModel
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? TemporaryPassword { get; init; }
}

public sealed class UpdateTenantOnboardingModel
{
    public string Step { get; init; } = string.Empty;
    public bool MarkCompleted { get; init; }
    public bool CompleteOnboarding { get; init; }
    public string? PayloadJson { get; init; }
    public string? Error { get; init; }
}

public sealed class UpsertTenantConfigModel
{
    public string ConfigDomain { get; init; } = string.Empty;
    public string ConfigKey { get; init; } = string.Empty;
    public string JsonValue { get; init; } = string.Empty;
    public bool IsEncrypted { get; init; }
}

public sealed class AssignTenantUserModel
{
    public Guid TenantId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public TenantUserRole Role { get; init; }
}

public sealed class FeatureAccessResultModel
{
    public Guid TenantId { get; init; }
    public string FeatureKey { get; init; } = string.Empty;
    public bool Allowed { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? PlanCode { get; init; }
    public LicenseStatus? LicenseStatus { get; init; }
    public IReadOnlyDictionary<string, int> Limits { get; init; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> CurrentUsage { get; init; } = new Dictionary<string, int>();
}

public sealed class MeterUsageModel
{
    public string Category { get; init; } = string.Empty;
    public string MetricKey { get; init; } = string.Empty;
    public decimal Quantity { get; init; } = 1;
    public string Unit { get; init; } = "count";
    public bool IsBillable { get; init; } = true;
    public string? Source { get; init; }
    public string? ReferenceEntityType { get; init; }
    public Guid? ReferenceEntityId { get; init; }
    public string? MetadataJson { get; init; }
    public DateTime? OccurredAt { get; init; }
}

public sealed class SessionBootstrapModel
{
    public DeploymentMode DeploymentMode { get; init; }
    public bool PlatformManagementEnabled { get; init; }
    public bool PublicSignupEnabled { get; init; }
    public string PublicSignupDisabledReason { get; init; } = string.Empty;
    public AuthTargetModel ManagementAuth { get; init; } = new();
    public AuthTargetModel? StandaloneTenantAuth { get; init; }
    public SessionActorModel? Session { get; init; }
}

public sealed class AuthTargetModel
{
    public string KeycloakUrl { get; init; } = string.Empty;
    public string Realm { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
}

public sealed class SessionActorModel
{
    public bool IsAuthenticated { get; init; }
    public bool IsPlatformAdmin { get; init; }
    public string? UserId { get; init; }
    public string? DisplayName { get; init; }
    public string? Email { get; init; }
    public Guid? CurrentTenantId { get; init; }
    public string? CurrentTenantSlug { get; init; }
    public string? CurrentTenantName { get; init; }
    public string HomeArea { get; init; } = "/app";
    public IReadOnlyList<SessionTenantMembershipModel> Memberships { get; init; } = [];
}

public sealed class SessionTenantMembershipModel
{
    public Guid TenantId { get; init; }
    public string TenantSlug { get; init; } = string.Empty;
    public string TenantName { get; init; } = string.Empty;
    public TenantUserRole Role { get; init; }
    public string RealmName { get; init; } = string.Empty;
}

public sealed class TenantLoginDiscoveryModel
{
    public bool Found { get; init; }
    public Guid? TenantId { get; init; }
    public string? TenantSlug { get; init; }
    public string? TenantName { get; init; }
    public string ResolutionSource { get; init; } = string.Empty;
    public AuthTargetModel? Auth { get; init; }
}

public sealed class DiscoverTenantLoginModel
{
    public string? Email { get; init; }
    public string? TenantSlug { get; init; }
}

public sealed class AuditEventCreateModel
{
    public Guid? TenantId { get; init; }
    public string ScopeType { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
    public string? TargetEntityType { get; init; }
    public Guid? TargetEntityId { get; init; }
    public string? MetadataJson { get; init; }
    public string? IpAddress { get; init; }
}
