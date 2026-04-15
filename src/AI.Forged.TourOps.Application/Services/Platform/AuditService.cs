using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Models.Platform;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Services.Platform;

public sealed class AuditService(
    ITenantPlatformRepository tenantPlatformRepository,
    IRequestActorContext requestActorContext) : IAuditService
{
    public async Task WriteAsync(AuditEventCreateModel model, CancellationToken cancellationToken = default)
    {
        await tenantPlatformRepository.AddAuditEventAsync(new AuditEvent
        {
            Id = Guid.NewGuid(),
            TenantId = model.TenantId,
            ScopeType = model.ScopeType.Trim(),
            Action = model.Action.Trim(),
            Result = model.Result.Trim(),
            ActorUserId = requestActorContext.GetUserIdOrNull(),
            ActorDisplayName = requestActorContext.GetDisplayNameOrNull() ?? requestActorContext.GetEmailOrNull(),
            TargetEntityType = NormalizeOptional(model.TargetEntityType, 128),
            TargetEntityId = model.TargetEntityId,
            IpAddress = NormalizeOptional(model.IpAddress, 128),
            MetadataJson = NormalizeOptional(model.MetadataJson, 8000),
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
