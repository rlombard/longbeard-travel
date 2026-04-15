using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Models.AdminUsers;
using AI.Forged.TourOps.Application.Models.Platform;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services.Platform;

public sealed class TenantPlatformService(
    ITenantPlatformRepository tenantPlatformRepository,
    ITenantConfigurationService tenantConfigurationService,
    IKeycloakProvisioningService keycloakProvisioningService,
    IAuditService auditService,
    IKeycloakRealmAdminRepository keycloakRealmAdminRepository) : ITenantPlatformService
{
    public async Task<IReadOnlyList<TenantSummaryModel>> GetTenantsAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await tenantPlatformRepository.GetTenantsAsync(cancellationToken);
        var results = new List<TenantSummaryModel>(tenants.Count);

        foreach (var tenant in tenants.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            results.Add(await BuildSummaryAsync(tenant, cancellationToken));
        }

        return results;
    }

    public async Task<TenantDetailModel?> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await tenantPlatformRepository.GetTenantByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            return null;
        }

        var summary = await BuildSummaryAsync(tenant, cancellationToken);
        var license = await BuildLicenseAsync(tenantId, cancellationToken);
        var onboarding = await tenantPlatformRepository.GetOnboardingAsync(tenantId, cancellationToken);
        var users = await tenantPlatformRepository.GetMembershipsAsync(tenantId, cancellationToken);
        var configs = await tenantConfigurationService.GetAsync(tenantId, null, cancellationToken);
        var identities = await tenantPlatformRepository.GetIdentityMappingsAsync(tenantId, cancellationToken);
        var usage = await tenantPlatformRepository.GetUsageAsync(tenantId, DateTime.UtcNow.AddDays(-30), cancellationToken);
        var transactions = await tenantPlatformRepository.GetTransactionsAsync(tenantId, cancellationToken);
        var auditEvents = await tenantPlatformRepository.GetAuditEventsAsync(tenantId, 100, cancellationToken);

        return new TenantDetailModel
        {
            Tenant = summary,
            License = license,
            Onboarding = onboarding is null ? null : MapOnboarding(onboarding),
            Users = users
                .OrderByDescending(x => x.Role)
                .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Select(MapUser)
                .ToList(),
            ConfigEntries = configs,
            IdentityMappings = identities.Select(MapIdentity).ToList(),
            Usage = usage
                .GroupBy(x => new { x.MetricKey, x.Category, x.Unit, x.IsBillable })
                .Select(group => new UsageMetricSummaryModel
                {
                    MetricKey = group.Key.MetricKey,
                    Category = group.Key.Category,
                    Quantity = group.Sum(x => x.Quantity),
                    Unit = group.Key.Unit,
                    IsBillable = group.Key.IsBillable
                })
                .OrderBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.MetricKey, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Transactions = transactions
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new MonetizationTransactionModel
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    TransactionType = x.TransactionType,
                    Status = x.Status,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    PeriodStart = x.PeriodStart,
                    PeriodEnd = x.PeriodEnd,
                    ExternalReference = x.ExternalReference,
                    CreatedAt = x.CreatedAt
                })
                .ToList(),
            AuditEvents = auditEvents.Select(MapAudit).ToList()
        };
    }

    public async Task<TenantDetailModel> CreateTenantAsync(CreateTenantModel model, CancellationToken cancellationToken = default)
    {
        ValidateCreateModel(model);

        if (await tenantPlatformRepository.GetTenantBySlugAsync(model.Slug.Trim(), cancellationToken) is not null)
        {
            throw new InvalidOperationException("Tenant slug already exists.");
        }

        var plan = await tenantPlatformRepository.GetLicensePlanByCodeAsync(model.LicensePlanCode.Trim(), cancellationToken)
            ?? throw new InvalidOperationException("License plan was not found.");

        var now = DateTime.UtcNow;
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Slug = model.Slug.Trim().ToLowerInvariant(),
            Name = model.Name.Trim(),
            LegalName = NormalizeOptional(model.LegalName, 256),
            BillingEmail = NormalizeOptional(model.BillingEmail, 256),
            DefaultCurrency = NormalizeRequired(model.DefaultCurrency, "Default currency is required.", 8).ToUpperInvariant(),
            TimeZone = NormalizeRequired(model.TimeZone, "Time zone is required.", 128),
            Status = TenantStatus.Provisioning,
            IsStandaloneTenant = model.IsStandaloneTenant,
            CreatedAt = now,
            UpdatedAt = now
        };

        await tenantPlatformRepository.AddTenantAsync(tenant, cancellationToken);
        await tenantPlatformRepository.UpsertLicenseAsync(new TenantLicense
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            LicensePlanId = plan.Id,
            LicensePlan = plan,
            Status = plan.IsStandalonePlan ? LicenseStatus.Active : LicenseStatus.Trial,
            BillingMode = plan.IsStandalonePlan ? BillingMode.Standalone : BillingMode.Trial,
            StartsAt = now,
            TrialEndsAt = plan.IsStandalonePlan ? null : now.AddDays(14),
            FeatureOverridesJson = "[]",
            CreatedAt = now,
            UpdatedAt = now
        }, cancellationToken);
        await tenantPlatformRepository.UpsertOnboardingAsync(new TenantOnboardingState
        {
            TenantId = tenant.Id,
            Status = OnboardingStatus.InProgress,
            CurrentStep = "organization",
            CompletedStepsJson = "[]",
            StartedAt = now,
            UpdatedAt = now
        }, cancellationToken);

        var identity = await keycloakProvisioningService.EnsureTenantIdentityAsync(tenant.Id, cancellationToken);
        if (identity.ProvisioningStatus != IdentityProvisioningStatus.Ready)
        {
            throw new InvalidOperationException(identity.LastError ?? "Tenant realm provisioning failed.");
        }

        if (model.BootstrapAdmin is not null)
        {
            await CreateBootstrapAdminAsync(tenant, identity, model.BootstrapAdmin, now, cancellationToken);
        }

        tenant.Status = TenantStatus.Active;
        tenant.UpdatedAt = DateTime.UtcNow;
        await tenantPlatformRepository.UpdateTenantAsync(tenant, cancellationToken);

        await auditService.WriteAsync(new AuditEventCreateModel
        {
            TenantId = tenant.Id,
            ScopeType = "Tenant",
            Action = "TenantCreated",
            Result = "Success",
            TargetEntityType = nameof(Tenant),
            TargetEntityId = tenant.Id,
            MetadataJson = JsonSerializer.Serialize(new
            {
                tenant.Slug,
                plan.Code,
                tenant.IsStandaloneTenant
            })
        }, cancellationToken);

        return await GetTenantAsync(tenant.Id, cancellationToken)
            ?? throw new InvalidOperationException("Created tenant could not be loaded.");
    }

    public async Task<TenantOnboardingModel> UpdateOnboardingAsync(Guid tenantId, UpdateTenantOnboardingModel model, CancellationToken cancellationToken = default)
    {
        var tenant = await tenantPlatformRepository.GetTenantByIdAsync(tenantId, cancellationToken)
            ?? throw new InvalidOperationException("Tenant not found.");
        var onboarding = await tenantPlatformRepository.GetOnboardingAsync(tenantId, cancellationToken)
            ?? new TenantOnboardingState
            {
                TenantId = tenantId,
                Status = OnboardingStatus.NotStarted,
                CurrentStep = "organization",
                StartedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        var completedSteps = ParseStringList(onboarding.CompletedStepsJson).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (model.MarkCompleted && !string.IsNullOrWhiteSpace(model.Step))
        {
            completedSteps.Add(model.Step.Trim());
        }

        onboarding.CurrentStep = string.IsNullOrWhiteSpace(model.Step) ? onboarding.CurrentStep : model.Step.Trim();
        onboarding.CompletedStepsJson = JsonSerializer.Serialize(completedSteps.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        onboarding.LastError = NormalizeOptional(model.Error, 2000);
        onboarding.Status = model.CompleteOnboarding
            ? OnboardingStatus.Completed
            : string.IsNullOrWhiteSpace(onboarding.LastError) ? OnboardingStatus.InProgress : OnboardingStatus.Blocked;
        onboarding.CompletedAt = model.CompleteOnboarding ? DateTime.UtcNow : onboarding.CompletedAt;
        onboarding.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(model.PayloadJson))
        {
            onboarding.OrganizationProfileJson = model.PayloadJson.Trim();
        }

        await tenantPlatformRepository.UpsertOnboardingAsync(onboarding, cancellationToken);
        tenant.UpdatedAt = DateTime.UtcNow;
        await tenantPlatformRepository.UpdateTenantAsync(tenant, cancellationToken);

        await auditService.WriteAsync(new AuditEventCreateModel
        {
            TenantId = tenantId,
            ScopeType = "Onboarding",
            Action = "OnboardingUpdated",
            Result = onboarding.Status.ToString(),
            TargetEntityType = nameof(TenantOnboardingState),
            TargetEntityId = tenantId,
            MetadataJson = JsonSerializer.Serialize(new
            {
                onboarding.CurrentStep,
                onboarding.Status
            })
        }, cancellationToken);

        return MapOnboarding(onboarding);
    }

    public Task<TenantConfigEntryModel> UpsertConfigAsync(Guid tenantId, UpsertTenantConfigModel model, CancellationToken cancellationToken = default) =>
        tenantConfigurationService.UpsertAsync(tenantId, model, cancellationToken);

    public async Task<TenantUserMembershipModel> AssignUserAsync(AssignTenantUserModel model, CancellationToken cancellationToken = default)
    {
        var existing = await tenantPlatformRepository.GetMembershipAsync(model.TenantId, model.UserId.Trim(), cancellationToken);
        var membership = existing ?? new TenantUserMembership
        {
            Id = Guid.NewGuid(),
            TenantId = model.TenantId,
            InvitedAt = DateTime.UtcNow
        };

        membership.UserId = model.UserId.Trim();
        membership.Email = model.Email.Trim();
        membership.DisplayName = model.DisplayName.Trim();
        membership.Role = model.Role;
        membership.Status = TenantUserStatus.Active;
        membership.JoinedAt ??= DateTime.UtcNow;
        membership.LastSeenAt = DateTime.UtcNow;

        await tenantPlatformRepository.UpsertMembershipAsync(membership, cancellationToken);
        await auditService.WriteAsync(new AuditEventCreateModel
        {
            TenantId = model.TenantId,
            ScopeType = "TenantUser",
            Action = existing is null ? "TenantUserAssigned" : "TenantUserUpdated",
            Result = "Success",
            TargetEntityType = nameof(TenantUserMembership),
            TargetEntityId = membership.Id,
            MetadataJson = JsonSerializer.Serialize(new
            {
                membership.UserId,
                membership.Role
            })
        }, cancellationToken);

        return MapUser(membership);
    }

    private async Task CreateBootstrapAdminAsync(Tenant tenant, TenantIdentityModel identity, BootstrapTenantAdminModel model, DateTime now, CancellationToken cancellationToken)
    {
        var userId = await keycloakRealmAdminRepository.CreateUserAsync(identity.RealmName, new KeycloakAdminCreateUserInput
        {
            Username = model.Username.Trim(),
            Email = model.Email.Trim(),
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Enabled = true,
            EmailVerified = false,
            TemporaryPassword = string.IsNullOrWhiteSpace(model.TemporaryPassword) ? "ChangeMe123!" : model.TemporaryPassword.Trim()
        }, ["tenant-admin"], cancellationToken);

        await tenantPlatformRepository.UpsertMembershipAsync(new TenantUserMembership
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            UserId = userId,
            Email = model.Email.Trim(),
            DisplayName = $"{model.FirstName.Trim()} {model.LastName.Trim()}".Trim(),
            Role = TenantUserRole.TenantAdmin,
            Status = TenantUserStatus.Active,
            InvitedAt = now,
            JoinedAt = now,
            LastSeenAt = now
        }, cancellationToken);
    }

    private async Task<TenantSummaryModel> BuildSummaryAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        var license = await tenantPlatformRepository.GetLicenseAsync(tenant.Id, cancellationToken);
        var onboarding = await tenantPlatformRepository.GetOnboardingAsync(tenant.Id, cancellationToken);

        return new TenantSummaryModel
        {
            Id = tenant.Id,
            Slug = tenant.Slug,
            Name = tenant.Name,
            BillingEmail = tenant.BillingEmail,
            DefaultCurrency = tenant.DefaultCurrency,
            TimeZone = tenant.TimeZone,
            Status = tenant.Status,
            IsStandaloneTenant = tenant.IsStandaloneTenant,
            LicensePlanCode = license?.LicensePlan.Code,
            LicenseStatus = license?.Status,
            OnboardingStatus = onboarding?.Status ?? OnboardingStatus.NotStarted,
            CurrentOnboardingStep = onboarding?.CurrentStep ?? "organization",
            ActiveUsers = await tenantPlatformRepository.CountTenantUsersAsync(tenant.Id, cancellationToken),
            ConnectedEmailAccounts = await tenantPlatformRepository.CountTenantConnectionsAsync(tenant.Id, cancellationToken),
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
    }

    private async Task<TenantLicenseModel?> BuildLicenseAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var license = await tenantPlatformRepository.GetLicenseAsync(tenantId, cancellationToken);
        if (license is null)
        {
            return null;
        }

        var limits = new Dictionary<string, int>
        {
            ["users.active"] = license.MaxUsersOverride ?? license.LicensePlan.MaxUsers,
            ["integrations.active"] = license.MaxIntegrationsOverride ?? license.LicensePlan.MaxIntegrations,
            ["email.accounts"] = license.MaxEmailAccountsOverride ?? license.LicensePlan.MaxEmailAccounts,
            ["ai.jobs.monthly"] = license.MaxMonthlyAiJobsOverride ?? license.LicensePlan.MaxMonthlyAiJobs,
            ["email.sends.monthly"] = license.MaxMonthlyEmailSendsOverride ?? license.LicensePlan.MaxMonthlyEmailSends,
            ["email.sync.monthly"] = license.MaxMonthlySyncOperationsOverride ?? license.LicensePlan.MaxMonthlySyncOperations,
            ["storage.mb"] = license.MaxStorageMbOverride ?? license.LicensePlan.MaxStorageMb
        };

        return new TenantLicenseModel
        {
            Id = license.Id,
            TenantId = tenantId,
            LicensePlanId = license.LicensePlanId,
            PlanCode = license.LicensePlan.Code,
            PlanName = license.LicensePlan.Name,
            Status = license.Status,
            BillingMode = license.BillingMode,
            StartsAt = license.StartsAt,
            TrialEndsAt = license.TrialEndsAt,
            EndsAt = license.EndsAt,
            IncludedFeatures = ParseStringList(license.LicensePlan.IncludedFeaturesJson)
                .Union(ParseStringList(license.FeatureOverridesJson), StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Limits = limits,
            CurrentUsage = new Dictionary<string, int>
            {
                ["users.active"] = await tenantPlatformRepository.CountTenantUsersAsync(tenantId, cancellationToken),
                ["email.accounts"] = await tenantPlatformRepository.CountTenantConnectionsAsync(tenantId, cancellationToken),
                ["ai.jobs.monthly"] = await tenantPlatformRepository.CountUsageAsync(tenantId, "ai.jobs.monthly", StartOfMonth(), cancellationToken),
                ["email.sends.monthly"] = await tenantPlatformRepository.CountUsageAsync(tenantId, "email.sends.monthly", StartOfMonth(), cancellationToken),
                ["email.sync.monthly"] = await tenantPlatformRepository.CountUsageAsync(tenantId, "email.sync.monthly", StartOfMonth(), cancellationToken)
            }
        };
    }

    private static void ValidateCreateModel(CreateTenantModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Slug))
        {
            throw new InvalidOperationException("Tenant slug is required.");
        }

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            throw new InvalidOperationException("Tenant name is required.");
        }

        if (string.IsNullOrWhiteSpace(model.LicensePlanCode))
        {
            throw new InvalidOperationException("License plan code is required.");
        }
    }

    private static TenantOnboardingModel MapOnboarding(TenantOnboardingState onboarding) => new()
    {
        TenantId = onboarding.TenantId,
        Status = onboarding.Status,
        CurrentStep = onboarding.CurrentStep,
        CompletedSteps = ParseStringList(onboarding.CompletedStepsJson),
        LastError = onboarding.LastError,
        StartedAt = onboarding.StartedAt,
        CompletedAt = onboarding.CompletedAt,
        UpdatedAt = onboarding.UpdatedAt
    };

    private static TenantUserMembershipModel MapUser(TenantUserMembership membership) => new()
    {
        Id = membership.Id,
        TenantId = membership.TenantId,
        UserId = membership.UserId,
        Email = membership.Email,
        DisplayName = membership.DisplayName,
        Role = membership.Role,
        Status = membership.Status,
        InvitedAt = membership.InvitedAt,
        JoinedAt = membership.JoinedAt,
        LastSeenAt = membership.LastSeenAt
    };

    private static TenantIdentityModel MapIdentity(TenantIdentityMapping mapping) => new()
    {
        Id = mapping.Id,
        TenantId = mapping.TenantId,
        IsolationMode = mapping.IsolationMode,
        ProvisioningStatus = mapping.ProvisioningStatus,
        RealmName = mapping.RealmName,
        ClientId = mapping.ClientId,
        IssuerUrl = mapping.IssuerUrl,
        LastError = mapping.LastError,
        UpdatedAt = mapping.UpdatedAt
    };

    private static AuditEventModel MapAudit(AuditEvent auditEvent) => new()
    {
        Id = auditEvent.Id,
        TenantId = auditEvent.TenantId,
        ScopeType = auditEvent.ScopeType,
        Action = auditEvent.Action,
        Result = auditEvent.Result,
        ActorUserId = auditEvent.ActorUserId,
        ActorDisplayName = auditEvent.ActorDisplayName,
        TargetEntityType = auditEvent.TargetEntityType,
        TargetEntityId = auditEvent.TargetEntityId,
        MetadataJson = auditEvent.MetadataJson,
        CreatedAt = auditEvent.CreatedAt
    };

    private static IReadOnlyList<string> ParseStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static DateTime StartOfMonth() => new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

    private static string NormalizeRequired(string? value, string message, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(message);
        }

        return normalized.Length > maxLength ? normalized[..maxLength] : normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return normalized.Length > maxLength ? normalized[..maxLength] : normalized;
    }
}
