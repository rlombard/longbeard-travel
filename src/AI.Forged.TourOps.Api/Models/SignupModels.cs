using System.Text.Json.Serialization;
using AI.Forged.TourOps.Application.Models.Platform;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Api.Models;

public sealed class SignupBootstrapResponse
{
    public bool Enabled { get; init; }
    public string DisabledReason { get; init; } = string.Empty;
    public bool AllowTestPaymentConfirmation { get; init; }
    public string SupportEmail { get; init; } = string.Empty;
}

public sealed class SignupPlanResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LicenseSignupKind SignupKind { get; init; }
    public int TrialDays { get; init; }
    public decimal MonthlyPrice { get; init; }
    public string Currency { get; init; } = "USD";
    public bool RequiresTermsAcceptance { get; init; }
    public IReadOnlyList<string> IncludedFeatures { get; init; } = [];
    public Dictionary<string, int> Limits { get; init; } = [];
}

public sealed class SignupSessionEnvelopeResponse
{
    public SignupSessionResponse Session { get; init; } = new();
    public string? AccessToken { get; init; }
    public string? DebugVerificationToken { get; init; }
}

public sealed class SignupSessionResponse
{
    public Guid Id { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SignupSessionStatus Status { get; init; }
    public string CurrentStep { get; init; } = string.Empty;
    public string? Email { get; init; }
    public bool EmailVerified { get; init; }
    public DateTime? EmailVerifiedAt { get; init; }
    public DateTime? VerificationSentAt { get; init; }
    public DateTime? VerificationExpiresAt { get; init; }
    public Guid? SelectedPlanId { get; init; }
    public string? SelectedPlanCode { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
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

public sealed class SignupSessionSummaryResponse
{
    public Guid Id { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SignupSessionStatus Status { get; init; }
    public string CurrentStep { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? OrganizationName { get; init; }
    public string? TenantSlug { get; init; }
    public string? SelectedPlanCode { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SignupBillingStatus BillingStatus { get; init; }
    public Guid? TenantId { get; init; }
    public string? LastError { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class SignupEmailUpdateRequest
{
    public string Email { get; init; } = string.Empty;
}

public sealed class SignupVerifyEmailRequest
{
    public string Token { get; init; } = string.Empty;
}

public sealed class SignupOrganizationRequest
{
    public string OrganizationName { get; init; } = string.Empty;
    public string? OrganizationLegalName { get; init; }
    public string TenantSlug { get; init; } = string.Empty;
    public string BillingEmail { get; init; } = string.Empty;
    public string DefaultCurrency { get; init; } = "USD";
    public string TimeZone { get; init; } = "UTC";
}

public sealed class SignupPlanSelectionRequest
{
    public string PlanCode { get; init; } = string.Empty;
}

public sealed class SignupTermsAcceptanceRequest
{
    public bool Accepted { get; init; }
}

public sealed class SignupAdminSetupRequest
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
}

public static class SignupMappings
{
    public static SignupEmailUpdateModel ToModel(this SignupEmailUpdateRequest request) => new()
    {
        Email = request.Email
    };

    public static SignupVerifyEmailModel ToModel(this SignupVerifyEmailRequest request) => new()
    {
        Token = request.Token
    };

    public static SignupOrganizationModel ToModel(this SignupOrganizationRequest request) => new()
    {
        OrganizationName = request.OrganizationName,
        OrganizationLegalName = request.OrganizationLegalName,
        TenantSlug = request.TenantSlug,
        BillingEmail = request.BillingEmail,
        DefaultCurrency = request.DefaultCurrency,
        TimeZone = request.TimeZone
    };

    public static SignupPlanSelectionModel ToModel(this SignupPlanSelectionRequest request) => new()
    {
        PlanCode = request.PlanCode
    };

    public static SignupTermsAcceptanceModel ToModel(this SignupTermsAcceptanceRequest request) => new()
    {
        Accepted = request.Accepted
    };

    public static SignupAdminSetupModel ToModel(this SignupAdminSetupRequest request) => new()
    {
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName,
        Username = request.Username
    };

    public static SignupBootstrapResponse ToResponse(this SignupBootstrapModel model) => new()
    {
        Enabled = model.Enabled,
        DisabledReason = model.DisabledReason,
        AllowTestPaymentConfirmation = model.AllowTestPaymentConfirmation,
        SupportEmail = model.SupportEmail
    };

    public static SignupPlanResponse ToResponse(this SignupPlanModel model) => new()
    {
        Id = model.Id,
        Code = model.Code,
        Name = model.Name,
        Description = model.Description,
        SignupKind = model.SignupKind,
        TrialDays = model.TrialDays,
        MonthlyPrice = model.MonthlyPrice,
        Currency = model.Currency,
        RequiresTermsAcceptance = model.RequiresTermsAcceptance,
        IncludedFeatures = model.IncludedFeatures,
        Limits = model.Limits
    };

    public static SignupSessionEnvelopeResponse ToResponse(this SignupSessionEnvelopeModel model) => new()
    {
        Session = model.Session.ToResponse(),
        AccessToken = model.AccessToken,
        DebugVerificationToken = model.DebugVerificationToken
    };

    public static SignupSessionResponse ToResponse(this SignupSessionModel model) => new()
    {
        Id = model.Id,
        Status = model.Status,
        CurrentStep = model.CurrentStep,
        Email = model.Email,
        EmailVerified = model.EmailVerified,
        EmailVerifiedAt = model.EmailVerifiedAt,
        VerificationSentAt = model.VerificationSentAt,
        VerificationExpiresAt = model.VerificationExpiresAt,
        SelectedPlanId = model.SelectedPlanId,
        SelectedPlanCode = model.SelectedPlanCode,
        BillingStatus = model.BillingStatus,
        BillingIntentId = model.BillingIntentId,
        TenantId = model.TenantId,
        TermsAccepted = model.TermsAccepted,
        OrganizationName = model.OrganizationName,
        OrganizationLegalName = model.OrganizationLegalName,
        TenantSlug = model.TenantSlug,
        BillingEmail = model.BillingEmail,
        DefaultCurrency = model.DefaultCurrency,
        TimeZone = model.TimeZone,
        AdminEmail = model.AdminEmail,
        AdminFirstName = model.AdminFirstName,
        AdminLastName = model.AdminLastName,
        AdminUsername = model.AdminUsername,
        TemporaryPassword = model.TemporaryPassword,
        LaunchPath = model.LaunchPath,
        LastError = model.LastError,
        ExpiresAt = model.ExpiresAt,
        CreatedAt = model.CreatedAt,
        UpdatedAt = model.UpdatedAt
    };

    public static SignupSessionSummaryResponse ToResponse(this SignupSessionSummaryModel model) => new()
    {
        Id = model.Id,
        Status = model.Status,
        CurrentStep = model.CurrentStep,
        Email = model.Email,
        OrganizationName = model.OrganizationName,
        TenantSlug = model.TenantSlug,
        SelectedPlanCode = model.SelectedPlanCode,
        BillingStatus = model.BillingStatus,
        TenantId = model.TenantId,
        LastError = model.LastError,
        CreatedAt = model.CreatedAt,
        UpdatedAt = model.UpdatedAt
    };
}
