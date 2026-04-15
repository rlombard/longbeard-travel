using System.Security.Claims;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Api.Middleware;

public sealed class TenantResolutionMiddleware(
    RequestDelegate next,
    IOptions<DeploymentSettings> deploymentSettings,
    IOptions<TenantResolutionSettings> tenantResolutionSettings)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantExecutionContextAccessor tenantExecutionContextAccessor,
        ITenantPlatformRepository tenantPlatformRepository,
        IRequestActorContext requestActorContext)
    {
        IDisposable? scope = null;

        try
        {
            if (deploymentSettings.Value.Mode == DeploymentMode.Standalone)
            {
                scope = tenantExecutionContextAccessor.BeginTenantScope(tenantResolutionSettings.Value.StandaloneTenantId);
            }
            else if (context.User.Identity?.IsAuthenticated == true)
            {
                var resolvedTenantId = await ResolveFromIssuerAsync(context.User, tenantPlatformRepository, context.RequestAborted)
                    ?? ResolveFromClaims(context.User)
                    ?? ResolveFromAdminHeader(context, requestActorContext)
                    ?? await ResolveFromMembershipsAsync(requestActorContext, tenantPlatformRepository, context.RequestAborted);

                if (resolvedTenantId.HasValue)
                {
                    scope = tenantExecutionContextAccessor.BeginTenantScope(resolvedTenantId.Value);
                }
            }

            await next(context);
        }
        finally
        {
            scope?.Dispose();
        }
    }

    private Guid? ResolveFromClaims(ClaimsPrincipal user)
    {
        var claimValue = user.FindFirstValue("tenant_id")
            ?? user.FindFirstValue("tenant")
            ?? user.FindFirstValue("agency_id");

        return Guid.TryParse(claimValue, out var tenantId) ? tenantId : null;
    }

    private static async Task<Guid?> ResolveFromIssuerAsync(
        ClaimsPrincipal user,
        ITenantPlatformRepository tenantPlatformRepository,
        CancellationToken cancellationToken)
    {
        var issuer = user.FindFirstValue("iss");
        if (string.IsNullOrWhiteSpace(issuer))
        {
            return null;
        }

        var tenant = await tenantPlatformRepository.GetTenantByIssuerAsync(issuer, cancellationToken);
        return tenant?.Id;
    }

    private Guid? ResolveFromAdminHeader(HttpContext context, IRequestActorContext requestActorContext)
    {
        if (!requestActorContext.IsPlatformAdmin())
        {
            return null;
        }

        return context.Request.Headers.TryGetValue(tenantResolutionSettings.Value.HeaderName, out var values)
               && Guid.TryParse(values.ToString(), out var tenantId)
            ? tenantId
            : null;
    }

    private static async Task<Guid?> ResolveFromMembershipsAsync(
        IRequestActorContext requestActorContext,
        ITenantPlatformRepository tenantPlatformRepository,
        CancellationToken cancellationToken)
    {
        var userId = requestActorContext.GetUserIdOrNull();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var memberships = await tenantPlatformRepository.GetActiveMembershipsByUserIdAsync(userId, cancellationToken);
        return memberships.Count == 1 ? memberships[0].TenantId : null;
    }
}
