using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public sealed class TenantPlatformRepository(AppDbContext dbContext) : ITenantPlatformRepository
{
    public async Task<Tenant?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await dbContext.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken);

    public async Task<Tenant?> GetTenantBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await dbContext.Tenants.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public async Task<Tenant?> GetTenantByRealmNameAsync(string realmName, CancellationToken cancellationToken = default) =>
        await dbContext.TenantIdentityMappings
            .AsNoTracking()
            .Where(x => x.RealmName == realmName)
            .Select(x => x.Tenant)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Tenant?> GetTenantByIssuerAsync(string issuerUrl, CancellationToken cancellationToken = default) =>
        await dbContext.TenantIdentityMappings
            .AsNoTracking()
            .Where(x => x.IssuerUrl == issuerUrl)
            .Select(x => x.Tenant)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Tenant?> GetStandaloneTenantAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Tenants.FirstOrDefaultAsync(x => x.IsStandaloneTenant, cancellationToken);

    public async Task<IReadOnlyList<Tenant>> GetTenantsAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Tenants
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<Tenant> AddTenantAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task UpdateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        dbContext.Tenants.Update(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LicensePlan>> GetLicensePlansAsync(CancellationToken cancellationToken = default) =>
        await dbContext.LicensePlans.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LicensePlan>> GetPublicSignupPlansAsync(CancellationToken cancellationToken = default) =>
        await dbContext.LicensePlans
            .AsNoTracking()
            .Where(x => x.IsPublicSignupEnabled && !x.IsStandalonePlan)
            .OrderBy(x => x.SignupSortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<LicensePlan?> GetLicensePlanByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await dbContext.LicensePlans.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

    public async Task<TenantLicense?> GetLicenseAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await dbContext.TenantLicenses
            .Include(x => x.LicensePlan)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

    public async Task UpsertLicenseAsync(TenantLicense license, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.TenantLicenses.FirstOrDefaultAsync(x => x.TenantId == license.TenantId, cancellationToken);
        if (existing is null)
        {
            dbContext.TenantLicenses.Add(license);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(license);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TenantOnboardingState?> GetOnboardingAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await dbContext.TenantOnboardingStates.FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

    public async Task UpsertOnboardingAsync(TenantOnboardingState onboarding, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.TenantOnboardingStates.FirstOrDefaultAsync(x => x.TenantId == onboarding.TenantId, cancellationToken);
        if (existing is null)
        {
            dbContext.TenantOnboardingStates.Add(onboarding);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(onboarding);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TenantUserMembership>> GetMembershipsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await dbContext.TenantUserMemberships
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TenantUserMembership>> GetActiveMembershipsByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        await dbContext.TenantUserMemberships
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status == TenantUserStatus.Active)
            .ToListAsync(cancellationToken);

    public async Task<TenantUserMembership?> GetMembershipAsync(Guid tenantId, string userId, CancellationToken cancellationToken = default) =>
        await dbContext.TenantUserMemberships.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken);

    public async Task UpsertMembershipAsync(TenantUserMembership membership, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.TenantUserMemberships.FirstOrDefaultAsync(x => x.TenantId == membership.TenantId && x.UserId == membership.UserId, cancellationToken);
        if (existing is null)
        {
            dbContext.TenantUserMemberships.Add(membership);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(membership);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TenantConfigEntry>> GetConfigEntriesAsync(Guid tenantId, string? configDomain, CancellationToken cancellationToken = default)
    {
        var query = dbContext.TenantConfigEntries.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(configDomain))
        {
            query = query.Where(x => x.ConfigDomain == configDomain);
        }

        return await query
            .OrderBy(x => x.ConfigDomain)
            .ThenBy(x => x.ConfigKey)
            .ToListAsync(cancellationToken);
    }

    public async Task<TenantConfigEntry?> GetConfigEntryAsync(Guid tenantId, string configDomain, string configKey, CancellationToken cancellationToken = default) =>
        await dbContext.TenantConfigEntries.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ConfigDomain == configDomain && x.ConfigKey == configKey, cancellationToken);

    public async Task UpsertConfigEntryAsync(TenantConfigEntry entry, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.TenantConfigEntries.FirstOrDefaultAsync(x => x.TenantId == entry.TenantId && x.ConfigDomain == entry.ConfigDomain && x.ConfigKey == entry.ConfigKey, cancellationToken);
        if (existing is null)
        {
            dbContext.TenantConfigEntries.Add(entry);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(entry);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TenantIdentityMapping>> GetIdentityMappingsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await dbContext.TenantIdentityMappings
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task UpsertIdentityMappingAsync(TenantIdentityMapping mapping, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.TenantIdentityMappings.FirstOrDefaultAsync(x => x.TenantId == mapping.TenantId && x.RealmName == mapping.RealmName, cancellationToken);
        if (existing is null)
        {
            dbContext.TenantIdentityMappings.Add(mapping);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(mapping);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddUsageAsync(UsageRecord record, CancellationToken cancellationToken = default)
    {
        dbContext.UsageRecords.Add(record);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UsageRecord>> GetUsageAsync(Guid tenantId, DateTime fromUtc, CancellationToken cancellationToken = default) =>
        await dbContext.UsageRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.OccurredAt >= fromUtc)
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(cancellationToken);

    public async Task<int> CountUsageAsync(Guid tenantId, string metricKey, DateTime fromUtc, CancellationToken cancellationToken = default)
    {
        var quantity = await dbContext.UsageRecords
            .Where(x => x.TenantId == tenantId && x.MetricKey == metricKey && x.OccurredAt >= fromUtc)
            .SumAsync(x => (decimal?)x.Quantity, cancellationToken);

        return quantity.HasValue ? (int)Math.Ceiling(quantity.Value) : 0;
    }

    public async Task AddMonetizationTransactionAsync(MonetizationTransaction transaction, CancellationToken cancellationToken = default)
    {
        dbContext.MonetizationTransactions.Add(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MonetizationTransaction>> GetTransactionsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await dbContext.MonetizationTransactions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        dbContext.AuditEvents.Add(auditEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditEvent>> GetAuditEventsAsync(Guid? tenantId, int take, CancellationToken cancellationToken = default)
    {
        var query = dbContext.AuditEvents.AsNoTracking();
        if (tenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == tenantId);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync(cancellationToken);
    }

    public async Task<SignupSession?> GetSignupSessionAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        await dbContext.SignupSessions
            .Include(x => x.EmailVerification)
            .Include(x => x.BillingIntent)
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

    public async Task AddSignupSessionAsync(SignupSession session, CancellationToken cancellationToken = default)
    {
        dbContext.SignupSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSignupSessionAsync(SignupSession session, CancellationToken cancellationToken = default)
    {
        dbContext.SignupSessions.Update(session);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<SignupEmailVerification?> GetSignupEmailVerificationAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        await dbContext.SignupEmailVerifications.FirstOrDefaultAsync(x => x.SignupSessionId == sessionId, cancellationToken);

    public async Task UpsertSignupEmailVerificationAsync(SignupEmailVerification verification, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.SignupEmailVerifications.FirstOrDefaultAsync(x => x.SignupSessionId == verification.SignupSessionId, cancellationToken);
        if (existing is null)
        {
            dbContext.SignupEmailVerifications.Add(verification);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(verification);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<SignupBillingIntent?> GetSignupBillingIntentAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        await dbContext.SignupBillingIntents.Include(x => x.LicensePlan).FirstOrDefaultAsync(x => x.SignupSessionId == sessionId, cancellationToken);

    public async Task UpsertSignupBillingIntentAsync(SignupBillingIntent billingIntent, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.SignupBillingIntents.FirstOrDefaultAsync(x => x.SignupSessionId == billingIntent.SignupSessionId, cancellationToken);
        if (existing is null)
        {
            dbContext.SignupBillingIntents.Add(billingIntent);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(billingIntent);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SignupSession>> GetSignupSessionsAsync(int take, CancellationToken cancellationToken = default) =>
        await dbContext.SignupSessions
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync(cancellationToken);

    public async Task<bool> IsSignupEmailAlreadyUsedAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        var normalized = normalizedEmail.Trim().ToLowerInvariant();
        return await dbContext.Tenants.AnyAsync(x => x.BillingEmail != null && x.BillingEmail.ToLower() == normalized, cancellationToken)
            || await dbContext.TenantUserMemberships.AnyAsync(x => x.Email.ToLower() == normalized, cancellationToken);
    }

    public async Task<bool> IsTenantSlugAlreadyUsedAsync(string tenantSlug, CancellationToken cancellationToken = default) =>
        await dbContext.Tenants.AnyAsync(x => x.Slug == tenantSlug.Trim().ToLowerInvariant(), cancellationToken);

    public Task<int> CountTenantUsersAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        dbContext.TenantUserMemberships.CountAsync(x => x.TenantId == tenantId && x.Status == TenantUserStatus.Active, cancellationToken);

    public Task<int> CountTenantConnectionsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        dbContext.EmailProviderConnections.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantId && x.Status == EmailIntegrationStatus.Active, cancellationToken);

    public async Task EnsureSeedDataAsync(
        DeploymentMode deploymentMode,
        Guid standaloneTenantId,
        string standaloneTenantSlug,
        string standaloneTenantName,
        bool seedDemoTenantInSaas,
        Guid demoTenantId,
        string demoTenantSlug,
        string demoTenantName,
        CancellationToken cancellationToken = default)
    {
        await EnsureDefaultPlansAsync(cancellationToken);

        if (deploymentMode == DeploymentMode.Standalone && !await dbContext.Tenants.AnyAsync(x => x.IsStandaloneTenant, cancellationToken))
        {
            var tenant = new Tenant
            {
                Id = standaloneTenantId,
                Slug = standaloneTenantSlug,
                Name = standaloneTenantName,
                DefaultCurrency = "USD",
                TimeZone = "UTC",
                Status = TenantStatus.Active,
                IsStandaloneTenant = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.Tenants.Add(tenant);

            var plan = await dbContext.LicensePlans.FirstAsync(x => x.Code == "standalone-unlimited", cancellationToken);
            dbContext.TenantLicenses.Add(new TenantLicense
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                LicensePlanId = plan.Id,
                Status = LicenseStatus.Active,
                BillingMode = BillingMode.Standalone,
                StartsAt = DateTime.UtcNow,
                FeatureOverridesJson = "[]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            dbContext.TenantOnboardingStates.Add(new TenantOnboardingState
            {
                TenantId = tenant.Id,
                Status = OnboardingStatus.Completed,
                CurrentStep = "completed",
                CompletedStepsJson = "[\"organization\",\"identity\",\"email\",\"billing\",\"activation\"]",
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (deploymentMode == DeploymentMode.SaaS && seedDemoTenantInSaas)
        {
            await EnsureDemoSaasTenantAsync(demoTenantId, demoTenantSlug, demoTenantName, cancellationToken);
        }
    }

    private async Task EnsureDemoSaasTenantAsync(Guid demoTenantId, string demoTenantSlug, string demoTenantName, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var tenant = await dbContext.Tenants.FirstOrDefaultAsync(
            x => x.Id == demoTenantId || x.Slug == demoTenantSlug,
            cancellationToken);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Id = demoTenantId,
                Slug = demoTenantSlug,
                Name = demoTenantName,
                LegalName = $"{demoTenantName} Pty Ltd",
                BillingEmail = $"billing@{demoTenantSlug}.local",
                DefaultCurrency = "USD",
                TimeZone = "UTC",
                Status = TenantStatus.Active,
                IsStandaloneTenant = false,
                Notes = "Seeded SaaS demo tenant for dev testing.",
                CreatedAt = now,
                UpdatedAt = now
            };

            dbContext.Tenants.Add(tenant);
        }
        else
        {
            tenant.Slug = demoTenantSlug;
            tenant.Name = demoTenantName;
            tenant.LegalName = $"{demoTenantName} Pty Ltd";
            tenant.BillingEmail = $"billing@{demoTenantSlug}.local";
            tenant.DefaultCurrency = "USD";
            tenant.TimeZone = "UTC";
            tenant.Status = TenantStatus.Active;
            tenant.IsStandaloneTenant = false;
            tenant.Notes = "Seeded SaaS demo tenant for dev testing.";
            tenant.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var plan = await dbContext.LicensePlans.FirstAsync(x => x.Code == "saas-growth", cancellationToken);
        var license = await dbContext.TenantLicenses.FirstOrDefaultAsync(x => x.TenantId == tenant.Id, cancellationToken);
        if (license is null)
        {
            dbContext.TenantLicenses.Add(new TenantLicense
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                LicensePlanId = plan.Id,
                Status = LicenseStatus.Active,
                BillingMode = BillingMode.Invoice,
                StartsAt = now,
                BillingCustomerReference = $"seed-{demoTenantSlug}",
                SubscriptionReference = $"seed-{demoTenantSlug}-growth",
                FeatureOverridesJson = "[]",
                Notes = "Seeded SaaS demo tenant license.",
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            license.LicensePlanId = plan.Id;
            license.Status = LicenseStatus.Active;
            license.BillingMode = BillingMode.Invoice;
            license.StartsAt = now;
            license.BillingCustomerReference = $"seed-{demoTenantSlug}";
            license.SubscriptionReference = $"seed-{demoTenantSlug}-growth";
            license.FeatureOverridesJson = "[]";
            license.Notes = "Seeded SaaS demo tenant license.";
            license.UpdatedAt = now;
        }

        var onboarding = await dbContext.TenantOnboardingStates.FirstOrDefaultAsync(x => x.TenantId == tenant.Id, cancellationToken);
        if (onboarding is null)
        {
            dbContext.TenantOnboardingStates.Add(new TenantOnboardingState
            {
                TenantId = tenant.Id,
                Status = OnboardingStatus.Completed,
                CurrentStep = "completed",
                CompletedStepsJson = "[\"organization\",\"identity\",\"email\",\"billing\",\"activation\"]",
                OrganizationProfileJson = $$"""{"name":"{{demoTenantName}}","slug":"{{demoTenantSlug}}"}""",
                StartedAt = now,
                CompletedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            onboarding.Status = OnboardingStatus.Completed;
            onboarding.CurrentStep = "completed";
            onboarding.CompletedStepsJson = "[\"organization\",\"identity\",\"email\",\"billing\",\"activation\"]";
            onboarding.OrganizationProfileJson = $$"""{"name":"{{demoTenantName}}","slug":"{{demoTenantSlug}}"}""";
            onboarding.CompletedAt = now;
            onboarding.UpdatedAt = now;
        }

        await UpsertTenantConfigSeedAsync(tenant.Id, "branding", "displayName", demoTenantName, now, cancellationToken);
        await UpsertTenantConfigSeedAsync(tenant.Id, "branding", "supportEmail", $"support@{demoTenantSlug}.local", now, cancellationToken);
        await UpsertTenantConfigSeedAsync(tenant.Id, "branding", "primaryColor", "#0f766e", now, cancellationToken);
        await UpsertTenantConfigSeedAsync(tenant.Id, "branding", "accentColor", "#f59e0b", now, cancellationToken);
        await UpsertTenantConfigSeedAsync(tenant.Id, "email", "supportAddress", $"support@{demoTenantSlug}.local", now, cancellationToken);
        await UpsertTenantConfigSeedAsync(tenant.Id, "email", "defaultSenderName", $"{demoTenantName} Ops", now, cancellationToken);
        await UpsertTenantConfigSeedAsync(tenant.Id, "email", "replyToAddress", $"hello@{demoTenantSlug}.local", now, cancellationToken);
        await UpsertTenantConfigSeedAsync(tenant.Id, "email", "signatureHtml", $"<p>Kind regards,<br/>{demoTenantName} Ops</p>", now, cancellationToken);
        await UpsertTenantConfigSeedAsync(tenant.Id, "email", "sharedInboxEnabled", true, now, cancellationToken);
        await UpsertTenantConfigSeedAsync(tenant.Id, "workflow", "defaultApprovalMode", "human-review", now, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertTenantConfigSeedAsync(Guid tenantId, string configDomain, string configKey, object configValue, DateTime now, CancellationToken cancellationToken)
    {
        var entry = await dbContext.TenantConfigEntries.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.ConfigDomain == configDomain && x.ConfigKey == configKey,
            cancellationToken);

        if (entry is null)
        {
            dbContext.TenantConfigEntries.Add(new TenantConfigEntry
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ConfigDomain = configDomain,
                ConfigKey = configKey,
                JsonValue = JsonSerializer.Serialize(configValue),
                IsEncrypted = false,
                UpdatedByUserId = "platform-seed",
                CreatedAt = now,
                UpdatedAt = now
            });
            return;
        }

        entry.JsonValue = JsonSerializer.Serialize(configValue);
        entry.IsEncrypted = false;
        entry.UpdatedByUserId = "platform-seed";
        entry.UpdatedAt = now;
    }

    private async Task EnsureDefaultPlansAsync(CancellationToken cancellationToken)
    {
        await UpsertPlanAsync(new LicensePlan
        {
            Code = "standalone-unlimited",
            Name = "Standalone Unlimited",
            Description = "Standalone tenant license.",
            IsStandalonePlan = true,
            IsPublicSignupEnabled = false,
            SignupKind = LicenseSignupKind.Hidden,
            SignupSortOrder = 100,
            TrialDays = 0,
            MonthlyPrice = 0,
            Currency = "USD",
            RequiresTermsAcceptance = false,
            MaxUsers = 5000,
            MaxIntegrations = 500,
            MaxEmailAccounts = 500,
            MaxMonthlyAiJobs = 500000,
            MaxMonthlyEmailSends = 500000,
            MaxMonthlySyncOperations = 500000,
            MaxStorageMb = 102400,
            IncludedFeaturesJson = "[\"tenant.users.manage\",\"email.integrations.manage\",\"email.sync\",\"email.send\",\"ai.itinerary\",\"ai.email\",\"ai.task-suggestions\"]"
        }, cancellationToken);

        await UpsertPlanAsync(new LicensePlan
        {
            Code = "saas-free",
            Name = "Free",
            Description = "Starter free SaaS tier.",
            IsStandalonePlan = false,
            IsPublicSignupEnabled = true,
            SignupKind = LicenseSignupKind.Free,
            SignupSortOrder = 10,
            TrialDays = 0,
            MonthlyPrice = 0,
            Currency = "USD",
            RequiresTermsAcceptance = true,
            MaxUsers = 3,
            MaxIntegrations = 1,
            MaxEmailAccounts = 1,
            MaxMonthlyAiJobs = 50,
            MaxMonthlyEmailSends = 100,
            MaxMonthlySyncOperations = 200,
            MaxStorageMb = 1024,
            IncludedFeaturesJson = "[\"tenant.users.manage\",\"email.integrations.manage\",\"email.sync\",\"email.send\",\"ai.itinerary\"]"
        }, cancellationToken);

        await UpsertPlanAsync(new LicensePlan
        {
            Code = "saas-trial",
            Name = "Trial",
            Description = "Default SaaS trial plan.",
            IsStandalonePlan = false,
            IsPublicSignupEnabled = true,
            SignupKind = LicenseSignupKind.Trial,
            SignupSortOrder = 20,
            TrialDays = 14,
            MonthlyPrice = 0,
            Currency = "USD",
            RequiresTermsAcceptance = true,
            MaxUsers = 10,
            MaxIntegrations = 5,
            MaxEmailAccounts = 5,
            MaxMonthlyAiJobs = 500,
            MaxMonthlyEmailSends = 500,
            MaxMonthlySyncOperations = 1000,
            MaxStorageMb = 4096,
            IncludedFeaturesJson = "[\"tenant.users.manage\",\"email.integrations.manage\",\"email.sync\",\"email.send\",\"ai.itinerary\",\"ai.email\",\"ai.task-suggestions\"]"
        }, cancellationToken);

        await UpsertPlanAsync(new LicensePlan
        {
            Code = "saas-growth",
            Name = "Growth",
            Description = "Paid SaaS growth plan.",
            IsStandalonePlan = false,
            IsPublicSignupEnabled = true,
            SignupKind = LicenseSignupKind.Paid,
            SignupSortOrder = 30,
            TrialDays = 0,
            MonthlyPrice = 199,
            Currency = "USD",
            RequiresTermsAcceptance = true,
            MaxUsers = 50,
            MaxIntegrations = 20,
            MaxEmailAccounts = 20,
            MaxMonthlyAiJobs = 5000,
            MaxMonthlyEmailSends = 10000,
            MaxMonthlySyncOperations = 25000,
            MaxStorageMb = 20480,
            IncludedFeaturesJson = "[\"tenant.users.manage\",\"email.integrations.manage\",\"email.sync\",\"email.send\",\"ai.itinerary\",\"ai.email\",\"ai.task-suggestions\"]"
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertPlanAsync(LicensePlan desired, CancellationToken cancellationToken)
    {
        var existing = await dbContext.LicensePlans.FirstOrDefaultAsync(x => x.Code == desired.Code, cancellationToken);
        if (existing is null)
        {
            desired.Id = Guid.NewGuid();
            desired.CreatedAt = DateTime.UtcNow;
            desired.UpdatedAt = DateTime.UtcNow;
            dbContext.LicensePlans.Add(desired);
            return;
        }

        existing.Name = desired.Name;
        existing.Description = desired.Description;
        existing.IsStandalonePlan = desired.IsStandalonePlan;
        existing.IsPublicSignupEnabled = desired.IsPublicSignupEnabled;
        existing.SignupKind = desired.SignupKind;
        existing.SignupSortOrder = desired.SignupSortOrder;
        existing.TrialDays = desired.TrialDays;
        existing.MonthlyPrice = desired.MonthlyPrice;
        existing.Currency = desired.Currency;
        existing.RequiresTermsAcceptance = desired.RequiresTermsAcceptance;
        existing.MaxUsers = desired.MaxUsers;
        existing.MaxIntegrations = desired.MaxIntegrations;
        existing.MaxEmailAccounts = desired.MaxEmailAccounts;
        existing.MaxMonthlyAiJobs = desired.MaxMonthlyAiJobs;
        existing.MaxMonthlyEmailSends = desired.MaxMonthlyEmailSends;
        existing.MaxMonthlySyncOperations = desired.MaxMonthlySyncOperations;
        existing.MaxStorageMb = desired.MaxStorageMb;
        existing.IncludedFeaturesJson = desired.IncludedFeaturesJson;
        existing.UpdatedAt = DateTime.UtcNow;
    }
}
