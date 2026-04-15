using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Models.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/platform")]
public sealed class PlatformController(ITenantPlatformService tenantPlatformService) : ControllerBase
{
    [HttpGet("tenants")]
    public async Task<ActionResult<IReadOnlyList<TenantSummaryResponse>>> GetTenants(CancellationToken cancellationToken)
    {
        var tenants = await tenantPlatformService.GetTenantsAsync(cancellationToken);
        return Ok(tenants.Select(x => x.ToResponse()).ToList());
    }

    [HttpGet("tenants/{tenantId:guid}")]
    public async Task<ActionResult<TenantDetailResponse>> GetTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await tenantPlatformService.GetTenantAsync(tenantId, cancellationToken);
        return tenant is null ? NotFound() : Ok(tenant.ToResponse());
    }

    [HttpPost("tenants")]
    public async Task<ActionResult<TenantDetailResponse>> CreateTenant([FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
    {
        var tenant = await tenantPlatformService.CreateTenantAsync(request.ToModel(), cancellationToken);
        return CreatedAtAction(nameof(GetTenant), new { tenantId = tenant.Tenant.Id }, tenant.ToResponse());
    }

    [HttpPatch("tenants/{tenantId:guid}/onboarding")]
    public async Task<ActionResult<TenantOnboardingResponse>> UpdateOnboarding(Guid tenantId, [FromBody] UpdateTenantOnboardingRequest request, CancellationToken cancellationToken)
    {
        var onboarding = await tenantPlatformService.UpdateOnboardingAsync(tenantId, request.ToModel(), cancellationToken);
        return Ok(onboarding.ToResponse());
    }

    [HttpPut("tenants/{tenantId:guid}/config")]
    public async Task<ActionResult<TenantConfigEntryResponse>> UpsertConfig(Guid tenantId, [FromBody] UpsertTenantConfigRequest request, CancellationToken cancellationToken)
    {
        var entry = await tenantPlatformService.UpsertConfigAsync(tenantId, request.ToModel(), cancellationToken);
        return Ok(entry.ToResponse());
    }

    [HttpPost("tenants/{tenantId:guid}/users")]
    public async Task<ActionResult<TenantUserMembershipResponse>> AssignUser(Guid tenantId, [FromBody] AssignTenantUserRequest request, CancellationToken cancellationToken)
    {
        var membership = await tenantPlatformService.AssignUserAsync(new AssignTenantUserModel
        {
            TenantId = tenantId,
            UserId = request.UserId,
            Email = request.Email,
            DisplayName = request.DisplayName,
            Role = request.Role
        }, cancellationToken);

        return Ok(membership.ToResponse());
    }

    [HttpGet("signup-sessions")]
    public async Task<ActionResult<IReadOnlyList<SignupSessionSummaryResponse>>> GetSignupSessions([FromServices] ISignupOnboardingService signupOnboardingService, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var sessions = await signupOnboardingService.GetAdminSessionsAsync(take, cancellationToken);
        return Ok(sessions.Select(x => x.ToResponse()).ToList());
    }

    [HttpPost("signup-sessions/{sessionId:guid}/retry")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> RetrySignupSession(Guid sessionId, [FromServices] ISignupOnboardingService signupOnboardingService, CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.RetryProvisioningAsync(sessionId, cancellationToken);
        return Ok(session.ToResponse());
    }

    [HttpPost("signup-sessions/{sessionId:guid}/confirm-billing")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> ConfirmSignupBilling(Guid sessionId, [FromServices] ISignupOnboardingService signupOnboardingService, CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.ConfirmBillingForAdminAsync(sessionId, cancellationToken);
        return Ok(session.ToResponse());
    }
}
