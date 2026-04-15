using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface ITenantPlatformRepository
{
    Task<Tenant?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Tenant?> GetTenantBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Tenant?> GetTenantByRealmNameAsync(string realmName, CancellationToken cancellationToken = default);
    Task<Tenant?> GetTenantByIssuerAsync(string issuerUrl, CancellationToken cancellationToken = default);
    Task<Tenant?> GetStandaloneTenantAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tenant>> GetTenantsAsync(CancellationToken cancellationToken = default);
    Task<Tenant> AddTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task UpdateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LicensePlan>> GetLicensePlansAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LicensePlan>> GetPublicSignupPlansAsync(CancellationToken cancellationToken = default);
    Task<LicensePlan?> GetLicensePlanByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<TenantLicense?> GetLicenseAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task UpsertLicenseAsync(TenantLicense license, CancellationToken cancellationToken = default);

    Task<TenantOnboardingState?> GetOnboardingAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task UpsertOnboardingAsync(TenantOnboardingState onboarding, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantUserMembership>> GetMembershipsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantUserMembership>> GetActiveMembershipsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<TenantUserMembership?> GetMembershipAsync(Guid tenantId, string userId, CancellationToken cancellationToken = default);
    Task UpsertMembershipAsync(TenantUserMembership membership, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantConfigEntry>> GetConfigEntriesAsync(Guid tenantId, string? configDomain, CancellationToken cancellationToken = default);
    Task<TenantConfigEntry?> GetConfigEntryAsync(Guid tenantId, string configDomain, string configKey, CancellationToken cancellationToken = default);
    Task UpsertConfigEntryAsync(TenantConfigEntry entry, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantIdentityMapping>> GetIdentityMappingsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task UpsertIdentityMappingAsync(TenantIdentityMapping mapping, CancellationToken cancellationToken = default);

    Task AddUsageAsync(UsageRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsageRecord>> GetUsageAsync(Guid tenantId, DateTime fromUtc, CancellationToken cancellationToken = default);
    Task<int> CountUsageAsync(Guid tenantId, string metricKey, DateTime fromUtc, CancellationToken cancellationToken = default);

    Task AddMonetizationTransactionAsync(MonetizationTransaction transaction, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonetizationTransaction>> GetTransactionsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task AddAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEvent>> GetAuditEventsAsync(Guid? tenantId, int take, CancellationToken cancellationToken = default);

    Task<SignupSession?> GetSignupSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task AddSignupSessionAsync(SignupSession session, CancellationToken cancellationToken = default);
    Task UpdateSignupSessionAsync(SignupSession session, CancellationToken cancellationToken = default);
    Task<SignupEmailVerification?> GetSignupEmailVerificationAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task UpsertSignupEmailVerificationAsync(SignupEmailVerification verification, CancellationToken cancellationToken = default);
    Task<SignupBillingIntent?> GetSignupBillingIntentAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task UpsertSignupBillingIntentAsync(SignupBillingIntent billingIntent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SignupSession>> GetSignupSessionsAsync(int take, CancellationToken cancellationToken = default);
    Task<bool> IsSignupEmailAlreadyUsedAsync(string normalizedEmail, CancellationToken cancellationToken = default);
    Task<bool> IsTenantSlugAlreadyUsedAsync(string tenantSlug, CancellationToken cancellationToken = default);

    Task<int> CountTenantUsersAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<int> CountTenantConnectionsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task EnsureSeedDataAsync(
        DeploymentMode deploymentMode,
        Guid standaloneTenantId,
        string standaloneTenantSlug,
        string standaloneTenantName,
        bool seedDemoTenantInSaas,
        Guid demoTenantId,
        string demoTenantSlug,
        string demoTenantName,
        CancellationToken cancellationToken = default);
}
