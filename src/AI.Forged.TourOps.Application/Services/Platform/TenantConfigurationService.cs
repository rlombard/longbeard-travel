using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Models.Platform;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Services.Platform;

public sealed class TenantConfigurationService(
    ITenantPlatformRepository tenantPlatformRepository,
    ICurrentUserContext currentUserContext) : ITenantConfigurationService
{
    public async Task<IReadOnlyList<TenantConfigEntryModel>> GetAsync(Guid tenantId, string? configDomain, CancellationToken cancellationToken = default)
    {
        var entries = await tenantPlatformRepository.GetConfigEntriesAsync(tenantId, configDomain, cancellationToken);
        return entries
            .OrderBy(x => x.ConfigDomain, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.ConfigKey, StringComparer.OrdinalIgnoreCase)
            .Select(Map)
            .ToList();
    }

    public async Task<TenantConfigEntryModel> UpsertAsync(Guid tenantId, UpsertTenantConfigModel model, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var existing = await tenantPlatformRepository.GetConfigEntryAsync(tenantId, model.ConfigDomain.Trim(), model.ConfigKey.Trim(), cancellationToken);
        var entry = existing ?? new TenantConfigEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CreatedAt = now
        };

        entry.ConfigDomain = model.ConfigDomain.Trim();
        entry.ConfigKey = model.ConfigKey.Trim();
        entry.JsonValue = model.JsonValue.Trim();
        entry.IsEncrypted = model.IsEncrypted;
        entry.UpdatedByUserId = currentUserContext.GetRequiredUserId();
        entry.UpdatedAt = now;

        await tenantPlatformRepository.UpsertConfigEntryAsync(entry, cancellationToken);
        return Map(entry);
    }

    private static TenantConfigEntryModel Map(TenantConfigEntry entry) => new()
    {
        Id = entry.Id,
        TenantId = entry.TenantId,
        ConfigDomain = entry.ConfigDomain,
        ConfigKey = entry.ConfigKey,
        JsonValue = entry.JsonValue,
        IsEncrypted = entry.IsEncrypted,
        UpdatedByUserId = entry.UpdatedByUserId,
        UpdatedAt = entry.UpdatedAt
    };
}
