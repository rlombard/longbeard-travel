using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Models.Platform;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services.Platform;

public sealed class SessionBootstrapService(
    IRequestActorContext requestActorContext,
    ITenantPlatformRepository tenantPlatformRepository,
    ITenantExecutionContextAccessor tenantExecutionContextAccessor,
    IKeycloakProvisioningService keycloakProvisioningService,
    IPlatformIdentitySettingsAccessor platformIdentitySettingsAccessor,
    ISignupSettingsAccessor signupSettingsAccessor) : ISessionBootstrapService
{
    public async Task<SessionBootstrapModel> GetBootstrapAsync(CancellationToken cancellationToken = default)
    {
        var deploymentMode = tenantExecutionContextAccessor.DeploymentMode;
        var managementAuth = new AuthTargetModel
        {
            KeycloakUrl = platformIdentitySettingsAccessor.PublicKeycloakBaseUrl,
            Realm = platformIdentitySettingsAccessor.ManagementRealm,
            ClientId = platformIdentitySettingsAccessor.ManagementClientId
        };

        AuthTargetModel? standaloneAuth = null;
        if (deploymentMode == DeploymentMode.Standalone)
        {
            var standaloneTenant = await tenantPlatformRepository.GetStandaloneTenantAsync(cancellationToken);
            if (standaloneTenant is not null)
            {
                standaloneAuth = await BuildTenantAuthAsync(standaloneTenant, cancellationToken);
            }
        }

        return new SessionBootstrapModel
        {
            DeploymentMode = deploymentMode,
            PlatformManagementEnabled = deploymentMode == DeploymentMode.SaaS,
            PublicSignupEnabled = signupSettingsAccessor.IsEnabled && (deploymentMode == DeploymentMode.SaaS || signupSettingsAccessor.AllowInStandalone),
            PublicSignupDisabledReason = signupSettingsAccessor.IsEnabled
                ? (deploymentMode == DeploymentMode.SaaS || signupSettingsAccessor.AllowInStandalone ? string.Empty : "Signup disabled in standalone mode.")
                : "Signup is disabled.",
            ManagementAuth = managementAuth,
            StandaloneTenantAuth = standaloneAuth,
            Session = await BuildSessionActorAsync(deploymentMode, cancellationToken)
        };
    }

    public async Task<TenantLoginDiscoveryModel> DiscoverTenantAsync(DiscoverTenantLoginModel model, CancellationToken cancellationToken = default)
    {
        var tenant = await ResolveTenantAsync(model, cancellationToken);
        if (tenant is null)
        {
            return new TenantLoginDiscoveryModel
            {
                Found = false,
                ResolutionSource = "none"
            };
        }

        var auth = await BuildTenantAuthAsync(tenant, cancellationToken);
        return new TenantLoginDiscoveryModel
        {
            Found = true,
            TenantId = tenant.Id,
            TenantSlug = tenant.Slug,
            TenantName = tenant.Name,
            ResolutionSource = !string.IsNullOrWhiteSpace(model.TenantSlug) ? "tenant-slug" : "email-domain",
            Auth = auth
        };
    }

    private async Task<SessionActorModel?> BuildSessionActorAsync(DeploymentMode deploymentMode, CancellationToken cancellationToken)
    {
        var userId = requestActorContext.GetUserIdOrNull();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var memberships = await tenantPlatformRepository.GetActiveMembershipsByUserIdAsync(userId, cancellationToken);
        var membershipModels = new List<SessionTenantMembershipModel>(memberships.Count);

        foreach (var membership in memberships)
        {
            var tenant = await tenantPlatformRepository.GetTenantByIdAsync(membership.TenantId, cancellationToken);
            if (tenant is null)
            {
                continue;
            }

            var identity = (await tenantPlatformRepository.GetIdentityMappingsAsync(tenant.Id, cancellationToken)).FirstOrDefault();
            membershipModels.Add(new SessionTenantMembershipModel
            {
                TenantId = tenant.Id,
                TenantSlug = tenant.Slug,
                TenantName = tenant.Name,
                Role = membership.Role,
                RealmName = identity?.RealmName ?? $"{platformIdentitySettingsAccessor.TenantRealmPrefix}{tenant.Slug}"
            });
        }

        var currentTenantId = tenantExecutionContextAccessor.CurrentTenantId;
        if (!currentTenantId.HasValue && deploymentMode == DeploymentMode.Standalone)
        {
            currentTenantId = (await tenantPlatformRepository.GetStandaloneTenantAsync(cancellationToken))?.Id;
        }

        var currentTenant = currentTenantId.HasValue
            ? await tenantPlatformRepository.GetTenantByIdAsync(currentTenantId.Value, cancellationToken)
            : null;

        return new SessionActorModel
        {
            IsAuthenticated = true,
            IsPlatformAdmin = requestActorContext.IsPlatformAdmin() && deploymentMode == DeploymentMode.SaaS,
            UserId = userId,
            DisplayName = requestActorContext.GetDisplayNameOrNull(),
            Email = requestActorContext.GetEmailOrNull(),
            CurrentTenantId = currentTenant?.Id,
            CurrentTenantSlug = currentTenant?.Slug,
            CurrentTenantName = currentTenant?.Name,
            HomeArea = requestActorContext.IsPlatformAdmin() && deploymentMode == DeploymentMode.SaaS ? "/platform/tenants" : "/app",
            Memberships = membershipModels
        };
    }

    private async Task<Tenant?> ResolveTenantAsync(DiscoverTenantLoginModel model, CancellationToken cancellationToken)
    {
        if (tenantExecutionContextAccessor.DeploymentMode == DeploymentMode.Standalone)
        {
            return await tenantPlatformRepository.GetStandaloneTenantAsync(cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(model.TenantSlug))
        {
            return await tenantPlatformRepository.GetTenantBySlugAsync(model.TenantSlug.Trim().ToLowerInvariant(), cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains('@'))
        {
            return null;
        }

        var domain = model.Email[(model.Email.LastIndexOf('@') + 1)..].Trim().ToLowerInvariant();
        var tenants = await tenantPlatformRepository.GetTenantsAsync(cancellationToken);
        return tenants.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(x.BillingEmail)
            && x.BillingEmail.Contains('@')
            && string.Equals(x.BillingEmail[(x.BillingEmail.LastIndexOf('@') + 1)..].Trim(), domain, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<AuthTargetModel> BuildTenantAuthAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        var existingIdentity = (await tenantPlatformRepository.GetIdentityMappingsAsync(tenant.Id, cancellationToken)).FirstOrDefault();
        TenantIdentityModel identity;

        if (existingIdentity is null || existingIdentity.ProvisioningStatus != IdentityProvisioningStatus.Ready)
        {
            identity = await keycloakProvisioningService.EnsureTenantIdentityAsync(tenant.Id, cancellationToken);
        }
        else
        {
            identity = new TenantIdentityModel
            {
                Id = existingIdentity.Id,
                TenantId = existingIdentity.TenantId,
                IsolationMode = existingIdentity.IsolationMode,
                ProvisioningStatus = existingIdentity.ProvisioningStatus,
                RealmName = existingIdentity.RealmName,
                ClientId = existingIdentity.ClientId,
                IssuerUrl = existingIdentity.IssuerUrl,
                LastError = existingIdentity.LastError,
                UpdatedAt = existingIdentity.UpdatedAt
            };
        }

        if (identity.ProvisioningStatus != IdentityProvisioningStatus.Ready)
        {
            throw new InvalidOperationException(identity.LastError ?? "Tenant identity provisioning is not ready.");
        }

        return new AuthTargetModel
        {
            KeycloakUrl = platformIdentitySettingsAccessor.PublicKeycloakBaseUrl,
            Realm = identity.RealmName,
            ClientId = identity.ClientId ?? platformIdentitySettingsAccessor.TenantClientId
        };
    }
}
