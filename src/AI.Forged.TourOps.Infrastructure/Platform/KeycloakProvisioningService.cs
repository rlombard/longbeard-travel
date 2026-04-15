using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Models.Platform;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Platform;

public sealed class KeycloakProvisioningService(
    ITenantPlatformRepository tenantPlatformRepository,
    IKeycloakRealmAdminRepository keycloakRealmAdminRepository,
    IOptions<KeycloakTenantProvisioningSettings> settings) : IKeycloakProvisioningService
{
    public async Task<TenantIdentityModel> EnsureTenantIdentityAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await tenantPlatformRepository.GetTenantByIdAsync(tenantId, cancellationToken)
            ?? throw new InvalidOperationException("Tenant not found.");
        var existing = (await tenantPlatformRepository.GetIdentityMappingsAsync(tenantId, cancellationToken)).FirstOrDefault();
        var now = DateTime.UtcNow;

        var mapping = existing ?? new TenantIdentityMapping
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CreatedAt = now
        };

        mapping.IsolationMode = settings.Value.IsolationMode;
        mapping.RealmName = mapping.IsolationMode == IdentityIsolationMode.RealmPerTenant
            ? $"{settings.Value.TenantRealmPrefix}{tenant.Slug}"
            : settings.Value.ManagementRealm;
        mapping.ClientId = mapping.IsolationMode == IdentityIsolationMode.RealmPerTenant
            ? settings.Value.TenantClientId
            : settings.Value.ManagementClientId;
        mapping.IssuerUrl = $"{settings.Value.PublicBaseUrl.TrimEnd('/')}/realms/{mapping.RealmName}";
        mapping.UpdatedAt = now;

        try
        {
            if (mapping.IsolationMode == IdentityIsolationMode.RealmPerTenant)
            {
                await keycloakRealmAdminRepository.EnsureRealmAsync(
                    mapping.RealmName,
                    tenant.Name,
                    settings.Value.TenantClientId,
                    settings.Value.FrontendRootUrl,
                    settings.Value.RedirectUris,
                    settings.Value.WebOrigins,
                    cancellationToken);
            }

            mapping.ProvisioningStatus = IdentityProvisioningStatus.Ready;
            mapping.LastError = null;
        }
        catch (Exception ex)
        {
            mapping.ProvisioningStatus = IdentityProvisioningStatus.Failed;
            mapping.LastError = ex.Message;
        }

        mapping.MetadataJson = JsonSerializer.Serialize(new
        {
            tenantSlug = tenant.Slug,
            managementRealm = settings.Value.ManagementRealm,
            tenantRealm = mapping.RealmName,
            mode = mapping.IsolationMode.ToString()
        });

        await tenantPlatformRepository.UpsertIdentityMappingAsync(mapping, cancellationToken);

        return new TenantIdentityModel
        {
            Id = mapping.Id,
            TenantId = tenantId,
            IsolationMode = mapping.IsolationMode,
            ProvisioningStatus = mapping.ProvisioningStatus,
            RealmName = mapping.RealmName,
            ClientId = mapping.ClientId,
            IssuerUrl = mapping.IssuerUrl,
            LastError = mapping.LastError,
            UpdatedAt = mapping.UpdatedAt
        };
    }
}
