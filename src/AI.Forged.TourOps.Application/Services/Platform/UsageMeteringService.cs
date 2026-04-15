using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Models.Platform;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services.Platform;

public sealed class UsageMeteringService(
    ITenantPlatformRepository tenantPlatformRepository,
    ITenantExecutionContextAccessor tenantExecutionContextAccessor) : IUsageMeteringService
{
    public async Task RecordAsync(MeterUsageModel model, CancellationToken cancellationToken = default)
    {
        var tenantId = tenantExecutionContextAccessor.CurrentTenantId;
        if (!tenantId.HasValue)
        {
            if (tenantExecutionContextAccessor.DeploymentMode == DeploymentMode.Standalone)
            {
                var tenant = await tenantPlatformRepository.GetStandaloneTenantAsync(cancellationToken)
                    ?? throw new InvalidOperationException("Standalone tenant is not configured.");
                tenantId = tenant.Id;
            }
            else
            {
                throw new InvalidOperationException("Tenant context is required for usage metering.");
            }
        }

        await tenantPlatformRepository.AddUsageAsync(new UsageRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            Category = model.Category.Trim(),
            MetricKey = model.MetricKey.Trim(),
            Quantity = model.Quantity <= 0 ? 1 : model.Quantity,
            Unit = string.IsNullOrWhiteSpace(model.Unit) ? "count" : model.Unit.Trim(),
            IsBillable = model.IsBillable,
            Source = NormalizeOptional(model.Source, 128),
            ReferenceEntityType = NormalizeOptional(model.ReferenceEntityType, 128),
            ReferenceEntityId = model.ReferenceEntityId,
            MetadataJson = NormalizeOptional(model.MetadataJson, 8000),
            OccurredAt = model.OccurredAt ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
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
