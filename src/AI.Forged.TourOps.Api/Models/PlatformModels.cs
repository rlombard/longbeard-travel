using AI.Forged.TourOps.Application.Models.Platform;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Api.Models;

public sealed class TenantSummaryResponse
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

public sealed class TenantDetailResponse
{
    public TenantSummaryResponse Tenant { get; init; } = new();
    public TenantLicenseResponse? License { get; init; }
    public TenantOnboardingResponse? Onboarding { get; init; }
    public IReadOnlyList<TenantUserMembershipResponse> Users { get; init; } = [];
    public IReadOnlyList<TenantConfigEntryResponse> ConfigEntries { get; init; } = [];
    public IReadOnlyList<TenantIdentityResponse> IdentityMappings { get; init; } = [];
    public IReadOnlyList<UsageMetricSummaryResponse> Usage { get; init; } = [];
    public IReadOnlyList<MonetizationTransactionResponse> Transactions { get; init; } = [];
    public IReadOnlyList<AuditEventResponse> AuditEvents { get; init; } = [];
}

public sealed class TenantLicenseResponse
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

public sealed class TenantOnboardingResponse
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

public sealed class TenantUserMembershipResponse
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

public sealed class TenantConfigEntryResponse
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

public sealed class TenantIdentityResponse
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

public sealed class UsageMetricSummaryResponse
{
    public string MetricKey { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public string Unit { get; init; } = string.Empty;
    public bool IsBillable { get; init; }
}

public sealed class MonetizationTransactionResponse
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

public sealed class AuditEventResponse
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

public sealed class CreateTenantRequest
{
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? LegalName { get; init; }
    public string? BillingEmail { get; init; }
    public string DefaultCurrency { get; init; } = "USD";
    public string TimeZone { get; init; } = "UTC";
    public string LicensePlanCode { get; init; } = string.Empty;
    public bool IsStandaloneTenant { get; init; }
    public BootstrapTenantAdminRequest? BootstrapAdmin { get; init; }
}

public sealed class BootstrapTenantAdminRequest
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? TemporaryPassword { get; init; }
}

public sealed class UpdateTenantOnboardingRequest
{
    public string Step { get; init; } = string.Empty;
    public bool MarkCompleted { get; init; }
    public bool CompleteOnboarding { get; init; }
    public string? PayloadJson { get; init; }
    public string? Error { get; init; }
}

public sealed class UpsertTenantConfigRequest
{
    public string ConfigDomain { get; init; } = string.Empty;
    public string ConfigKey { get; init; } = string.Empty;
    public string JsonValue { get; init; } = string.Empty;
    public bool IsEncrypted { get; init; }
}

public sealed class AssignTenantUserRequest
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public TenantUserRole Role { get; init; }
}

public static class PlatformMappings
{
    public static CreateTenantModel ToModel(this CreateTenantRequest request) => new()
    {
        Slug = request.Slug,
        Name = request.Name,
        LegalName = request.LegalName,
        BillingEmail = request.BillingEmail,
        DefaultCurrency = request.DefaultCurrency,
        TimeZone = request.TimeZone,
        LicensePlanCode = request.LicensePlanCode,
        IsStandaloneTenant = request.IsStandaloneTenant,
        BootstrapAdmin = request.BootstrapAdmin is null ? null : new BootstrapTenantAdminModel
        {
            Username = request.BootstrapAdmin.Username,
            Email = request.BootstrapAdmin.Email,
            FirstName = request.BootstrapAdmin.FirstName,
            LastName = request.BootstrapAdmin.LastName,
            TemporaryPassword = request.BootstrapAdmin.TemporaryPassword
        }
    };

    public static UpdateTenantOnboardingModel ToModel(this UpdateTenantOnboardingRequest request) => new()
    {
        Step = request.Step,
        MarkCompleted = request.MarkCompleted,
        CompleteOnboarding = request.CompleteOnboarding,
        PayloadJson = request.PayloadJson,
        Error = request.Error
    };

    public static UpsertTenantConfigModel ToModel(this UpsertTenantConfigRequest request) => new()
    {
        ConfigDomain = request.ConfigDomain,
        ConfigKey = request.ConfigKey,
        JsonValue = request.JsonValue,
        IsEncrypted = request.IsEncrypted
    };

    public static TenantSummaryResponse ToResponse(this TenantSummaryModel model) => new()
    {
        Id = model.Id,
        Slug = model.Slug,
        Name = model.Name,
        BillingEmail = model.BillingEmail,
        DefaultCurrency = model.DefaultCurrency,
        TimeZone = model.TimeZone,
        Status = model.Status,
        IsStandaloneTenant = model.IsStandaloneTenant,
        LicensePlanCode = model.LicensePlanCode,
        LicenseStatus = model.LicenseStatus,
        OnboardingStatus = model.OnboardingStatus,
        CurrentOnboardingStep = model.CurrentOnboardingStep,
        ActiveUsers = model.ActiveUsers,
        ConnectedEmailAccounts = model.ConnectedEmailAccounts,
        CreatedAt = model.CreatedAt,
        UpdatedAt = model.UpdatedAt
    };

