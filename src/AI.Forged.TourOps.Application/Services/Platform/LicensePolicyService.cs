using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Models.Platform;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services.Platform;

public sealed class LicensePolicyService(
    ITenantPlatformRepository tenantPlatformRepository,
    ITenantExecutionContextAccessor tenantExecutionContextAccessor,
    IAuditService? auditService = null) : ILicensePolicyService
{
    public async Task<FeatureAccessResultModel> EvaluateAsync(string featureKey, CancellationToken cancellationToken = default)
    {
        var tenantId = await ResolveTenantIdAsync(cancellationToken);
        var license = await tenantPlatformRepository.GetLicenseAsync(tenantId, cancellationToken);
        if (license is null)
        {
            return new FeatureAccessResultModel
            {
                TenantId = tenantId,
                FeatureKey = featureKey,
                Allowed = false,
                Reason = "No tenant license is configured."
            };
        }

        var plan = await tenantPlatformRepository.GetLicensePlanByCodeAsync(license.LicensePlan.Code, cancellationToken)
            ?? license.LicensePlan;

        var featureSet = ParseFeatures(plan.IncludedFeaturesJson)
            .Union(ParseFeatures(license.FeatureOverridesJson), StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var limits = await BuildLimitMapAsync(tenantId, plan, license, cancellationToken);
        var currentUsage = await BuildCurrentUsageMapAsync(tenantId, cancellationToken);

        if (license.Status is LicenseStatus.Suspended or LicenseStatus.Expired or LicenseStatus.Cancelled)
        {
            return CreateResult(false, $"License state '{license.Status}' blocks feature access.");
        }

        if (!featureSet.Contains(featureKey))
        {
            return CreateResult(false, $"Plan '{plan.Code}' does not include feature '{featureKey}'.");
        }

        if (TryGetLimitMetric(featureKey, out var metricKey)
            && limits.TryGetValue(metricKey, out var limit)
            && limit >= 0
            && await GetCurrentUsageForMetricAsync(tenantId, metricKey, cancellationToken) >= limit)
        {
            return CreateResult(false, $"Plan limit reached for '{metricKey}'.");
        }

        return CreateResult(true, "Allowed.");

        FeatureAccessResultModel CreateResult(bool allowed, string reason) => new()
        {
            TenantId = tenantId,
            FeatureKey = featureKey,
            Allowed = allowed,
            Reason = reason,
            PlanCode = plan.Code,
            LicenseStatus = license.Status,
            Limits = limits,
            CurrentUsage = currentUsage
        };
    }

    public async Task AssertAllowedAsync(string featureKey, CancellationToken cancellationToken = default)
    {
        var result = await EvaluateAsync(featureKey, cancellationToken);
        if (!result.Allowed)
        {
            if (auditService is not null)
            {
                await auditService.WriteAsync(new AuditEventCreateModel
                {
                    TenantId = result.TenantId,
                    ScopeType = "License",
                    Action = "FeatureDenied",
                    Result = "Denied",
                    MetadataJson = JsonSerializer.Serialize(new
                    {
                        result.FeatureKey,
                        result.Reason,
                        result.PlanCode,
                        result.LicenseStatus
                    })
                }, cancellationToken);
            }

            throw new InvalidOperationException(result.Reason);
        }
    }

    public async Task<TenantLicenseModel?> GetCurrentLicenseAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = await ResolveTenantIdAsync(cancellationToken);
        var license = await tenantPlatformRepository.GetLicenseAsync(tenantId, cancellationToken);
        if (license is null)
        {
            return null;
        }

        var plan = license.LicensePlan;
        return new TenantLicenseModel
        {
            Id = license.Id,
            TenantId = tenantId,
            LicensePlanId = plan.Id,
            PlanCode = plan.Code,
            PlanName = plan.Name,
            Status = license.Status,
            BillingMode = license.BillingMode,
            StartsAt = license.StartsAt,
            TrialEndsAt = license.TrialEndsAt,
            EndsAt = license.EndsAt,
            IncludedFeatures = ParseFeatures(plan.IncludedFeaturesJson)
                .Union(ParseFeatures(license.FeatureOverridesJson), StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Limits = await BuildLimitMapAsync(tenantId, plan, license, cancellationToken),
            CurrentUsage = await BuildCurrentUsageMapAsync(tenantId, cancellationToken)
        };
    }

    private async Task<Guid> ResolveTenantIdAsync(CancellationToken cancellationToken)
    {
        if (tenantExecutionContextAccessor.CurrentTenantId.HasValue)
        {
            return tenantExecutionContextAccessor.CurrentTenantId.Value;
        }

        if (tenantExecutionContextAccessor.DeploymentMode == DeploymentMode.Standalone)
        {
            var tenant = await tenantPlatformRepository.GetStandaloneTenantAsync(cancellationToken)
                ?? throw new InvalidOperationException("Standalone tenant is not configured.");
            return tenant.Id;
        }

        throw new InvalidOperationException("Tenant context is required.");
    }

    private async Task<Dictionary<string, int>> BuildLimitMapAsync(Guid tenantId, LicensePlan plan, TenantLicense license, CancellationToken cancellationToken)
    {
        _ = tenantId;
        _ = cancellationToken;

        return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["users.active"] = license.MaxUsersOverride ?? plan.MaxUsers,
            ["integrations.active"] = license.MaxIntegrationsOverride ?? plan.MaxIntegrations,
            ["email.accounts"] = license.MaxEmailAccountsOverride ?? plan.MaxEmailAccounts,
            ["ai.jobs.monthly"] = license.MaxMonthlyAiJobsOverride ?? plan.MaxMonthlyAiJobs,
            ["email.sends.monthly"] = license.MaxMonthlyEmailSendsOverride ?? plan.MaxMonthlyEmailSends,
            ["email.sync.monthly"] = license.MaxMonthlySyncOperationsOverride ?? plan.MaxMonthlySyncOperations,
            ["storage.mb"] = license.MaxStorageMbOverride ?? plan.MaxStorageMb
        };
    }

    private async Task<int> GetCurrentUsageForMetricAsync(Guid tenantId, string metricKey, CancellationToken cancellationToken)
    {
        var windowStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return await tenantPlatformRepository.CountUsageAsync(tenantId, metricKey, windowStart, cancellationToken);
    }

    private async Task<Dictionary<string, int>> BuildCurrentUsageMapAsync(Guid tenantId, CancellationToken cancellationToken) => new()
    {
        ["users.active"] = await tenantPlatformRepository.CountTenantUsersAsync(tenantId, cancellationToken),
        ["email.accounts"] = await tenantPlatformRepository.CountTenantConnectionsAsync(tenantId, cancellationToken),
        ["ai.jobs.monthly"] = await GetCurrentUsageForMetricAsync(tenantId, "ai.jobs.monthly", cancellationToken),
        ["email.sends.monthly"] = await GetCurrentUsageForMetricAsync(tenantId, "email.sends.monthly", cancellationToken),
        ["email.sync.monthly"] = await GetCurrentUsageForMetricAsync(tenantId, "email.sync.monthly", cancellationToken)
    };

    private static bool TryGetLimitMetric(string featureKey, out string metricKey)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["tenant.users.manage"] = "users.active",
            ["email.integrations.manage"] = "email.accounts",
            ["email.sync"] = "email.sync.monthly",
            ["email.send"] = "email.sends.monthly",
            ["ai.itinerary"] = "ai.jobs.monthly",
            ["ai.email"] = "ai.jobs.monthly",
            ["ai.task-suggestions"] = "ai.jobs.monthly"
        };

        return map.TryGetValue(featureKey, out metricKey!);
    }

    private static IReadOnlyList<string> ParseFeatures(string? json)
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
}