    public static TenantDetailResponse ToResponse(this TenantDetailModel model) => new()
    {
        Tenant = model.Tenant.ToResponse(),
        License = model.License?.ToResponse(),
        Onboarding = model.Onboarding?.ToResponse(),
        Users = model.Users.Select(x => x.ToResponse()).ToList(),
        ConfigEntries = model.ConfigEntries.Select(x => x.ToResponse()).ToList(),
        IdentityMappings = model.IdentityMappings.Select(x => x.ToResponse()).ToList(),
        Usage = model.Usage.Select(x => x.ToResponse()).ToList(),
        Transactions = model.Transactions.Select(x => x.ToResponse()).ToList(),
        AuditEvents = model.AuditEvents.Select(x => x.ToResponse()).ToList()
    };

    public static TenantLicenseResponse ToResponse(this TenantLicenseModel model) => new()
    {
        Id = model.Id,
        TenantId = model.TenantId,
        LicensePlanId = model.LicensePlanId,
        PlanCode = model.PlanCode,
        PlanName = model.PlanName,
        Status = model.Status,
        BillingMode = model.BillingMode,
        StartsAt = model.StartsAt,
        TrialEndsAt = model.TrialEndsAt,
        EndsAt = model.EndsAt,
        IncludedFeatures = model.IncludedFeatures,
        Limits = model.Limits,
        CurrentUsage = model.CurrentUsage
    };

    public static TenantOnboardingResponse ToResponse(this TenantOnboardingModel model) => new()
    {
        TenantId = model.TenantId,
        Status = model.Status,
        CurrentStep = model.CurrentStep,
        CompletedSteps = model.CompletedSteps,
        LastError = model.LastError,
        StartedAt = model.StartedAt,
        CompletedAt = model.CompletedAt,
        UpdatedAt = model.UpdatedAt
    };

    public static TenantUserMembershipResponse ToResponse(this TenantUserMembershipModel model) => new()
    {
        Id = model.Id,
        TenantId = model.TenantId,
        UserId = model.UserId,
        Email = model.Email,
        DisplayName = model.DisplayName,
        Role = model.Role,
        Status = model.Status,
        InvitedAt = model.InvitedAt,
        JoinedAt = model.JoinedAt,
        LastSeenAt = model.LastSeenAt
    };

    public static TenantConfigEntryResponse ToResponse(this TenantConfigEntryModel model) => new()
    {
        Id = model.Id,
        TenantId = model.TenantId,
        ConfigDomain = model.ConfigDomain,
        ConfigKey = model.ConfigKey,
        JsonValue = model.JsonValue,
        IsEncrypted = model.IsEncrypted,
        UpdatedByUserId = model.UpdatedByUserId,
        UpdatedAt = model.UpdatedAt
    };

    public static TenantIdentityResponse ToResponse(this TenantIdentityModel model) => new()
    {
        Id = model.Id,
        TenantId = model.TenantId,
        IsolationMode = model.IsolationMode,
        ProvisioningStatus = model.ProvisioningStatus,
        RealmName = model.RealmName,
        ClientId = model.ClientId,
        IssuerUrl = model.IssuerUrl,
        LastError = model.LastError,
        UpdatedAt = model.UpdatedAt
    };

    public static UsageMetricSummaryResponse ToResponse(this UsageMetricSummaryModel model) => new()
    {
        MetricKey = model.MetricKey,
        Category = model.Category,
        Quantity = model.Quantity,
        Unit = model.Unit,
        IsBillable = model.IsBillable
    };

    public static MonetizationTransactionResponse ToResponse(this MonetizationTransactionModel model) => new()
    {
        Id = model.Id,
        TenantId = model.TenantId,
        TransactionType = model.TransactionType,
        Status = model.Status,
        Amount = model.Amount,
        Currency = model.Currency,
        PeriodStart = model.PeriodStart,
        PeriodEnd = model.PeriodEnd,
        ExternalReference = model.ExternalReference,
        CreatedAt = model.CreatedAt
    };

    public static AuditEventResponse ToResponse(this AuditEventModel model) => new()
    {
        Id = model.Id,
        TenantId = model.TenantId,
        ScopeType = model.ScopeType,
        Action = model.Action,
        Result = model.Result,
        ActorUserId = model.ActorUserId,
        ActorDisplayName = model.ActorDisplayName,
        TargetEntityType = model.TargetEntityType,
        TargetEntityId = model.TargetEntityId,
        MetadataJson = model.MetadataJson,
        CreatedAt = model.CreatedAt
    };
}
